using CleanOpsAi.Modules.TaskOperations.Application.Common.Interfaces.Repositories;
using CleanOpsAi.Modules.TaskOperations.Domain.Entities;
using CleanOpsAi.Modules.TaskOperations.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CleanOpsAi.Modules.TaskOperations.Infrastructure.Repositories
{
	public class TaskAssignmentRepository : BaseRepo<TaskAssignment, Guid>, ITaskAssignmentRepository
	{
		public TaskAssignmentRepository(TaskOperationsDbContext context) : base(context)
		{
			
		}

		public async Task<bool> ExistsAsync(Guid scheduleId, DateTime scheduledAt)
		{
			return await _context.TaskAssignments
				.AnyAsync(x =>
					x.TaskScheduleId == scheduleId &&
					x.ScheduledStartAt == scheduledAt &&
					!x.IsDeleted);
		}

		public async Task BulkInsertAsync(IEnumerable<TaskAssignment> assignments)
		{
			await _context.TaskAssignments.AddRangeAsync(assignments);
			await _context.SaveChangesAsync();
		}
	}
}
