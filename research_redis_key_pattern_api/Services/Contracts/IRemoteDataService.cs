using research_redis_key_pattern.core.DataDto;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace research_redis_key_pattern_api.Services.Contracts
{
    public interface IRemoteDataService
    {
        /// <summary>
        /// Get segments by tenant.
        /// </summary>
        /// <param name="tenantId">Tenant's Id.</param>
        /// <returns>serialized object.</returns>
        Task<IEnumerable<Segment>> GetSegmentsByTenant(Guid tenantId);

        /// <summary>
        /// Get segment.
        /// </summary>
        /// <param name="tenantId">Tenant's Id.</param>
        /// <param name="segmentId">Segment's Id.</param>
        /// <returns>serialized object.</returns>
        Task<Segment> GetSegment(Guid tenantId, Guid segmentId);

        /// <summary>
        /// Get segment updates by segment.
        /// </summary>
        /// <param name="tenantId">Tenant's Id.</param>
        /// <param name="segmentId">Segment's Id.</param>
        /// <returns>serialized object.</returns>
        Task<IEnumerable<SegmentUpdate>> GetSegmentUpdatesBySegment(Guid tenantId, Guid segmentId);

        /// <summary>
        /// Get segment updates by tenant
        /// </summary>
        /// <param name="tenantId">Tenant's Id.</param>
        /// <returns>serialized object.</returns>
        Task<IEnumerable<SegmentUpdate>> GetSegmentUpdatesByTenant(Guid tenantId);

        /// <summary>
        /// Get inspections by segment.
        /// </summary>
        /// <param name="tenantId">Tenant's Id.</param>
        /// <param name="segmentId">Segment's Id.</param>
        /// <returns>serialized object.</returns>
        Task<IEnumerable<Inspection>> GetInspectionsBySegment(Guid tenantId, Guid segmentId);

        /// <summary>
        /// Get inspections by tenant.
        /// </summary>
        /// <param name="tenantId">Tenant's Id.</param>
        /// <returns>serialized object.</returns>
        Task<IEnumerable<Inspection>> GetInspectionsByTenant(Guid tenantId);
    }
}
