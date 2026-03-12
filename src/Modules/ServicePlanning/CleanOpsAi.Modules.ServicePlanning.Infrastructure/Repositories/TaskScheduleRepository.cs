using CleanOpsAi.Modules.ServicePlanning.Application.Common.Interfaces.Repositories;
using CleanOpsAi.Modules.ServicePlanning.Domain.Entities;
using CleanOpsAi.Modules.ServicePlanning.Infrastructure.Data;

namespace CleanOpsAi.Modules.ServicePlanning.Infrastructure.Repositories
{
	public class TaskScheduleRepository : BaseRepo<TaskSchedule, Guid>, ITaskScheduleRepository
	{
		public TaskScheduleRepository(ServicePlanningDbContext context) : base(context)
		{
		}
	}
}
