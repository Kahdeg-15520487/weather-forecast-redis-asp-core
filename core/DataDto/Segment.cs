using System;
using System.Collections.Generic;
using System.Text;

namespace research_redis_key_pattern.core.DataDto
{
    public class Segment
    {
        public Guid SegmentId { get; set; }
        public Guid TenantId { get; set; }
        public string SegmentName { get; set; }
        public ICollection<Guid> SegmentUpdateIds { get; set; }
    }
}
