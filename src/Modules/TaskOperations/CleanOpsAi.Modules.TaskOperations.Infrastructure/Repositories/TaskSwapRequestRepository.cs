using CleanOpsAi.Modules.TaskOperations.Application.Common.Interfaces.Repositories;
using CleanOpsAi.Modules.TaskOperations.Domain.Entities;
using CleanOpsAi.Modules.TaskOperations.Infrastructure.Data;

namespace CleanOpsAi.Modules.TaskOperations.Infrastructure.Repositories
{
	public class TaskSwapRequestRepository : BaseRepo<TaskSwapRequest, Guid>, ITaskSwapRequestRepository
	{
		public TaskSwapRequestRepository(TaskOperationsDbContext context) : base(context)
		{

		}
	}
}
