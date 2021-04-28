using System;
using System.Collections.Generic;
using System.Text;

namespace research_redis_key_pattern.core.DataDto
{
    public class Inspection
    {
        public Guid InspectionId { get; set; }
        public string InspctionName { get; set; }

        public Guid SegmentUpdateId { get; set; }
        public ICollection<Guid> SegmentIds { get; set; }
    }
}
