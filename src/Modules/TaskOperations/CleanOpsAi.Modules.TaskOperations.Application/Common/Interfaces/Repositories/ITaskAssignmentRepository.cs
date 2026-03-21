using CleanOpsAi.BuildingBlocks.Application.Pagination; 
using CleanOpsAi.Modules.TaskOperations.Domain.Entities;

namespace CleanOpsAi.Modules.TaskOperations.Application.Common.Interfaces.Repositories
{
	public interface ITaskAssignmentRepository : IBaseRepo<TaskAssignment, Guid>
	{
		Task<bool> ExistsAsync(Guid scheduleId, DateTime scheduledAt);

		Task BulkInsertAsync(IEnumerable<TaskAssignment> assignments);

		Task<TaskAssignment?> GetByIdExist(Guid id, CancellationToken ct = default);

		Task<PaginatedResult<TaskAssignment>> GetsPaging(PaginationRequest request, CancellationToken ct = default);

		Task<PaginatedResult<TaskAssignment>> GetsByAssigneeIdPaging(Guid assgineeId, PaginationRequest request, CancellationToken ct = default);
	}
}
