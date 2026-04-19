using CleanOpsAi.BuildingBlocks.Infrastructure.Events.Request;
using CleanOpsAi.Modules.Workforce.Application.Interfaces;
using MassTransit;

namespace CleanOpsAi.Modules.Workforce.Infrastructure.Consumers
{
	public class GetManagedWorkerUserIdsBySupervisorConsumer : IConsumer<GetManagedWorkerUserIdsBySupervisorRequest>
	{
		private readonly IWorkAreaSupervisorService _workAreaSupervisorService;

		public GetManagedWorkerUserIdsBySupervisorConsumer(IWorkAreaSupervisorService workAreaSupervisorService)
		{
			_workAreaSupervisorService = workAreaSupervisorService;
		}

		public async Task Consume(ConsumeContext<GetManagedWorkerUserIdsBySupervisorRequest> context)
		{
			var workerUserIds = await _workAreaSupervisorService.GetManagedWorkerUserIdsBySupervisorAsync(
				context.Message.SupervisorUserId,
				context.CancellationToken);

			await context.RespondAsync(new GetManagedWorkerUserIdsBySupervisorResponse
			{
				WorkerUserIds = workerUserIds
			});
		}
	}
}
