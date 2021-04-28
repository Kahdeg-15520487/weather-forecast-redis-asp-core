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

        [HttpGet("{tenantId}")]
        public async Task<IActionResult> GetTenant(Guid tenantId)
        {
            var result = new
            {
                segments = await this.remoteDataService.GetSegmentsByTenant(tenantId),
                updates = await this.remoteDataService.GetSegmentUpdatesByTenant(tenantId),
                inspections = await this.remoteDataService.GetInspectionsByTenant(tenantId),
            };
            return Ok(result);
        }

        [HttpGet("{tenantId}/segments")]
        public async Task<IActionResult> GetSegmentsByTenant(Guid tenantId)
        {
            return Ok(await this.remoteDataService.GetSegmentsByTenant(tenantId));
        }

        [HttpGet("{tenantId}/segments/{segmentId}")]
        public async Task<IActionResult> GetSegment(Guid tenantId, Guid segmentId)
        {
            return Ok(await this.remoteDataService.GetSegment(tenantId, segmentId));
        }

        [HttpGet("{tenantId}/segments/{segmentId}/updates")]
        public async Task<IActionResult> GetUpdatesBySegment(Guid tenantId, Guid segmentId)
        {
            return Ok(await this.remoteDataService.GetSegmentUpdatesBySegment(tenantId, segmentId));
        }

        [HttpGet("{tenantId}/segments/{segmentId}/inspections")]
        public async Task<IActionResult> GetInspectionsBySegment(Guid tenantId, Guid segmentId)
        {
            return Ok(await this.remoteDataService.GetInspectionsBySegment(tenantId, segmentId));
        }

        [HttpGet("{tenantId}/updates")]
        public async Task<IActionResult> GetUpdatesByTenant(Guid tenantId)
        {
            return Ok(await this.remoteDataService.GetSegmentUpdatesByTenant(tenantId));
        }

        [HttpGet("{tenantId}/inspections")]
        public async Task<IActionResult> GetInspectionsByTenant(Guid tenantId)
        {
            return Ok(await this.remoteDataService.GetInspectionsByTenant(tenantId));
        }
    }
}
