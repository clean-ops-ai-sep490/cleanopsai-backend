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

		Task<PaginatedResult<TaskAssignment>> GetSwapCandidatesAsync(
			Guid workAreaId,
			Guid excludeAssigneeId,
			DateTime scheduledStartAt,
			DateTime scheduledEndAt,
			DateTime weekStart,
			DateTime weekEnd,
			DateOnly? date,
			TimeOnly? preferredStartTime, 
			PaginationRequest paginationRequest,
			CancellationToken ct = default);

		Task<bool> HasTimeConflictAsync(
			Guid excludeTaskId,
			Guid assigneeId,
			DateTime scheduledStartAt,
			DateTime scheduledEndAt,
			CancellationToken ct = default);
	}
}
