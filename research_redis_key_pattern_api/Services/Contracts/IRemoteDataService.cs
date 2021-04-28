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
        Task<IEnumerable<Segment>> GetSegments(Guid tenantId);

        /// <summary>
        /// Get segment.
        /// </summary>
        /// <param name="tenantId">Tenant's Id.</param>
        /// <param name="segmentId">Segment's Id.</param>
        /// <returns>serialized object.</returns>
        Task<Segment> GetSegment(Guid tenantId, Guid segmentId);

        /// <summary>
        /// Get segment.
        /// </summary>
        /// <param name="tenantId">Tenant's Id.</param>
        /// <param name="segmentId">Segment's Id.</param>
        /// <returns>serialized object.</returns>
        Task<IEnumerable<SegmentUpdate>> GetSegmentUpdatesBySegment(Guid tenantId, Guid segmentId);

        /// <summary>
        /// Get segment update by tenant.
        /// </summary>
        /// <param name="tenantId">Tenant's Id.</param>
        /// <param name="updateId">Segment's Id.</param>
        /// <returns>serialized object.</returns>
        Task<SegmentUpdate> GetSegmentUpdateByTenant(Guid tenantId, Guid updateId);

        Task<IEnumerable<SegmentUpdate>> GetSegmentUpdatesByTenant(Guid tenantId);
    }
}
