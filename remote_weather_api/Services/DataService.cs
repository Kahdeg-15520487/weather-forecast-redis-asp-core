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
        static readonly string inspectionCacheKeyTemplate = "inspection:{0}";

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

        public (IEnumerable<Segment> segments, IEnumerable<SegmentUpdate> segmentUpdates, IEnumerable<Inspection> inspections) FetchData()
        {
            if (IsDataCached(this.distributedCache))
            {
                logger.LogInformation("fetching from cache");
                var segmentIds = Enumerable.Range(0, 20).Select(idx => GetGuid(rnd));
                var segmentUpdateIds = Enumerable.Range(0, 10).Select(idx => GetGuid(rnd));
                var inspectionIds = Enumerable.Range(0, 10).Select(idx => GetGuid(rnd));
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
                var inspections = inspectionIds.Select(iid =>
                {
                    var cacheKey = string.Format(inspectionCacheKeyTemplate, iid);
                    return JsonConvert.DeserializeObject<Inspection>(distributedCache.GetString(cacheKey));
                }).ToList();
                return (segments, segmentUpdates, inspections);
            }
            else
            {
                logger.LogInformation("updating cache");
                (IEnumerable<Segment> segments, IEnumerable<SegmentUpdate> segmentUpdates, IEnumerable<Inspection> inspections) data = GenData(rnd);
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
                foreach (var inspection in data.inspections)
                {
                    var cacheKey = string.Format(inspectionCacheKeyTemplate, inspection.InspectionId);
                    distributedCache.SetString(cacheKey, JsonConvert.SerializeObject(inspection));
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

        private static (IEnumerable<Segment> segments, IEnumerable<SegmentUpdate> segmentUpdates, IEnumerable<Inspection> inspections) GenData(Random r)
        {
            List<Segment> segments = Enumerable.Range(0, 20).Select(idx => new Segment()
            {
                SegmentId = GetGuid(r),
                SegmentName = $"segment#{idx}",
                SegmentUpdateIds = new List<Guid>(),
                InspectionIds = new List<Guid>(),
            }).ToList();

            List<SegmentUpdate> segmentUpdates = Enumerable.Range(0, 10).Select(idx => new SegmentUpdate()
            {
                SegmentUpdateId = GetGuid(r),
                SegmentUpdateName = $"segmentUpdate#{idx}",
                SegmentIds = new List<Guid>(),
            }).ToList();

            List<Inspection> inspections = Enumerable.Range(0, 10).Select(idx => new Inspection()
            {
                InspectionId = GetGuid(r),
                InspctionName = $"inspection#{idx}",
                SegmentIds = new List<Guid>(),
            }).ToList();

            List<Guid> tenantIds = Enumerable.Range(0, 5).Select(idx => GetGuid(r)).ToList();

            //for (int i = 0; i < 20; i++)
            //{
            //    for (int j = 0; j < 10; j++)
            //    {
            //        segments[i].SegmentUpdateIds.Add(segmentUpdates[i].SegmentUpdateId);
            //        segmentUpdates[i].SegmentIds.Add(segments[i].SegmentId);
            //    }
            //    segments[i].TenantId = GetGuid(r);
            //    segments[i].SegmentUpdateIds = segments[i].SegmentUpdateIds.Distinct().ToList();
            //    segmentUpdates[i].SegmentIds = segmentUpdates[i].SegmentIds.Distinct().ToList();
            //}

            //for (int i = 0; i < 10; i++)
            //{
            //    Guid tenantId = tenantIds[i / 2];
            //    for (int j = 0; j < 20; j++)
            //    {
            //        segments[j].TenantId = tenantId;
            //        segments[j].SegmentUpdateIds.Add(segmentUpdates[j/2].SegmentUpdateId);
            //        segments[j].SegmentUpdateIds = segments[j].SegmentUpdateIds.Distinct().ToList();

            //        segmentUpdates[i].SegmentIds.Add(segments[j/2].SegmentId);
            //        inspections[i].SegmentIds.Add(segments[j/2].SegmentId);
            //    }
            //    segmentUpdates[i].SegmentIds = segmentUpdates[i].SegmentIds.Distinct().ToList();
            //    inspections[i].SegmentIds = inspections[i].SegmentIds.Distinct().ToList();
            //}

            for (int i = 0; i < 10; i++)
            {
                Guid tenantId = tenantIds[i / 2];
                segments[i * 2].TenantId = tenantId;
                segments[i * 2 + 1].TenantId = tenantId;

                segmentUpdates[i].SegmentIds.Add(segments[i * 2].SegmentId);
                segmentUpdates[i].SegmentIds.Add(segments[i * 2 + 1].SegmentId);

                segments[i * 2].SegmentUpdateIds.Add(segmentUpdates[i].SegmentUpdateId);
                segments[i * 2 + 1].SegmentUpdateIds.Add(segmentUpdates[i].SegmentUpdateId);

                inspections[i].SegmentIds.Add(segments[i * 2].SegmentId);
                inspections[i].SegmentIds.Add(segments[i * 2 + 1].SegmentId);

                segments[i * 2].InspectionIds.Add(inspections[i].InspectionId);
                segments[i * 2 + 1].InspectionIds.Add(inspections[i].InspectionId);

                segmentUpdates[i].InspectionId = inspections[i].InspectionId;
                inspections[i].SegmentUpdateId = segmentUpdates[i].SegmentUpdateId;
            }

            return (segments, segmentUpdates, inspections);
        }

        private static Guid GetGuid(Random r)
        {
            byte[] newGuid = new byte[16];
            r.NextBytes(newGuid);
            return new Guid(newGuid);
        }
    }
}
