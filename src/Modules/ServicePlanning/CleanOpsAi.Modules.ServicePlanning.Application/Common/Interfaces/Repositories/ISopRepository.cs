using CleanOpsAi.BuildingBlocks.Application.Pagination;
using CleanOpsAi.Modules.ServicePlanning.Domain.Entities;

namespace CleanOpsAi.Modules.ServicePlanning.Application.Common.Interfaces.Repositories
{
	public interface ISopRepository : IBaseRepo<Sop, Guid>
	{
		Task<Sop?> GetByIdWithStepsAsync(
			Guid id,
			bool includeDeleted = false,
			CancellationToken cancellationToken = default);

		Task<PaginatedResult<Sop>> GetsPaging(PaginationRequest request, CancellationToken ct = default);

	}
}
