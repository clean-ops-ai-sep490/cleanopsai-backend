using CleanOpsAi.BuildingBlocks.Infrastructure.Events.Request;
using CleanOpsAi.BuildingBlocks.Infrastructure.Events.Response;
using CleanOpsAi.BuildingBlocks.Application.Interfaces.Messaging;
using CleanOpsAi.Modules.TaskOperations.Application.Common.Interfaces.Services;
using MassTransit;

namespace CleanOpsAi.Modules.TaskOperations.Infrastructure.Services.Features.Queries
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

		public async Task<Guid?> GetUserIdByWorkerIdAsync(Guid workerId, CancellationToken ct = default)
		{
			var response = await _bus.RequestAsync<GetWorkersByIdsRequest, GetWorkersByIdsResponse>(new GetWorkersByIdsRequest
			{
				WorkerIds = new List<Guid> { workerId }
			}, ct);

			var worker = response.Workers.FirstOrDefault(w => w.Id == workerId);
			return worker?.UserId == Guid.Empty ? null : worker?.UserId;
		}

        public async Task<Guid?> GetWorkerIdByUserIdAsync(Guid userId, CancellationToken ct = default)
        {
			var response = await _bus.RequestAsync<GetWorkerIdByUserIdRequest, GetWorkerIdByUserIdResponse>(
				new GetWorkerIdByUserIdRequest { UserId = userId });

			return response.Found ? response.WorkerId : null;
		}

    }
}
