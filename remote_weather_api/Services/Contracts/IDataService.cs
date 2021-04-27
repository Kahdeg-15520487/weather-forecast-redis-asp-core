using research_redis_key_pattern.core.DataDto;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace remote_weather_api.Services.Contracts
{
    public interface IDataService
    {
        IEnumerable<Segment> GetSegments();
        IEnumerable<SegmentUpdate> GetSegmentUpdate(Guid segmentId);
        (IEnumerable<Segment> segments, IEnumerable<SegmentUpdate> segmentUpdates) FetchData();
    }
}
