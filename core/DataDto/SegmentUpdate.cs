using System;
using System.Collections.Generic;
using System.Text;

namespace research_redis_key_pattern.core.DataDto
{
    public class SegmentUpdate
    {
        public Guid SegmentUpdateId { get; set; }
        public string SegmentUpdateName { get; set; }
        public ICollection<Guid> SegmentIds { get; set; }
    }
}
