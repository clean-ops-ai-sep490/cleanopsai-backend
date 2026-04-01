using CleanOpsAi.BuildingBlocks.Infrastructure.Events.Request;
using CleanOpsAi.BuildingBlocks.Infrastructure.Events.Response;
using CleanOpsAi.Modules.TaskOperations.Application.Common.Interfaces.Services;
using MassTransit;

namespace CleanOpsAi.Modules.TaskOperations.Infrastructure.Services
{
	public class WorkerQueryService : IWorkerQueryService
	{
		private readonly IRequestClient<GetWorkersByIdsRequest> _client;
        private readonly IRequestClient<GetWorkerIdByUserIdRequest> _workerIdClient;
        public WorkerQueryService(IRequestClient<GetWorkersByIdsRequest> client, IRequestClient<GetWorkerIdByUserIdRequest> workerIdClient)
		{
			_client = client;
            _workerIdClient = workerIdClient;
        }

		public async Task<Dictionary<Guid, string>> GetUserNames(List<Guid> workerIds)
		{
			var response = await _client.GetResponse<GetWorkersByIdsResponse>(new
			{
				WorkerIds = workerIds
			});
			return response.Message.Workers.ToDictionary(w => w.Id, w => w.FullName);
		}

        public async Task<Guid?> GetWorkerIdByUserIdAsync(Guid userId, CancellationToken ct = default)
        {
            var response = await _workerIdClient.GetResponse<GetWorkerIdByUserIdResponse>(
                new GetWorkerIdByUserIdRequest { UserId = userId }, ct);

            return response.Message.Found ? response.Message.WorkerId : null;
        }

    }
}
