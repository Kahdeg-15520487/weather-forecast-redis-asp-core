using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

using Newtonsoft.Json;

using remote_weather_api.Services.Contracts;

using research_redis_key_pattern.core.DataDto;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace remote_weather_api.Services
{
    public class DataService : IDataService
    {
        static readonly string segmentCacheKeyTemplate = "segment:{0}";
        static readonly string segmentUpdateCacheKeyTemplate = "update:{0}";

        private readonly Random rnd;
        private readonly IDistributedCache distributedCache;
        private readonly ILogger<DataService> logger;

        public DataService(IDistributedCache distributedCache, ILogger<DataService> logger)
        {
            rnd = new Random(0);
            this.distributedCache = distributedCache;
            this.logger = logger;
        }

        public IEnumerable<Segment> GetSegments()
        {
            var data = FetchData();
            return data.segments;
        }

        public IEnumerable<SegmentUpdate> GetSegmentUpdate(Guid segmentId)
        {
            var data = FetchData();
            return from su in data.segmentUpdates
                   where su.SegmentIds.Contains(segmentId)
                   select su;
        }

        public (IEnumerable<Segment> segments, IEnumerable<SegmentUpdate> segmentUpdates) FetchData()
        {
            if (IsDataCached(this.distributedCache))
            {
                logger.LogInformation("fetching from cache");
                var segmentIds = Enumerable.Range(0, 10).Select(idx => GetGuid(rnd));
                var segmentUpdateIds = Enumerable.Range(0, 10).Select(idx => GetGuid(rnd));
                var segments = segmentIds.Select(sid =>
                {
                    var cacheKey = string.Format(segmentCacheKeyTemplate, sid);
                    return JsonConvert.DeserializeObject<Segment>(distributedCache.GetString(cacheKey));
                }).ToList();
                var segmentUpdates = segmentUpdateIds.Select(suid =>
                {
                    var cacheKey = string.Format(segmentUpdateCacheKeyTemplate, suid);
                    return JsonConvert.DeserializeObject<SegmentUpdate>(distributedCache.GetString(cacheKey));
                }).ToList();
                return (segments, segmentUpdates);
            }
            else
            {
                logger.LogInformation("updating cache");
                (IEnumerable<Segment> segments, IEnumerable<SegmentUpdate> segmentUpdates) data = GenData(rnd);
                foreach (var segment in data.segments)
                {
                    var cacheKey = string.Format(segmentCacheKeyTemplate, segment.SegmentId);
                    distributedCache.SetString(cacheKey, JsonConvert.SerializeObject(segment));
                }
                foreach (var segmentUpdate in data.segmentUpdates)
                {
                    var cacheKey = string.Format(segmentUpdateCacheKeyTemplate, segmentUpdate.SegmentUpdateId);
                    distributedCache.SetString(cacheKey, JsonConvert.SerializeObject(segmentUpdate));
                }
                return data;
            }
        }

        private static bool IsDataCached(IDistributedCache distributedCache)
        {
            Random r = new Random(0);
            var cacheKey = $"segment:{GetGuid(r)}";
            return distributedCache.GetString(cacheKey) != null;
        }

        private static (IEnumerable<Segment> segments, IEnumerable<SegmentUpdate> segmentUpdates) GenData(Random r)
        {
            List<Segment> segments = Enumerable.Range(0, 10).Select(idx => new Segment()
            {
                SegmentId = GetGuid(r),
                SegmentName = $"segment#{idx}",
                SegmentUpdateIds = new List<Guid>(),
            }).ToList();

            List<SegmentUpdate> segmentUpdates = Enumerable.Range(0, 10).Select(idx => new SegmentUpdate()
            {
                SegmentUpdateId = GetGuid(r),
                SegmentUpdateName = $"segmentUpdate#{idx}",
                SegmentIds = new List<Guid>(),
            }).ToList();

            for (int i = 0; i < 10; i++)
            {
                for (int j = 0; j < 10; j++)
                {
                    segments[i].SegmentUpdateIds.Add(segmentUpdates[i].SegmentUpdateId);
                    segmentUpdates[i].SegmentIds.Add(segments[i].SegmentId);
                }
                segments[i].TenantId = GetGuid(r);
                segments[i].SegmentUpdateIds = segments[i].SegmentUpdateIds.Distinct().ToList();
                segmentUpdates[i].SegmentIds = segmentUpdates[i].SegmentIds.Distinct().ToList();
            }

            return (segments, segmentUpdates);
        }

        private static Guid GetGuid(Random r)
        {
            byte[] newGuid = new byte[16];
            r.NextBytes(newGuid);
            return new Guid(newGuid);
        }
    }
}
