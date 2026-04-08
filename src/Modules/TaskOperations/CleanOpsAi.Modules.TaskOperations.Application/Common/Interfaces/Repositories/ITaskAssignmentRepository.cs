using CleanOpsAi.BuildingBlocks.Application.Pagination;
using CleanOpsAi.Modules.TaskOperations.Application.DTOs.Request;
using CleanOpsAi.Modules.TaskOperations.Domain.Entities;

namespace CleanOpsAi.Modules.TaskOperations.Application.Common.Interfaces.Repositories
{
	public interface ITaskAssignmentRepository : IBaseRepo<TaskAssignment, Guid>
	{
		Task<bool> ExistsAsync(Guid scheduleId, DateTime scheduledAt);

		Task<IEnumerable<(Guid ScheduleId, DateTime ScheduledAt)>> GetExistingKeysAsync(List<Guid> scheduleIds);

		Task BulkInsertAsync(IEnumerable<TaskAssignment> assignments);

		Task<TaskAssignment?> GetByIdExist(Guid id, CancellationToken ct = default);

		Task<PaginatedResult<TaskAssignment>> GetSwapCandidatesAsync(
			Guid workAreaId,
			Guid excludeAssigneeId,
			DateTime scheduledStartAt,
			DateTime scheduledEndAt,
			DateTime weekStart,
			DateTime weekEnd,
			DateOnly? date,
			TimeOnly? preferredStartTime,
			List<Guid>? qualifiedWorkerIds,
			PaginationRequest paginationRequest,
			CancellationToken ct = default);

		Task<bool> HasTimeConflictAsync(
			Guid excludeTaskId,
			Guid assigneeId,
			DateTime scheduledStartAt,
			DateTime scheduledEndAt,
			CancellationToken ct = default);

		Task<PaginatedResult<TaskAssignment>> Gets(TaskAssignmentFilter filter, PaginationRequest request,
			CancellationToken ct = default);

        Task<TaskAssignment?> GetOverlappingTask(
			Guid assigneeId,
			DateTime start,
			DateTime end,
			CancellationToken ct = default);

        Task<List<Guid>> GetBusyWorkerIdsAsync(
			Guid workAreaId,
			DateTime start,
			DateTime end,
			CancellationToken ct = default);

		Task<List<Guid>> GetBusyWorkerIdsWithoutAreaAsync(
			DateTime start,
			DateTime end,
			CancellationToken ct = default);

    }
}
