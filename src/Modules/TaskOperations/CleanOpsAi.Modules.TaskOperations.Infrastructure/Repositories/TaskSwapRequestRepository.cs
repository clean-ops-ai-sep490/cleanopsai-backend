using CleanOpsAi.BuildingBlocks.Application.Pagination;
using CleanOpsAi.BuildingBlocks.Infrastructure.Extensions;
using CleanOpsAi.Modules.TaskOperations.Application.Common.Interfaces.Repositories;
using CleanOpsAi.Modules.TaskOperations.Application.DTOs.Request;
using CleanOpsAi.Modules.TaskOperations.Domain.Entities;
using CleanOpsAi.Modules.TaskOperations.Domain.Enums;
using CleanOpsAi.Modules.TaskOperations.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CleanOpsAi.Modules.TaskOperations.Infrastructure.Repositories
{
	public class TaskSwapRequestRepository : BaseRepo<TaskSwapRequest, Guid>, ITaskSwapRequestRepository
	{
		public TaskSwapRequestRepository(TaskOperationsDbContext context) : base(context)
		{

		}

		public async Task<TaskSwapRequest?> GetByIdWithDetailsAsync(Guid id, CancellationToken ct = default)
		{
			return await _context.TaskSwapRequests
						.Include(x => x.TaskAssignment)
						.Include(x => x.TargetTaskAssignment)
						.FirstOrDefaultAsync(x => x.Id == id, ct);
		}

		public async Task<PaginatedResult<TaskSwapRequest>> GetMySwapRequestsPaging(Guid WorkerId, SwapPerspective perspective, SwapRequestStatus? status, PaginationRequest paginationRequest, CancellationToken ct = default)
		{
			var query = _context.TaskSwapRequests.AsQueryable();

			query = perspective switch
			{
				SwapPerspective.Sent => query.Where(x => x.RequesterId == WorkerId),
				SwapPerspective.Received => query.Where(x => x.TargetWorkerId == WorkerId),
				_ => query.Where(x => x.RequesterId == WorkerId
														  || x.TargetWorkerId == WorkerId)
			};

			if (status.HasValue)
				query = query.Where(x => x.Status == status.Value);

			query = query.OrderByDescending(x => x.Id);

			return await query.ToPaginatedResultAsync(paginationRequest, ct);
		}

		public async Task<PaginatedResult<TaskSwapRequest>> GetSwapRequestsPaging(Guid supervisorId, SwapRequestStatus? status, PaginationRequest paginationRequest, CancellationToken ct = default)
		{
			var query = _context.TaskSwapRequests.AsQueryable(); 

			if (status.HasValue)
			{
				query = query.Where(x => x.Status == status.Value && x.ReviewedByUserId == supervisorId).OrderByDescending(x=>x.Id);
			}else
			{
				query = query.Where(x => x.ReviewedByUserId == supervisorId).OrderByDescending(x => x.Id);
			}

			return await query.ToPaginatedResultAsync(paginationRequest, ct);

		}

		public async Task<bool> HasPendingSwapAsync(Guid taskAssignmentId)
		{
			return await _context.TaskSwapRequests
			.AnyAsync(x =>
				(x.TaskAssignmentId == taskAssignmentId ||
				 x.TargetTaskAssignmentId == taskAssignmentId) &&
				(x.Status == SwapRequestStatus.PendingTargetApproval ||
				 x.Status == SwapRequestStatus.PendingManagerApproval));
		}
	}
}
