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
    public interface IEmergencyLeaveRequestRepository : IBaseRepo<EmergencyLeaveRequest, Guid>
    {
        Task<EmergencyLeaveRequest?> GetByIdExistAsync(Guid id, CancellationToken ct = default);
        Task<PaginatedResult<EmergencyLeaveRequest>> GetsPagingAsync(PaginationRequest request, CancellationToken ct = default);
        Task<PaginatedResult<EmergencyLeaveRequest>> GetsByWorkerIdPagingAsync(Guid workerId, PaginationRequest request, CancellationToken ct = default);
        Task<PaginatedResult<EmergencyLeaveRequest>> GetsByTaskAssignmentIdPagingAsync(Guid taskAssignmentId, PaginationRequest request, CancellationToken ct = default);
        Task<PaginatedResult<EmergencyLeaveRequest>> GetsByStatusPagingAsync(RequestStatus status, PaginationRequest request, CancellationToken ct = default);
        Task<PaginatedResult<EmergencyLeaveRequest>> GetsByDateRangePagingAsync(DateTime from, DateTime to, PaginationRequest request, CancellationToken ct = default);
        Task<bool> ExistsAsync(Guid taskAssignmentId, Guid workerId, CancellationToken ct = default);
        Task AddAsync(EmergencyLeaveRequest request, CancellationToken ct = default);
        Task UpdateAsync(EmergencyLeaveRequest request, CancellationToken ct = default);
        Task DeleteAsync(EmergencyLeaveRequest request, CancellationToken ct = default);
        Task<PaginatedResult<EmergencyLeaveRequest>> GetsByWorkerCurrentMonthPagingAsync(Guid workerId, DateTime now, PaginationRequest request, CancellationToken ct = default);
    }
}
