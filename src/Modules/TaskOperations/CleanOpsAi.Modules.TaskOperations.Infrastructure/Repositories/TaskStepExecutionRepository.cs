using CleanOpsAi.Modules.TaskOperations.Application.Common.Interfaces.Repositories;
using CleanOpsAi.Modules.TaskOperations.Domain.Entities;
using CleanOpsAi.Modules.TaskOperations.Domain.Enums;
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

		public async Task<bool> AnyUnfinishedStepAsync(Guid assignmentId, CancellationToken ct)
		{
			return await _context.TaskStepExecutions
				.AnyAsync(x => x.TaskAssignmentId == assignmentId
							&& x.Status != TaskStepExecutionStatus.Completed, ct);
		}

		public async Task<bool> ExistsByAssignmentId(Guid taskAssignmentId, CancellationToken ct = default)
		{
			return await _context.TaskStepExecutions.AnyAsync(x => x.TaskAssignmentId == taskAssignmentId, ct);
		}

		public async Task<TaskStepExecution?> GetByIdDetail(Guid id, CancellationToken ct = default)
		{
			return await _context.TaskStepExecutions
				.Include(x => x.TaskAssignment)
				.FirstOrDefaultAsync(x => x.Id == id, ct);
		}

		public async Task<TaskStepExecution?> GetNextStepAsync(Guid assignmentId, int currentStepOrder, CancellationToken ct)
		{
			return await _context.TaskStepExecutions
				.Where(x => x.TaskAssignmentId == assignmentId
						 && x.StepOrder > currentStepOrder)
				.OrderBy(x => x.StepOrder)
				.FirstOrDefaultAsync(ct);
		}

        public async Task<List<TaskStepExecution>> GetStepsWithImagesByAssignmentIdAsync(Guid taskAssignmentId, CancellationToken ct = default)
        {
            return await _context.TaskStepExecutions
                .Where(x => x.TaskAssignmentId == taskAssignmentId && !x.IsDeleted)
                .Include(x => x.TaskStepExecutionImages
                    .Where(img => !img.IsDeleted))
                .OrderBy(x => x.StepOrder)
                .AsNoTracking()
                .ToListAsync(ct);
        }

    }
}
