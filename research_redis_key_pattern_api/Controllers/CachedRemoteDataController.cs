using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

using research_redis_key_pattern_api.Services.Contracts;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace research_redis_key_pattern_api.Controllers
{
    [ApiController]
    [Route("api/data")]
    public class CachedRemoteDataController : ControllerBase
    {
        private readonly IRemoteDataService remoteDataService;
        private readonly ILogger<CachedRemoteDataController> _logger;

        public CachedRemoteDataController(IRemoteDataService remoteDataService, ILogger<CachedRemoteDataController> logger)
        {
            this.remoteDataService = remoteDataService;
            _logger = logger;
        }

        [HttpGet("{tenantId}/segments")]
        public async Task<IActionResult> Get(Guid tenantId)
        {
            return Ok(await this.remoteDataService.GetSegments(tenantId));
        }

        [HttpGet("{tenantId}/segments/{segmentId}/updates")]
        public async Task<IActionResult> Get(Guid tenantId, Guid segmentId)
        {
            return Ok(await this.remoteDataService.GetSegmentUpdatesBySegment(tenantId, segmentId));
        }
    }
}
