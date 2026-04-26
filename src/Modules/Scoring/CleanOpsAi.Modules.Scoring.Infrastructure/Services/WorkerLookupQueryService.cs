using CleanOpsAi.BuildingBlocks.Application.Interfaces.Messaging;
using CleanOpsAi.BuildingBlocks.Infrastructure.Events.Request;
using CleanOpsAi.Modules.Scoring.Application.Common.Interfaces.Services;

namespace CleanOpsAi.Modules.Scoring.Infrastructure.Services
{
	public class WorkerLookupQueryService : IWorkerLookupQueryService
	{
		private readonly IIntegrationBus _integrationBus;

		public WorkerLookupQueryService(IIntegrationBus integrationBus)
		{
			_integrationBus = integrationBus;
		}

		public async Task<IReadOnlyCollection<WorkerLookupItem>> GetWorkersByUserIdsAsync(
			IReadOnlyCollection<Guid> userIds,
			CancellationToken ct = default)
		{
			if (userIds.Count == 0)
			{
				return Array.Empty<WorkerLookupItem>();
			}

			var distinctUserIds = userIds
				.Where(x => x != Guid.Empty)
				.Distinct()
				.ToList();

			if (distinctUserIds.Count == 0)
			{
				return Array.Empty<WorkerLookupItem>();
			}

			var response = await _integrationBus.RequestAsync<
				GetWorkersByUserIdsRequest,
				GetWorkersByUserIdsResponse>(
				new GetWorkersByUserIdsRequest
				{
					UserIds = distinctUserIds
				},
				ct);

			return response.Workers
				.Select(x => new WorkerLookupItem
				{
					UserId = x.UserId,
					WorkerId = x.WorkerId,
					FullName = x.FullName
				})
				.ToList();
		}
	}
}
