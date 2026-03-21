using CleanOpsAi.BuildingBlocks.Application.Pagination;
using CleanOpsAi.BuildingBlocks.Infrastructure.Extensions;
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

		public async Task<Sop?> GetByIdWithStepsAsync(
			Guid id,
			bool includeDeleted = false,
			CancellationToken cancellationToken = default)
		{
			var query = _context.Sops.AsQueryable();

			if (includeDeleted)
			{ 
				query = query.IgnoreQueryFilters();
			}

			return await query
				.Where(x => x.Id == id)
				.Include(x => x.SopSteps)
				.Include(x => x.SopRequiredSkills)
				.Include(x => x.SopRequiredCertifications)
				.FirstOrDefaultAsync(cancellationToken);
		}

		public async Task<PaginatedResult<Sop>> GetsPaging(PaginationRequest request, CancellationToken ct = default)
		{
			return await _context.Sops.ToPaginatedResultAsync(request, ct);
		}
	}
}
