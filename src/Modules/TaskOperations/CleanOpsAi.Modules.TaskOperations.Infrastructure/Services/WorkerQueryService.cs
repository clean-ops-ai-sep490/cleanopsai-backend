using CleanOpsAi.BuildingBlocks.Application.Interfaces.Messaging;
using CleanOpsAi.BuildingBlocks.Infrastructure.Events.Request;
using CleanOpsAi.Modules.TaskOperations.Application.Common.Interfaces.Services; 

namespace CleanOpsAi.Modules.TaskOperations.Infrastructure.Services
{
	public class WorkerQueryService : IWorkerQueryService
	{
		private readonly IIntegrationBus _bus;

		public WorkerQueryService(IIntegrationBus bus)
		{
			_bus = bus;
		}

		public async Task<Dictionary<Guid, string>> GetUserNames(List<Guid> workerIds)
		{
			var response = await _bus.RequestAsync<GetWorkersByIdsRequest, GetWorkersByIdsResponse>(new GetWorkersByIdsRequest
			{
				WorkerIds = workerIds
			});

			return response.Workers.ToDictionary(w => w.Id, w => w.FullName);
		}
	}
}
