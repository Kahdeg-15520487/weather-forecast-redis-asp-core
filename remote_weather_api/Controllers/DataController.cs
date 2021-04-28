using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using remote_weather_api.Services.Contracts;
using remote_weather_api.Services.DTOs;

using research_redis_key_pattern.core.DataDto;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace remote_weather_api.Controllers
{
    [ApiController]
    [Route("api/data")]
    public class DataController : ControllerBase
    {

        private readonly IDataService dataService;
        private readonly ILogger<DataController> logger;

        public DataController(IDataService dataService, ILogger<DataController> logger)
        {
            this.dataService = dataService;
            this.logger = logger;
        }

        [HttpGet]
        public IActionResult Get()
        {
            logger.LogInformation("fetching all data");
            (IEnumerable<Segment> segments, IEnumerable<SegmentUpdate> segmentUpdates, IEnumerable<Inspection> inspections) = this.dataService.FetchData();

            return Ok(new AllData()
            {
                Segments = segments.ToList(),
                Updates = segmentUpdates.ToList(),
                Inspections = inspections.ToList(),
            });
        }

        [HttpGet("segment")]
        public IActionResult GetSevenDays()
        {
            return Ok(this.dataService.GetSegments());
        }

        [HttpGet("segment/{segmentId}/updates")]
        public IActionResult Get(Guid segmentId)
        {
            return Ok(this.dataService.GetSegmentUpdate(segmentId));
        }
    }
}
