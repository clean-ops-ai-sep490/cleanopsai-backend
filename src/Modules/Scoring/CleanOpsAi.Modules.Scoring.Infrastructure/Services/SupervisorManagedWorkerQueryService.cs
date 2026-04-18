using CleanOpsAi.BuildingBlocks.Application.Interfaces.Messaging;
using CleanOpsAi.BuildingBlocks.Infrastructure.Events.Request;
using CleanOpsAi.Modules.Scoring.Application.Common.Interfaces.Services;

namespace CleanOpsAi.Modules.Scoring.Infrastructure.Services
{
	public class SupervisorManagedWorkerQueryService : ISupervisorManagedWorkerQueryService
	{
		private readonly IIntegrationBus _integrationBus;

		public SupervisorManagedWorkerQueryService(IIntegrationBus integrationBus)
		{
			_integrationBus = integrationBus;
		}

		public async Task<IReadOnlyCollection<Guid>> GetManagedWorkerUserIdsAsync(Guid supervisorUserId, CancellationToken ct = default)
		{
			if (supervisorUserId == Guid.Empty)
			{
				return Array.Empty<Guid>();
			}

			var response = await _integrationBus.RequestAsync<
				GetManagedWorkerUserIdsBySupervisorRequest,
				GetManagedWorkerUserIdsBySupervisorResponse>(
				new GetManagedWorkerUserIdsBySupervisorRequest
				{
					SupervisorUserId = supervisorUserId
				},
				ct);

			return response.WorkerUserIds;
		}
	}
}
