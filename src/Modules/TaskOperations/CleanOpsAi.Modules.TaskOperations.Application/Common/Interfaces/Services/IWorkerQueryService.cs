namespace CleanOpsAi.Modules.TaskOperations.Application.Common.Interfaces.Services
{
	public interface IWorkerQueryService
	{
		Task<Dictionary<Guid, string>> GetUserNames(List<Guid> workerIds);
		Task<Guid?> GetUserIdByWorkerIdAsync(Guid workerId, CancellationToken ct = default);
        Task<Guid?> GetWorkerIdByUserIdAsync(Guid userId, CancellationToken ct = default);
    }
}
