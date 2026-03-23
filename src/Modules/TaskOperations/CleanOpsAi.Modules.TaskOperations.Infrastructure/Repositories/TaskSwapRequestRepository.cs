using CleanOpsAi.BuildingBlocks.Application.Pagination;
using CleanOpsAi.BuildingBlocks.Infrastructure.Extensions;
using CleanOpsAi.Modules.TaskOperations.Application.Common.Interfaces.Repositories;
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

		public Task<PaginatedResult<TaskSwapRequest>> GetSwapCandidatesAsync(Guid taskAssignmentId, DateOnly? date, PaginationRequest paginationRequest, CancellationToken ct = default)
		{
			throw new NotImplementedException();
		}

		public async Task<PaginatedResult<TaskSwapRequest>> GetSwapRequestsPaging(SwapRequestStatus? status, PaginationRequest paginationRequest, CancellationToken ct = default)
		{
			return await _context.TaskSwapRequests.Where(x=>x.Status == status).ToPaginatedResultAsync(paginationRequest, ct);
			 
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
