using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

using Newtonsoft.Json;

using research_redis_key_pattern.core.DataDto;

using research_redis_key_pattern_api.Services.Contracts;

using StackExchange.Redis;

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
        static readonly string segmentCacheKeyTemplate = "segment:{0}";
        static readonly string segmentUpdateCacheKeyTemplate = "segment:{0}:update:{1}";
        static readonly string inspectionCacheKeyTemplate = "inspection:{0}";

        static readonly string tenant_To_segmentSetCacheKeyTemplate = "tenant:{0}:segments";
        static readonly string tenant_To_inspectionSetCacheKeyTemplate = "tenant:{0}:inspections";
        static readonly string tenant_To_segmentUpdateSetCacheKeyTemplate = "tenant:{0}:updates";

        static readonly string segmentUpdate_To_segmentSetCacheKeyTemplate = "update:{0}:segments";
        static readonly string segmentUpdate_To_inspectionSetCacheKeyTemplate = "update:{0}:inspections";

        static readonly string inspection_To_segmentSetCacheKeyTemplate = "inspection:{0}:segments";

        static readonly string segment_To_segmentUpdateSetCacheKeyTemplate = "segment:{0}:updates";
        static readonly string segment_To_inspectionSetCacheKeyTemplate = "segment:{0}:inspections";

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
        public async Task<IEnumerable<Segment>> GetSegmentsByTenant(Guid tenantId)
        {
            await this.CheckCache(tenantId);
            string tenant_To_SegmentCacheKey = string.Format(tenant_To_segmentSetCacheKeyTemplate, tenantId);
            IEnumerable<string> segmentCacheKeys =
                (await this.distributedCacheWrapper.SetGet(tenant_To_SegmentCacheKey))
                                                   .Select(s => string.Format(segmentCacheKeyTemplate, s))
                                                   .ToList();
            return (await this.distributedCacheWrapper.StringGetManyAsync(segmentCacheKeys.ToArray()))
                                                      .Select(s => JsonConvert.DeserializeObject<Segment>(s));
        }

        /// <inheritdoc/>
        public async Task<Segment> GetSegment(Guid tenantId, Guid segmentId)
        {
            await this.CheckCache(tenantId);
            string segmentCacheKey = string.Format(segmentCacheKeyTemplate, segmentId);
            //TODO check relationship tenant to segment
            return JsonConvert.DeserializeObject<Segment>(await distributedCacheWrapper.StringGetAsync(segmentCacheKey));
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<SegmentUpdate>> GetSegmentUpdatesBySegment(Guid tenantId, Guid segmentId)
        {
            await this.CheckCache(tenantId);
            string segment_To_segmentUpdateCacheKey = string.Format(segment_To_segmentUpdateSetCacheKeyTemplate, segmentId);
            IEnumerable<string> segmentUpdateCacheKeys =
                (await this.distributedCacheWrapper.SetGet(segment_To_segmentUpdateCacheKey)).ToList();
            return (await this.distributedCacheWrapper.StringGetManyAsync(segmentUpdateCacheKeys.ToArray()))
                                                      .Select(u => JsonConvert.DeserializeObject<SegmentUpdate>(u));
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<SegmentUpdate>> GetSegmentUpdatesByTenant(Guid tenantId)
        {
            await this.CheckCache(tenantId);
            var tenant_To_segmentUpdateCacheKey = string.Format(tenant_To_segmentUpdateSetCacheKeyTemplate, tenantId);
            IEnumerable<string> segmentUpdateCacheKeys =
                (await this.distributedCacheWrapper.SetGet(tenant_To_segmentUpdateCacheKey)).ToList();
            return (await this.distributedCacheWrapper.StringGetManyAsync(segmentUpdateCacheKeys.ToArray()))
                                                      .Distinct()
                                                      .Select(u => JsonConvert.DeserializeObject<SegmentUpdate>(u));
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<Inspection>> GetInspectionsBySegment(Guid tenantId, Guid segmentId)
        {
            await this.CheckCache(tenantId);
            var segment_To_inspectionCacheKey = string.Format(segment_To_inspectionSetCacheKeyTemplate, segmentId);
            IEnumerable<string> inspectionCacheKeys =
                (await this.distributedCacheWrapper.SetGet(segment_To_inspectionCacheKey))
                                                   .Select(ii => string.Format(inspectionCacheKeyTemplate, ii))
                                                   .ToList();
            return (await this.distributedCacheWrapper.StringGetManyAsync(inspectionCacheKeys.ToArray()))
                                                      .Select(i => JsonConvert.DeserializeObject<Inspection>(i));
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<Inspection>> GetInspectionsByTenant(Guid tenantId)
        {
            await this.CheckCache(tenantId);
            var tenant_To_inspectionCacheKey = string.Format(tenant_To_inspectionSetCacheKeyTemplate, tenantId);
            IEnumerable<string> inspectionCacheKeys =
                (await this.distributedCacheWrapper.SetGet(tenant_To_inspectionCacheKey))
                                                   .Select(ii => string.Format(inspectionCacheKeyTemplate, ii))
                                                   .ToList();
            return (await this.distributedCacheWrapper.StringGetManyAsync(inspectionCacheKeys.ToArray()))
                                                      .Select(i => JsonConvert.DeserializeObject<Inspection>(i));
        }

        private async Task CheckCache(Guid tenantId)
        {
            var tenantCacheKey = string.Format(tenantCacheKeyTemplate, tenantId);
            var tenant = await distributedCacheWrapper.StringGetAsync(tenantCacheKey);
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
            foreach (Segment segment in data.Segments)
            {
                var segmentCacheKey = string.Format(segmentCacheKeyTemplate, segment.SegmentId);
                var tenantCacheKey = string.Format(tenantCacheKeyTemplate, segment.TenantId);
                var tenant_To_SegmentCacheKey = string.Format(tenant_To_segmentSetCacheKeyTemplate, segment.TenantId);
                var segment_To_segmentUpdateCacheKey = string.Format(segment_To_segmentUpdateSetCacheKeyTemplate, segment.SegmentId);
                var segment_To_inspectionCacheKey = string.Format(segment_To_inspectionSetCacheKeyTemplate, segment.SegmentId);

                sb.AppendFormat("segment: {0}\r\n", segmentCacheKey);
                sb.AppendFormat("tenant: {0}\r\n", tenantCacheKey);
                sb.AppendFormat("tenant-segment: {0}\r\n", tenant_To_SegmentCacheKey);
                sb.AppendFormat("segment-update: {0}\r\n", segment_To_segmentUpdateCacheKey);

                await distributedCacheWrapper.StringSetAsync(tenantCacheKey, segment.TenantId.ToString());
                await distributedCacheWrapper.StringSetAsync(segmentCacheKey, JsonConvert.SerializeObject(segment));
                await distributedCacheWrapper.SetAdd(tenant_To_SegmentCacheKey, segment.SegmentId.ToString());

                foreach (SegmentUpdate update in data.Updates.Where(u => u.SegmentIds.Contains(segment.SegmentId)))
                {
                    var segmentUpdateCacheKey = string.Format(segmentUpdateCacheKeyTemplate, segment.SegmentId, update.SegmentUpdateId);
                    var segmentUpdate_To_segmentCacheKey = string.Format(segmentUpdate_To_segmentSetCacheKeyTemplate, update.SegmentUpdateId);
                    var tenant_To_segmentUpdateCacheKey = string.Format(tenant_To_segmentUpdateSetCacheKeyTemplate, segment.TenantId);
                    var segmentUpdate_To_inspectionCacheKey = string.Format(segmentUpdate_To_inspectionSetCacheKeyTemplate, update.SegmentUpdateId);

                    sb.AppendFormat("segment: {0}\r\n", segmentCacheKey);
                    sb.AppendFormat("update-segment: {0}\r\n", segmentUpdate_To_segmentCacheKey);
                    sb.AppendFormat("tenant-update: {0}\r\n", tenant_To_segmentUpdateCacheKey);

                    await distributedCacheWrapper.StringSetAsync(segmentUpdateCacheKey, JsonConvert.SerializeObject(update));
                    await distributedCacheWrapper.SetAdd(segmentUpdate_To_segmentCacheKey, update.SegmentIds.Select(sid => sid.ToString()).ToArray());
                    await distributedCacheWrapper.SetAdd(tenant_To_segmentUpdateCacheKey, segmentUpdateCacheKey);
                    await distributedCacheWrapper.SetAdd(segment_To_segmentUpdateCacheKey, segmentUpdateCacheKey);
                    await distributedCacheWrapper.SetAdd(segmentUpdate_To_inspectionCacheKey, update.InspectionId.ToString());
                }

                foreach (Inspection inspection in data.Inspections.Where(i => i.SegmentIds.Contains(segment.SegmentId)))
                {
                    var inspectionCacheKey = string.Format(inspectionCacheKeyTemplate, inspection.InspectionId);
                    var inspection_To_segmentCacheKey = string.Format(inspection_To_segmentSetCacheKeyTemplate, inspection.InspectionId);
                    var tenant_To_inspectionCacheKey = string.Format(tenant_To_inspectionSetCacheKeyTemplate, segment.TenantId);

                    sb.AppendFormat("inspection: {0}\r\n", inspectionCacheKey);
                    sb.AppendFormat("inspection-segment: {0}\r\n", inspection_To_segmentCacheKey);
                    sb.AppendFormat("tenant-inspection: {0}\r\n", tenant_To_inspectionCacheKey);

                    await distributedCacheWrapper.StringSetAsync(inspectionCacheKey, JsonConvert.SerializeObject(inspection));
                    await distributedCacheWrapper.SetAdd(inspection_To_segmentCacheKey, inspection.SegmentIds.Select(sid => sid.ToString()).ToArray());
                    await distributedCacheWrapper.SetAdd(tenant_To_inspectionCacheKey, inspection.InspectionId.ToString());
                    await distributedCacheWrapper.SetAdd(segment_To_inspectionCacheKey, inspection.InspectionId.ToString());
                }
            }
            logger.LogInformation(sb.ToString());
            logger.LogInformation("updated cache");
        }
    }
}
