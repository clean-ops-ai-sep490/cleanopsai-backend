using CleanOpsAi.Modules.ServicePlanning.Application.Common.Interfaces.Repositories;
using CleanOpsAi.Modules.ServicePlanning.Domain.Entities;
using CleanOpsAi.Modules.ServicePlanning.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CleanOpsAi.Modules.ServicePlanning.Infrastructure.Repositories
{
	public class SopRepository : BaseRepo<Sop, Guid>, ISopRepository
	{
		public SopRepository(ServicePlanningDbContext context) : base(context)
		{
		}

		public async Task<Sop?> GetByIdWithStepsAsync(Guid id, CancellationToken cancellationToken = default)
		=> await _context.Sops
			.Include(s => s.SopSteps)
			.FirstOrDefaultAsync(s => s.Id == id && s.IsDeleted == false, cancellationToken);

	}
}
