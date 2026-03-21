using CleanOpsAi.BuildingBlocks.Application.Pagination;
using CleanOpsAi.Modules.ServicePlanning.Domain.Entities;

namespace CleanOpsAi.Modules.ServicePlanning.Application.Common.Interfaces.Repositories
{
	public interface IEnvironmentTypeRepository : IBaseRepo<EnvironmentType, Guid>
	{
		Task<PaginatedResult<EnvironmentType>> GetsPaging(PaginationRequest request, CancellationToken ct = default);
	}
}
