using CleanOpsAi.Modules.ServicePlanning.Application.Common.Interfaces.Repositories;
using CleanOpsAi.Modules.ServicePlanning.Domain.Entities;
using CleanOpsAi.Modules.ServicePlanning.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CleanOpsAi.Modules.ServicePlanning.Infrastructure.Repositories
{
	public class StepRepository : BaseRepo<Step, Guid>, IStepRepository
	{
		public StepRepository(ServicePlanningDbContext context) : base(context)
		{
		}

		public async Task<List<Step>> GetByIdsAsync(List<Guid> ids, CancellationToken cancellationToken = default)
		{
			return await _context.Steps
				.Where(s => ids.Contains(s.Id) && s.IsDeleted == false)
				.ToListAsync(cancellationToken);
		}
	}
}
