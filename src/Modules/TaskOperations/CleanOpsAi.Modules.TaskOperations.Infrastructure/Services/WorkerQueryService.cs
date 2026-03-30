using CleanOpsAi.BuildingBlocks.Infrastructure.Events.Request;
using CleanOpsAi.Modules.TaskOperations.Application.Common.Interfaces.Services;
using MassTransit;

namespace CleanOpsAi.Modules.TaskOperations.Infrastructure.Services
{
	public class WorkerQueryService : IWorkerQueryService
	{
		private readonly IRequestClient<GetWorkersByIdsRequest> _client;
		public WorkerQueryService(IRequestClient<GetWorkersByIdsRequest> client)
		{
			_client = client;
		}

		public async Task<Dictionary<Guid, string>> GetUserNames(List<Guid> workerIds)
		{
			var response = await _client.GetResponse<GetWorkersByIdsResponse>(new
			{
				WorkerIds = workerIds
			});
			return response.Message.Workers.ToDictionary(w => w.Id, w => w.FullName);
		}
	}
}
