using CleanOpsAi.BuildingBlocks.Application.Pagination;
using CleanOpsAi.Modules.ServicePlanning.Domain.Entities;

namespace CleanOpsAi.Modules.ServicePlanning.Application.Common.Interfaces.Repositories
{
	public interface IStepRepository : IBaseRepo<Step, Guid>
	{
		Task<List<Step>> GetByIdsAsync(List<Guid> ids, CancellationToken cancellationToken = default);

		Task<PaginatedResult<Step>> GetsPaging(PaginationRequest request, CancellationToken ct = default);

		Task<bool> IsActionKeyExists(string actionKey, CancellationToken ct = default);
	}
}
