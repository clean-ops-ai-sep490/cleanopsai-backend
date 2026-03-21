using CleanOpsAi.Modules.ServicePlanning.Application.Common.Interfaces.Repositories;
using CleanOpsAi.Modules.ServicePlanning.Domain.Entities;
using CleanOpsAi.Modules.ServicePlanning.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CleanOpsAi.Modules.ServicePlanning.Infrastructure.Repositories
{
	public class SopStepRepository : BaseRepo<SopStep, Guid>, ISopStepRepository
	{
		public SopStepRepository(ServicePlanningDbContext context) : base(context)
		{	
		}

		public async Task<List<SopStep>> GetListBySopId(Guid id, CancellationToken cancellationToken = default)
		{
			return await _context.SopSteps
				.Where(s => s.SopId == id)
				.ToListAsync(cancellationToken);
		}
	}
}
