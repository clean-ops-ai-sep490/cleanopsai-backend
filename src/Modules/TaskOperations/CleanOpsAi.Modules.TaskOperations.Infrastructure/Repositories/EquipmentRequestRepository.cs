using CleanOpsAi.BuildingBlocks.Application.Pagination;
using CleanOpsAi.BuildingBlocks.Infrastructure.Extensions;
using CleanOpsAi.Modules.TaskOperations.Application.Common.Interfaces.Repositories;
using CleanOpsAi.Modules.TaskOperations.Domain.Entities;
using CleanOpsAi.Modules.TaskOperations.Domain.Enums;
using CleanOpsAi.Modules.TaskOperations.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CleanOpsAi.Modules.TaskOperations.Infrastructure.Repositories
{
    public class EquipmentRequestRepository : BaseRepo<EquipmentRequest, Guid>, IEquipmentRequestRepository
    {
        public EquipmentRequestRepository(TaskOperationsDbContext context) : base(context)
        {
        }

        public async Task<EquipmentRequest?> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            return await _context.EquipmentRequests
                .Include(x => x.Items)
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, ct);
        }

        public async Task<PaginatedResult<EquipmentRequest>> GetsAsync(
            PaginationRequest request,
            CancellationToken ct = default)
        {
            var query = _context.EquipmentRequests
                .Include(x => x.Items)
                .Where(x => !x.IsDeleted)
                .OrderByDescending(x => x.Created);

            return await query.ToPaginatedResultAsync(request, ct);
        }

        public async Task AddAsync(EquipmentRequest entity, CancellationToken ct = default)
        {
            await _context.EquipmentRequests.AddAsync(entity, ct);
            await _context.SaveChangesAsync(ct);
        }

        public async Task UpdateAsync(EquipmentRequest entity, CancellationToken ct = default)
        {
            _context.EquipmentRequests.Update(entity);
            await _context.SaveChangesAsync(ct);
        }

        public async Task DeleteAsync(EquipmentRequest entity, CancellationToken ct = default)
        {
            entity.IsDeleted = true;
            _context.EquipmentRequests.Update(entity);
            await _context.SaveChangesAsync(ct);
        }

        public async Task<PaginatedResult<EquipmentRequest>> GetByStatusAsync(
            EquipmentRequestStatus status,
            PaginationRequest request,
            CancellationToken ct = default)
        {
            var query = _context.EquipmentRequests
                .Include(x => x.Items)
                .Where(x => !x.IsDeleted && x.Status == status)
                .OrderByDescending(x => x.Created);

            return await query.ToPaginatedResultAsync(request, ct);
        }

        public async Task<PaginatedResult<EquipmentRequest>> GetByTaskAssignmentIdAsync(
            Guid taskAssignmentId,
            PaginationRequest request,
            CancellationToken ct = default)
        {
            var query = _context.EquipmentRequests
                .Include(x => x.Items)
                .Where(x => !x.IsDeleted && x.TaskAssignmentId == taskAssignmentId)
                .OrderByDescending(x => x.Created);

            return await query.ToPaginatedResultAsync(request, ct);
        }

        public async Task<PaginatedResult<EquipmentRequest>> GetByWorkerIdAsync(
            Guid workerId,
            PaginationRequest request,
            CancellationToken ct = default)
        {
            var query = _context.EquipmentRequests
                .Include(x => x.Items)
                .Where(x => !x.IsDeleted && x.WorkerId == workerId)
                .OrderByDescending(x => x.Created);

            return await query.ToPaginatedResultAsync(request, ct);
        }

    }
}
