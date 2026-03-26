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
    public class EquipmentRequestRepository : BaseRepo<EquipmentRequest, Guid>, IEquipmentRequestRepository
    {
        public EquipmentRequestRepository(TaskOperationsDbContext context) : base(context)
        {
        }

        public async Task<EquipmentRequest?> GetByIdExistAsync(Guid id, CancellationToken ct = default)
        {
            return await _context.EquipmentRequests
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, ct);
        }

        public async Task<PaginatedResult<EquipmentRequest>> GetsPagingAsync(PaginationRequest request, CancellationToken ct = default)
        {
            return await _context.EquipmentRequests
                .Where(x => !x.IsDeleted)
                .ToPaginatedResultAsync(request, ct);
        }

        public async Task<PaginatedResult<EquipmentRequest>> GetsByWorkerIdPagingAsync(Guid workerId, PaginationRequest request, CancellationToken ct = default)
        {
            return await _context.EquipmentRequests
                .Where(x => x.WorkerId == workerId && !x.IsDeleted)
                .ToPaginatedResultAsync(request, ct);
        }

        public async Task<PaginatedResult<EquipmentRequest>> GetsByTaskAssignmentIdPagingAsync(Guid taskAssignmentId, PaginationRequest request, CancellationToken ct = default)
        {
            return await _context.EquipmentRequests
                .Where(x => x.TaskAssignmentId == taskAssignmentId && !x.IsDeleted)
                .ToPaginatedResultAsync(request, ct);
        }

        public async Task<PaginatedResult<EquipmentRequest>> GetsByStatusPagingAsync(EquipmentRequestStatus status, PaginationRequest request, CancellationToken ct = default)
        {
            return await _context.EquipmentRequests
                .Where(x => x.Status == status && !x.IsDeleted)
                .ToPaginatedResultAsync(request, ct);
        }

        public async Task<PaginatedResult<EquipmentRequest>> GetsByEquipmentIdPagingAsync(Guid equipmentId, PaginationRequest request, CancellationToken ct = default)
        {
            return await _context.EquipmentRequests
                .Where(x => x.EquipmentId == equipmentId && !x.IsDeleted)
                .ToPaginatedResultAsync(request, ct);
        }

        public async Task<bool> ExistsAsync(Guid taskAssignmentId, Guid equipmentId, CancellationToken ct = default)
        {
            return await _context.EquipmentRequests
                .AnyAsync(x =>
                    x.TaskAssignmentId == taskAssignmentId &&
                    x.EquipmentId == equipmentId &&
                    !x.IsDeleted, ct);
        }

        public async Task AddAsync(EquipmentRequest request, CancellationToken ct = default)
        {
            await _context.EquipmentRequests.AddAsync(request, ct);
            await _context.SaveChangesAsync(ct);
        }

        public async Task UpdateAsync(EquipmentRequest request, CancellationToken ct = default)
        {
            _context.EquipmentRequests.Update(request);
            await _context.SaveChangesAsync(ct);
        }

        public async Task DeleteAsync(EquipmentRequest request, CancellationToken ct = default)
        {
            request.IsDeleted = true;
            _context.EquipmentRequests.Update(request);
            await _context.SaveChangesAsync(ct);
        }
    }
}
