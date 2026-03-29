using CleanOpsAi.BuildingBlocks.Infrastructure.Events.Request;
using CleanOpsAi.Modules.ServicePlanning.Application.Common.Interfaces.Services; 
using MassTransit;

namespace CleanOpsAi.Modules.ServicePlanning.Infrastructure.Consumer
{
	public class GetSopStepsByScheduleConsumer(ITaskScheduleService taskScheduleService) : IConsumer<SopStepsRequested>
	{
		public async Task Consume(ConsumeContext<SopStepsRequested> context)
		{
			var msg = context.Message;

			var schedule  = await taskScheduleService.GetById(msg.TaskScheduleId);

			if (schedule is null)
			{
				await context.RespondAsync(new SopStepsIntegrated
				{
					Found = false
				});
				return;
			}

			await context.RespondAsync(new SopStepsIntegrated
			{
				Found = true,
				Metadata = schedule.Metadata.GetRawText()

			});
		}
	}
}
