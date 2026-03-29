namespace CleanOpsAi.Modules.TaskOperations.Application.Common.Interfaces.Services
{
	public interface IWorkerQueryService
	{
		Task<Dictionary<Guid, string>> GetUserNames(List<Guid> workerIds);
	}
}
