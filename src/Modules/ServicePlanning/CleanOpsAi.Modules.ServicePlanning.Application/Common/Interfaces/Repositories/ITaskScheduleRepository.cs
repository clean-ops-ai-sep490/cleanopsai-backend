using CleanOpsAi.BuildingBlocks.Application.Pagination;
using CleanOpsAi.Modules.ServicePlanning.Domain.Entities;

namespace CleanOpsAi.Modules.ServicePlanning.Application.Common.Interfaces.Repositories
{
	public interface ITaskScheduleRepository : IBaseRepo<TaskSchedule, Guid>
	{
		Task<TaskSchedule?> GetById(Guid id, CancellationToken cancellationToken = default);

		Task<IReadOnlyList<TaskSchedule>> GetActiveSchedulesAsync();

		Task<IReadOnlyList<TaskSchedule>> GetConflictingCandidateSchedulesAsync(
			Guid? workAreaDetailId,
			Guid slaShiftId,
			Guid? assigneeId,
			DateOnly windowStart,
			DateOnly windowEnd,
			Guid? excludeScheduleId = null,
			CancellationToken cancellationToken = default);

		Task<List<TaskSchedule>> GetByIdsAsync( List<Guid> ids, CancellationToken ct = default);

		Task<PaginatedResult<TaskSchedule>> GetsPaging(GetsTaskScheduleQuery query, PaginationRequest request, CancellationToken ct = default);

	}
}
