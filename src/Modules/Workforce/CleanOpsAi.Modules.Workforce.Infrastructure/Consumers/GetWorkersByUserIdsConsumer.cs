using CleanOpsAi.BuildingBlocks.Infrastructure.Events.Request;
using CleanOpsAi.Modules.Workforce.Application.Interfaces;
using MassTransit;

namespace CleanOpsAi.Modules.Workforce.Infrastructure.Consumers
{
	public class GetWorkersByUserIdsConsumer : IConsumer<GetWorkersByUserIdsRequest>
	{
		private readonly IWorkerService _workerService;

		public GetWorkersByUserIdsConsumer(IWorkerService workerService)
		{
			_workerService = workerService;
		}

		public async Task Consume(ConsumeContext<GetWorkersByUserIdsRequest> context)
		{
			var workers = await _workerService.GetWorkersByUserIds(context.Message.UserIds);

			await context.RespondAsync(new GetWorkersByUserIdsResponse
			{
				Workers = workers.Select(x => new WorkerSummaryByUserIdDto
				{
					UserId = x.UserId,
					WorkerId = x.WorkerId,
					FullName = x.FullName
				}).ToList()
			});
		}
	}
}
