using CleanOpsAi.Modules.ServicePlanning.Application.Common.Interfaces.Repositories;
using CleanOpsAi.Modules.ServicePlanning.Domain.Entities;
using CleanOpsAi.Modules.ServicePlanning.Infrastructure.Data;

namespace CleanOpsAi.Modules.ServicePlanning.Infrastructure.Repositories
{
	public class StepRepository : BaseRepo<Step, Guid>, IStepRepository
	{
		public StepRepository(ServicePlanningDbContext context) : base(context)
		{
		}
	}
}
