using CleanOpsAi.BuildingBlocks.Infrastructure.Events.Request; 

namespace CleanOpsAi.Modules.TaskOperations.Application.Common.Interfaces.Services
{
	public interface ISupervisorQueryService
	{
		Task<GetSupervisorWorkAreasResponse> GetSupervisorWorkAreasAsync(
			Guid workerId,
			Guid workerIdTarget,
			Guid workAreaId,
			CancellationToken ct);
	}
}
