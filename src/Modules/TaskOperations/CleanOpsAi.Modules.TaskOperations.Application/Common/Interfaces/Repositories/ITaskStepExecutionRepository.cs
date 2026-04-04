using CleanOpsAi.Modules.TaskOperations.Domain.Entities;

namespace CleanOpsAi.Modules.TaskOperations.Application.Common.Interfaces.Repositories
{
	public interface ITaskStepExecutionRepository : IBaseRepo<TaskStepExecution, Guid>
	{
		Task AddRangeStepExecutionsAsync(IEnumerable<TaskStepExecution> executions,	CancellationToken ct = default);

		Task<bool> ExistsByAssignmentId(Guid taskAssignmentId, CancellationToken ct = default);

		Task<TaskStepExecution?> GetByIdDetail(Guid id, CancellationToken ct = default);

		Task<TaskStepExecution?> GetNextStepAsync(Guid assignmentId, int currentStepOrder, CancellationToken ct);

		Task<bool> AnyUnfinishedStepAsync(Guid assignmentId, CancellationToken ct); 
	}
}
