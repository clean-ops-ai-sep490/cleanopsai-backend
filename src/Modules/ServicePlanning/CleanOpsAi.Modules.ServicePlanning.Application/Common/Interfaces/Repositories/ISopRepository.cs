using CleanOpsAi.Modules.ServicePlanning.Domain.Entities;

namespace CleanOpsAi.Modules.ServicePlanning.Application.Common.Interfaces.Repositories
{
	public interface ISopRepository : IBaseRepo<Sop, Guid>
	{
		Task<Sop?> GetByIdWithStepsAsync(
			Guid id,
			bool includeDeleted = false,
			CancellationToken cancellationToken = default);


	}
}
