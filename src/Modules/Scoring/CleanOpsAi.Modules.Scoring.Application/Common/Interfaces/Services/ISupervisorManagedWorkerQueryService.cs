namespace CleanOpsAi.Modules.Scoring.Application.Common.Interfaces.Services
{
	public interface ISupervisorManagedWorkerQueryService
	{
		Task<IReadOnlyCollection<Guid>> GetManagedWorkerUserIdsAsync(Guid supervisorUserId, CancellationToken ct = default);
	}
}
