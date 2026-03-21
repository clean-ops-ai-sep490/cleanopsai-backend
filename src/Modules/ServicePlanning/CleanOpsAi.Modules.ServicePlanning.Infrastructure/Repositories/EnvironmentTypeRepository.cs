using CleanOpsAi.BuildingBlocks.Application.Pagination;
using CleanOpsAi.BuildingBlocks.Infrastructure.Extensions;
using CleanOpsAi.Modules.ServicePlanning.Application.Common.Interfaces.Repositories;
using CleanOpsAi.Modules.ServicePlanning.Domain.Entities;
using CleanOpsAi.Modules.ServicePlanning.Infrastructure.Data;

namespace CleanOpsAi.Modules.ServicePlanning.Infrastructure.Repositories
{
	public class EnvironmentTypeRepository : BaseRepo<EnvironmentType, Guid>, IEnvironmentTypeRepository
	{
		public EnvironmentTypeRepository(ServicePlanningDbContext context) : base(context)
		{
			
		}

		public async Task<PaginatedResult<EnvironmentType>> GetsPaging(PaginationRequest request, CancellationToken ct = default)
		{
			return await _context.EnvironmentTypes.ToPaginatedResultAsync(request, ct);
		}
	}
}
