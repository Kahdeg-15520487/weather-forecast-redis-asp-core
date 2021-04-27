using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

using Newtonsoft.Json;

using research_redis_key_pattern.core.DataDto;

using research_redis_key_pattern_api.Services.Contracts;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace research_redis_key_pattern_api.Services
{
    public class RemoteDataService : IRemoteDataService
    {
        /*
         * tenant: tenant:{tenantId}
         * segment: tenant:{tenantId}:segment:{segmentId}
         * segmentUpdate: tenant:{tenantId}:update:{updateId}
         * inspection: tenant:{tenantId}:inspection:{inspectionId}
         *
         * relation ship:
         * tenant: 1-n :segment
         * segment: n-n :segmentUpdate
         * segmentUpdate: 1-n :inspection
         * segment: n-n : inspection
         *
         * redis mapping
         * tenant: 1-n :segment
         *  tenant:{tenantId}:segment
         *
         * segment: n-n :segmentUpdate
         *  segment:{segmentId}:update
         *  update:{updateId}:segment
         *
         * segmentUpdate: 1-n :inspection
         *  update:{updateId}:inspection
         * segment: n-n : inspection
         *  segment:{segmentId}:inspection
         *  inspection:{inspectionId}:segment
         */

        static readonly string tenantCacheKeyTemplate = "tenant:{0}";
        static readonly string segmentCacheKeyTemplate = "tenant:{0}:segment:{1}";
        static readonly string segmentUpdateCacheKeyTemplate = "tenant:{0}:update:{1}";

        static readonly string tenant_to_SegmentSetCacheKeyTemplate = "tenant:{0}:segment";
        static readonly string segment_To_segmentUpdateSetCacheKeyTemplate = "segment:{0}:update";
        static readonly string segmentUpdate_To_SegmentSetCacheKeyTemplate = "update:{0}:segment";

        private readonly IDistributedCacheWrapper distributedCacheWrapper;
        private readonly HttpClient httpClient;
        private readonly IHttpClientWrapper httpClientWrapper;
        private readonly ILogger<RemoteDataService> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="RADataService"/> class.
        /// </summary>
        /// <param name="distributedCacheWrapper">Inject <see cref="IDistributedCache"/>.</param>
        /// <param name="httpClientWrapper">Inject <see cref="IHttpClientWrapper"/>.</param>
        /// <param name="logger">Inject <see cref="ILogger{RADataService}"/>.</param>
        public RemoteDataService(IDistributedCacheWrapper distributedCacheWrapper, IHttpClientFactory httpClientFactory, IHttpClientWrapper httpClientWrapper, ILogger<RemoteDataService> logger)
        {
            this.distributedCacheWrapper = distributedCacheWrapper;
            this.httpClientWrapper = httpClientWrapper;
            this.httpClient = httpClientFactory.CreateClient("remote");
            this.logger = logger;
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<Segment>> GetSegments(Guid tenantId)
        {
            await this.CheckCache(tenantId);
            string tenant_To_SegmentCacheKey = string.Format(tenant_to_SegmentSetCacheKeyTemplate, tenantId);
            IEnumerable<string> segmentCacheKeys = (await this.distributedCacheWrapper.SetGet(tenant_To_SegmentCacheKey)).Select(s => string.Format(segmentCacheKeyTemplate, tenantId, s)).ToList();
            return await this.distributedCacheWrapper.GetStringManyAsync(segmentCacheKeys.ToArray()).Select(s => JsonConvert.DeserializeObject<Segment>(s)).ToListAsync();
        }

        /// <inheritdoc/>
        public async Task<Segment> GetSegment(Guid tenantId, Guid segmentId)
        {
            await this.CheckCache(tenantId);
            var segmentCacheKey = string.Format(segmentCacheKeyTemplate, tenantId, segmentId);
            return JsonConvert.DeserializeObject<Segment>(await distributedCacheWrapper.GetStringAsync(segmentCacheKey));
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<SegmentUpdate>> GetSegmentUpdatesBySegment(Guid tenantId, Guid segmentId)
        {
            await this.CheckCache(tenantId);
            var segment_To_segmentUpdateCacheKey = string.Format(segment_To_segmentUpdateSetCacheKeyTemplate, segmentId);
            IEnumerable<string> segmentUpdateCacheKeys =
                (await this.distributedCacheWrapper.SetGet(segment_To_segmentUpdateCacheKey))
                                                   .Select(u => string.Format(segmentUpdateCacheKeyTemplate, tenantId, u))
                                                   .ToList();
            logger.LogInformation(JsonConvert.SerializeObject(segmentUpdateCacheKeys));
            return await this.distributedCacheWrapper.GetStringManyAsync(segmentUpdateCacheKeys.ToArray())
                                                     .Select(u => JsonConvert.DeserializeObject<SegmentUpdate>(u))
                                                     .ToListAsync();
        }

        /// <inheritdoc/>
        public async Task<SegmentUpdate> GetSegmentUpdateByTenant(Guid tenantId, Guid updateId)
        {
            await this.CheckCache(tenantId);
            var segmentUpdateCacheKey = string.Format(segmentUpdateCacheKeyTemplate, tenantId, updateId);
            return JsonConvert.DeserializeObject<SegmentUpdate>(await distributedCacheWrapper.GetStringAsync(segmentUpdateCacheKey));
        }

        private async Task CheckCache(Guid tenantId)
        {
            var tck = string.Format(tenantCacheKeyTemplate, tenantId);
            var tenant = await distributedCacheWrapper.GetStringAsync(tck);
            if (string.IsNullOrEmpty(tenant))
            {
                logger.LogInformation("updating cache...");
                await UpdateCache();
            }
            else
            {
                logger.LogInformation("fetching from cache...");
            }
        }

        private async Task UpdateCache()
        {
            var data = await httpClientWrapper.GetStringFromApiAsync<AllData>(httpClient, "api/data");
            StringBuilder sb = new StringBuilder();
            foreach (var segment in data.Segments)
            {
                var segmentCacheKey = string.Format(segmentCacheKeyTemplate, segment.TenantId, segment.SegmentId);
                var tenantCacheKey = string.Format(tenantCacheKeyTemplate, segment.TenantId);
                var tenant_To_SegmentCacheKey = string.Format(tenant_to_SegmentSetCacheKeyTemplate, segment.TenantId);
                var segment_To_segmentUpdateCacheKey = string.Format(segment_To_segmentUpdateSetCacheKeyTemplate, segment.SegmentId);

                sb.AppendFormat("segment: {0}\r\n", segmentCacheKey);
                sb.AppendFormat("tenant: {0}\r\n", tenantCacheKey);
                sb.AppendFormat("tenant-segment: {0}\r\n", tenant_To_SegmentCacheKey);
                sb.AppendFormat("segment-update: {0}\r\n", segment_To_segmentUpdateCacheKey);

                await distributedCacheWrapper.SetStringAsync(tenantCacheKey, segment.TenantId.ToString());
                await distributedCacheWrapper.SetStringAsync(segmentCacheKey, JsonConvert.SerializeObject(segment));
                await distributedCacheWrapper.SetAdd(tenant_To_SegmentCacheKey, segment.SegmentId.ToString());
                await distributedCacheWrapper.SetAdd(segment_To_segmentUpdateCacheKey, segment.SegmentUpdateIds.Select(suid => suid.ToString()).ToArray());

                foreach (var update in data.Updates.Where(u => segment.SegmentUpdateIds.Contains(u.SegmentUpdateId)))
                {
                    var segmentUpdateCacheKey = string.Format(segmentUpdateCacheKeyTemplate, segment.TenantId, update.SegmentUpdateId);
                    var segmentUpdate_To_segmentCacheKey = string.Format(tenant_to_SegmentSetCacheKeyTemplate, update.SegmentUpdateId);
                    sb.AppendFormat("segment: {0}\r\n", segmentCacheKey);
                    sb.AppendFormat("update-segment: {0}\r\n", segmentUpdate_To_segmentCacheKey);
                    await distributedCacheWrapper.SetStringAsync(segmentUpdateCacheKey, JsonConvert.SerializeObject(update));
                    await distributedCacheWrapper.SetAdd(segmentUpdate_To_segmentCacheKey, update.SegmentIds.Select(sid => sid.ToString()).ToArray());
                }
            }
            logger.LogInformation(sb.ToString());
            logger.LogInformation("updated cache");
        }
    }
}
