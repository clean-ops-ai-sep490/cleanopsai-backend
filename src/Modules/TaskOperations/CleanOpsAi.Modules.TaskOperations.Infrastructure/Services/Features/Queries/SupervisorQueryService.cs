using CleanOpsAi.BuildingBlocks.Application.Interfaces.Messaging;
using CleanOpsAi.BuildingBlocks.Infrastructure.Events.Request;
using CleanOpsAi.BuildingBlocks.Infrastructure.Events.Response;
using CleanOpsAi.Modules.TaskOperations.Application.Common.Interfaces.Services;

namespace CleanOpsAi.Modules.TaskOperations.Infrastructure.Services.Features.Queries
{
	public class SupervisorQueryService : ISupervisorQueryService
	{
		private readonly IIntegrationBus _bus;
		public SupervisorQueryService(IIntegrationBus integrationBus)
		{
			_bus = integrationBus;
		}

		public async Task<Guid?> GetSupervisorIdAsync(Guid workAreaId, Guid workerId, CancellationToken ct = default)
		{
			var response = await _bus.RequestAsync<GetSupervisorByWorkerAndWorkAreaRequest, GetSupervisorByWorkerAndWorkAreaResponse>(
				new GetSupervisorByWorkerAndWorkAreaRequest
				{
					WorkAreaId = workAreaId,
					WorkerId = workerId
				});

			return response.Found ? response.SupervisorId : null;
		}

		public async Task<string?> GetSupervisorNameAsync(Guid userId, CancellationToken ct = default)
		{
			var response = await _bus.RequestAsync<GetSupervisorNameByUserIdRequest, GetSupervisorNameByUserIdResponse>(
		new GetSupervisorNameByUserIdRequest { UserId = userId });

			return response.Found ? response.FullName : null;
		}

		public async Task<GetSupervisorWorkAreasResponse> GetSupervisorWorkAreasAsync(
			Guid workerId,
			Guid workerIdTarget,
			Guid workAreaId,
			CancellationToken ct)
		{ 
			var response = await _bus.RequestAsync<GetSupervisorWorkAreasRequest, GetSupervisorWorkAreasResponse>(
				new GetSupervisorWorkAreasRequest
				{
					WorkerId = workerId,
					WorkerIdTarget = workerIdTarget,
					WorkAreaId = workAreaId
				}, ct);

			return response;
		}
	}
}
