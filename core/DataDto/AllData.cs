using System;
using System.Collections.Generic;
using System.Text;

namespace research_redis_key_pattern.core.DataDto
{
    public class AllData
    {
        public ICollection<Segment> Segments { get; set; }
        public ICollection<SegmentUpdate> Updates { get; set; }
    }
}
