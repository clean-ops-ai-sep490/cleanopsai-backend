using CleanOpsAi.BuildingBlocks.Application.Pagination;
using CleanOpsAi.Modules.TaskOperations.Domain.Entities;
using CleanOpsAi.Modules.TaskOperations.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanOpsAi.Modules.TaskOperations.Application.Common.Interfaces.Repositories
{
    public interface IAdHocRequestRepository : IBaseRepo<AdHocRequest, Guid>
    {
        Task<AdHocRequest?> GetByIdExistAsync(Guid id, CancellationToken ct = default);

        Task<PaginatedResult<AdHocRequest>> GetsPagingAsync(PaginationRequest request, CancellationToken ct = default);

        Task<PaginatedResult<AdHocRequest>> GetsByWorkerIdPagingAsync(Guid workerId, PaginationRequest request, CancellationToken ct = default);

        Task<PaginatedResult<AdHocRequest>> GetsByTaskAssignmentIdPagingAsync(Guid taskAssignmentId, PaginationRequest request, CancellationToken ct = default);

        Task<PaginatedResult<AdHocRequest>> GetsByStatusPagingAsync(AdHocRequestStatus status, PaginationRequest request, CancellationToken ct = default);

        Task<PaginatedResult<AdHocRequest>> GetsByTypePagingAsync(AdHocRequestType type, PaginationRequest request, CancellationToken ct = default);

        Task<bool> ExistsAsync(Guid taskAssignmentId, Guid workerId, CancellationToken ct = default);

        Task AddAsync(AdHocRequest request, CancellationToken ct = default);

        Task UpdateAsync(AdHocRequest request, CancellationToken ct = default);

        Task DeleteAsync(AdHocRequest request, CancellationToken ct = default);
    }
}
