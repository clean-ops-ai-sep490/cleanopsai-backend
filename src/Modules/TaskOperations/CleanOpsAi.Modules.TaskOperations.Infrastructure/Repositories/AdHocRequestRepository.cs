using CleanOpsAi.BuildingBlocks.Application.Pagination;
using CleanOpsAi.BuildingBlocks.Infrastructure.Extensions;
using CleanOpsAi.Modules.TaskOperations.Application.Common.Interfaces.Repositories;
using CleanOpsAi.Modules.TaskOperations.Domain.Entities;
using CleanOpsAi.Modules.TaskOperations.Domain.Enums;
using CleanOpsAi.Modules.TaskOperations.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanOpsAi.Modules.TaskOperations.Infrastructure.Repositories
{
    public class AdHocRequestRepository : BaseRepo<AdHocRequest, Guid>, IAdHocRequestRepository
    {
        public AdHocRequestRepository(TaskOperationsDbContext context) : base(context)
        {
        }

        public async Task<AdHocRequest?> GetByIdExistAsync(Guid id, CancellationToken ct = default)
        {
            return await _context.AdHocRequests
                .Include(x => x.TaskAssignment)
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, ct);
        }

        public async Task<PaginatedResult<AdHocRequest>> GetsPagingAsync(PaginationRequest request, CancellationToken ct = default)
        {
            return await _context.AdHocRequests
                .Include(x => x.TaskAssignment)
                .Where(x => !x.IsDeleted)
                .OrderByDescending(x => x.Created)
                .ToPaginatedResultAsync(request, ct);
        }

        public async Task<PaginatedResult<AdHocRequest>> GetsByWorkerIdPagingAsync(Guid workerId, PaginationRequest request, CancellationToken ct = default)
        {
            return await _context.AdHocRequests
                .Include(x => x.TaskAssignment)
                .Where(x => x.RequestedByWorkerId == workerId && !x.IsDeleted)
                .OrderByDescending(x => x.Created)
                .ToPaginatedResultAsync(request, ct);
        }

        public async Task<PaginatedResult<AdHocRequest>> GetsByTaskAssignmentIdPagingAsync(Guid taskAssignmentId, PaginationRequest request, CancellationToken ct = default)
        {
            return await _context.AdHocRequests
                .Include(x => x.TaskAssignment)
                .Where(x => x.TaskAssignmentId == taskAssignmentId && !x.IsDeleted)
                .OrderByDescending(x => x.Created)
                .ToPaginatedResultAsync(request, ct);
        }

        public async Task<PaginatedResult<AdHocRequest>> GetsByStatusPagingAsync(AdHocRequestStatus status, PaginationRequest request, CancellationToken ct = default)
        {
            return await _context.AdHocRequests
                .Include(x => x.TaskAssignment)
                .Where(x => x.Status == status && !x.IsDeleted)
                .OrderByDescending(x => x.Created)
                .ToPaginatedResultAsync(request, ct);
        }

        public async Task<PaginatedResult<AdHocRequest>> GetsByTypePagingAsync(AdHocRequestType type, PaginationRequest request, CancellationToken ct = default)
        {
            return await _context.AdHocRequests
                .Include(x => x.TaskAssignment)
                .Where(x => x.RequestType == type && !x.IsDeleted)
                .OrderByDescending(x => x.Created)
                .ToPaginatedResultAsync(request, ct);
        }

        public async Task<bool> ExistsAsync(Guid taskAssignmentId, Guid workerId, CancellationToken ct = default)
        {
            return await _context.AdHocRequests
                .AnyAsync(x =>
                    x.TaskAssignmentId == taskAssignmentId &&
                    x.RequestedByWorkerId == workerId &&
                    !x.IsDeleted, ct);
        }

        public async Task AddAsync(AdHocRequest request, CancellationToken ct = default)
        {
            await _context.AdHocRequests.AddAsync(request, ct);
            await _context.SaveChangesAsync(ct);
        }

        public async Task UpdateAsync(AdHocRequest request, CancellationToken ct = default)
        {
            _context.AdHocRequests.Update(request);
            await _context.SaveChangesAsync(ct);
        }

        public async Task DeleteAsync(AdHocRequest request, CancellationToken ct = default)
        {
            request.IsDeleted = true;
            _context.AdHocRequests.Update(request);
            await _context.SaveChangesAsync(ct);
        }
    }
}
