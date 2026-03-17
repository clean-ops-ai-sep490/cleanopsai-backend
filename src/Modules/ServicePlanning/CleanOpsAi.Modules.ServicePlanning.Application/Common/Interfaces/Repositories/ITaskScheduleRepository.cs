using CleanOpsAi.Modules.ServicePlanning.Domain.Entities;

namespace CleanOpsAi.Modules.ServicePlanning.Application.Common.Interfaces.Repositories
{
	public interface ITaskScheduleRepository : IBaseRepo<TaskSchedule, Guid>
	{
		Task<TaskSchedule?> GetById(Guid id, CancellationToken cancellationToken = default);

		Task<IReadOnlyList<TaskSchedule>> GetActiveSchedulesAsync();
	}
}
