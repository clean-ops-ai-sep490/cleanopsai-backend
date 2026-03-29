using CleanOpsAi.Modules.TaskOperations.Application.Common.Interfaces.Repositories;
using CleanOpsAi.Modules.TaskOperations.Domain.Entities;
using CleanOpsAi.Modules.TaskOperations.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CleanOpsAi.Modules.TaskOperations.Infrastructure.Repositories
{
	public class TaskStepExecutionRepository : BaseRepo<TaskStepExecution, Guid>, ITaskStepExecutionRepository
	{
		public TaskStepExecutionRepository(TaskOperationsDbContext dbContext) : base(dbContext)
		{ 
		}

		public async Task AddRangeStepExecutionsAsync(IEnumerable<TaskStepExecution> executions, CancellationToken ct = default)
		{
			await _context.TaskStepExecutions.AddRangeAsync(executions, ct);
			await _context.SaveChangesAsync(ct);
		}

		public async Task<bool> ExistsByAssignmentId(Guid taskAssignmentId, CancellationToken ct = default)
		{
			return await _context.TaskStepExecutions.AnyAsync(x => x.TaskAssignmentId == taskAssignmentId, ct);
		}
	}
}
