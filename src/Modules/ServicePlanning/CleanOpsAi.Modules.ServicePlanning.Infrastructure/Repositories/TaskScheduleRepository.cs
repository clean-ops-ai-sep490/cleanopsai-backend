using CleanOpsAi.Modules.ServicePlanning.Application.Common.Interfaces.Repositories;
using CleanOpsAi.Modules.ServicePlanning.Domain.Entities;
using CleanOpsAi.Modules.ServicePlanning.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CleanOpsAi.Modules.ServicePlanning.Infrastructure.Repositories
{
	public class TaskScheduleRepository : BaseRepo<TaskSchedule, Guid>, ITaskScheduleRepository
	{
		public TaskScheduleRepository(ServicePlanningDbContext context) : base(context)
		{
		}

		public async Task<TaskSchedule?> GetById(Guid id, CancellationToken cancellationToken = default)
		{
			return await _context.TaskSchedules.FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
		}
	}
}
