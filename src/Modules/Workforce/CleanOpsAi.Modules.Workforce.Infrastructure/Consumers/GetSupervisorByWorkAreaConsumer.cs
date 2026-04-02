using CleanOpsAi.BuildingBlocks.Infrastructure.Events.Request;
using CleanOpsAi.Modules.Workforce.Application.Interfaces;
using MassTransit;

namespace CleanOpsAi.Modules.Workforce.Infrastructure.Consumers
{
	public class GetSupervisorByWorkAreaConsumer : IConsumer<GetSupervisorWorkAreasRequest>
	{
		private readonly IWorkAreaSupervisorService _workAreaSupervisorService;
		public GetSupervisorByWorkAreaConsumer(IWorkAreaSupervisorService workAreaSupervisorService)
		{
			_workAreaSupervisorService = workAreaSupervisorService;
		}
		public async Task Consume(ConsumeContext<GetSupervisorWorkAreasRequest> context)
		{
			var (found, supervisorId) = await _workAreaSupervisorService.GetCommonSupervisorAsync(
				context.Message.WorkAreaId,
				context.Message.WorkerId,
				context.Message.WorkerIdTarget,
				context.CancellationToken);

			await context.RespondAsync(new GetSupervisorWorkAreasResponse
			{
				Found = found,
				SupervisorUserId = supervisorId
			});
		}
	}
}
