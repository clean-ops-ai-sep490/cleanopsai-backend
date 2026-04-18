using CleanOpsAi.BuildingBlocks.Infrastructure.Events.Request;
using CleanOpsAi.Modules.Workforce.Application.Interfaces;
using MassTransit;

namespace CleanOpsAi.Modules.Workforce.Infrastructure.Consumers
{
	public class GetWorkersByIdsConsumer : IConsumer<GetWorkersByIdsRequest>
	{
		private readonly IWorkerService _workerService;

		public GetWorkersByIdsConsumer(IWorkerService workerService)
		{
			_workerService = workerService;
		}

		public async Task Consume(ConsumeContext<GetWorkersByIdsRequest> context)
		{
			var workerIds = context.Message.WorkerIds;

			var workers = await _workerService.GetWorkersByIds(workerIds);

			await context.RespondAsync(new GetWorkersByIdsResponse
			{
				Workers = workers
			});
		}
	}
}
