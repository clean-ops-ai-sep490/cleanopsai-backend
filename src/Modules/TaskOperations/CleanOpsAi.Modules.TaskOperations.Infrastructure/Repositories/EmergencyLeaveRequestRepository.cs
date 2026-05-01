using CleanOpsAi.BuildingBlocks.Application.Pagination;
using CleanOpsAi.BuildingBlocks.Infrastructure.Extensions;
using CleanOpsAi.Modules.TaskOperations.Application.Common.Interfaces.Repositories;
using CleanOpsAi.Modules.TaskOperations.Domain.Entities;
using CleanOpsAi.Modules.TaskOperations.Domain.Enums;
using CleanOpsAi.Modules.TaskOperations.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CleanOpsAi.Modules.TaskOperations.Infrastructure.Repositories
{
    public class EmergencyLeaveRequestRepository : BaseRepo<EmergencyLeaveRequest, Guid>, IEmergencyLeaveRequestRepository
    {
        public EmergencyLeaveRequestRepository(TaskOperationsDbContext context) : base(context)
        {
        }

        public async Task<EmergencyLeaveRequest?> GetByIdExistAsync(Guid id, CancellationToken ct = default)
        {
            return await _context.EmergencyLeaveRequests
                .Include(x => x.TaskAssignment)
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, ct);
        }

        public async Task<PaginatedResult<EmergencyLeaveRequest>> GetsPagingAsync(PaginationRequest request, CancellationToken ct = default)
        {
            return await _context.EmergencyLeaveRequests
                .Include(x => x.TaskAssignment)
                .Where(x => !x.IsDeleted)
                .OrderByDescending(x => x.Created)
                .ToPaginatedResultAsync(request, ct);
        }

        public async Task<PaginatedResult<EmergencyLeaveRequest>> GetsByWorkerIdPagingAsync(Guid workerId, PaginationRequest request, CancellationToken ct = default)
        {
            return await _context.EmergencyLeaveRequests
                .Include(x => x.TaskAssignment)
                .Where(x => x.WorkerId == workerId && !x.IsDeleted)
                .OrderByDescending(x => x.Created)
                .ToPaginatedResultAsync(request, ct);
        }

        public async Task<PaginatedResult<EmergencyLeaveRequest>> GetsByTaskAssignmentIdPagingAsync(Guid taskAssignmentId, PaginationRequest request, CancellationToken ct = default)
        {
            return await _context.EmergencyLeaveRequests
                .Include(x => x.TaskAssignment)
                .Where(x => x.TaskAssignmentId == taskAssignmentId && !x.IsDeleted)
                .OrderByDescending(x => x.Created)
                .ToPaginatedResultAsync(request, ct);
        }

        public async Task<PaginatedResult<EmergencyLeaveRequest>> GetsByStatusPagingAsync(RequestStatus status, PaginationRequest request, CancellationToken ct = default)
        {
            return await _context.EmergencyLeaveRequests
                .Include(x => x.TaskAssignment)
                .Where(x => x.Status == status && !x.IsDeleted)
                .OrderByDescending(x => x.Created)
                .ToPaginatedResultAsync(request, ct);
        }

        public async Task<PaginatedResult<EmergencyLeaveRequest>> GetsByDateRangePagingAsync(
            DateTime from, DateTime to, PaginationRequest request, CancellationToken ct = default)
        {
            return await _context.EmergencyLeaveRequests
                .Where(x => !x.IsDeleted
                    && x.LeaveDateFrom <= to
                    && x.LeaveDateTo >= from)
                .OrderByDescending(x => x.LeaveDateFrom)
                .ToPaginatedResultAsync(request, ct);
        }

        public async Task<bool> ExistsAsync(Guid taskAssignmentId, Guid workerId, CancellationToken ct = default)
        {
            return await _context.EmergencyLeaveRequests
                .AnyAsync(x =>
                    x.TaskAssignmentId == taskAssignmentId &&
                    x.WorkerId == workerId &&
                    !x.IsDeleted, ct);
        }

        public async Task AddAsync(EmergencyLeaveRequest request, CancellationToken ct = default)
        {
            await _context.EmergencyLeaveRequests.AddAsync(request, ct);
            await _context.SaveChangesAsync(ct);
        }

        public async Task UpdateAsync(EmergencyLeaveRequest request, CancellationToken ct = default)
        {
            _context.EmergencyLeaveRequests.Update(request);
            await _context.SaveChangesAsync(ct);
        }

        public async Task DeleteAsync(EmergencyLeaveRequest request, CancellationToken ct = default)
        {
            request.IsDeleted = true;
            _context.EmergencyLeaveRequests.Update(request);
            await _context.SaveChangesAsync(ct);
        }

        public async Task<PaginatedResult<EmergencyLeaveRequest>> GetsByWorkerCurrentMonthPagingAsync(
            Guid workerId,
            DateTime now,
            PaginationRequest request,
            CancellationToken ct = default)
        {
            // đảm bảo now là UTC
            var nowUtc = DateTime.SpecifyKind(now, DateTimeKind.Utc);

            var monthStart = new DateTime(nowUtc.Year, nowUtc.Month, 1, 0, 0, 0, DateTimeKind.Utc);
            var monthEnd = monthStart.AddMonths(1).AddTicks(-1);

            return await _context.EmergencyLeaveRequests
                .Include(x => x.TaskAssignment)
                .Where(x =>
                    x.WorkerId == workerId &&
                    !x.IsDeleted &&
                    x.LeaveDateFrom <= monthEnd &&
                    x.LeaveDateTo >= monthStart)
                .OrderByDescending(x => x.LeaveDateFrom)
                .ToPaginatedResultAsync(request, ct);
        }

    }
}
