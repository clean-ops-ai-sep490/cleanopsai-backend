using CleanOpsAi.Modules.ServicePlanning.Domain.Entities;

namespace CleanOpsAi.Modules.ServicePlanning.Application.Common.Interfaces.Repositories
{
	public interface ISopStepRepository : IBaseRepo<SopStep, Guid>
	{
		Task<List<SopStep>> GetListBySopId(Guid id, CancellationToken cancellationToken = default);
	}
}
