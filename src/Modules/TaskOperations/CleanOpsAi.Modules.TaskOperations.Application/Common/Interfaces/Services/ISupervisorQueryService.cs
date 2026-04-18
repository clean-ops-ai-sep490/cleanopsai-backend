using CleanOpsAi.BuildingBlocks.Infrastructure.Events.Request; 
namespace CleanOpsAi.Modules.TaskOperations.Application.Common.Interfaces.Services
{
    public interface ISupervisorQueryService
    {
        Task<Guid?> GetSupervisorIdAsync(Guid workAreaId, Guid workerId, CancellationToken ct = default);

        Task<string?> GetSupervisorNameAsync(Guid userId, CancellationToken ct = default);

		Task<GetSupervisorWorkAreasResponse> GetSupervisorWorkAreasAsync(
			Guid workerId,
			Guid workerIdTarget,
			Guid workAreaId,
			CancellationToken ct);
    } 
}	

