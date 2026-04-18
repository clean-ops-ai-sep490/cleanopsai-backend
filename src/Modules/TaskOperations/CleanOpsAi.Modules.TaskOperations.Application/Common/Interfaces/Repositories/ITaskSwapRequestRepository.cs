using CleanOpsAi.BuildingBlocks.Application.Pagination;
using CleanOpsAi.Modules.TaskOperations.Application.DTOs.Request;
using CleanOpsAi.Modules.TaskOperations.Domain.Entities;
using CleanOpsAi.Modules.TaskOperations.Domain.Enums;

namespace CleanOpsAi.Modules.TaskOperations.Application.Common.Interfaces.Repositories
{
	public interface ITaskSwapRequestRepository : IBaseRepo<TaskSwapRequest, Guid>
	{
		Task<TaskSwapRequest?> GetByIdWithDetailsAsync(Guid id, CancellationToken ct = default); 

		Task<bool> HasPendingSwapAsync(Guid taskAssignmentId);

		Task<PaginatedResult<TaskSwapRequest>> GetSwapRequestsPaging(Guid SupervisorId, SwapRequestStatus? status, PaginationRequest paginationRequest, CancellationToken ct = default);

		Task<PaginatedResult<TaskSwapRequest>> GetMySwapRequestsPaging(
			Guid WorkerId,
			SwapPerspective perspective,
			SwapRequestStatus? status,
			PaginationRequest paginationRequest,
			CancellationToken ct = default);
	}
}
