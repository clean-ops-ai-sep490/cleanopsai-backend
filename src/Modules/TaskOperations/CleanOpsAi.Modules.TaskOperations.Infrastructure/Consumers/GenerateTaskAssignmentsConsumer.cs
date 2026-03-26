using CleanOpsAi.BuildingBlocks.Infrastructure.Events;
using CleanOpsAi.Modules.TaskOperations.Application.Services;
using MassTransit; 

namespace CleanOpsAi.Modules.TaskOperations.Infrastructure.Consumers
{
	public class GenerateTaskAssignmentsConsumer(
	TaskAssignmentService taskAssignmentService)
	: IConsumer<GenerateTaskAssignmentsRequestedEvent>
	{
		public async Task Consume(
			ConsumeContext<GenerateTaskAssignmentsRequestedEvent> context)
		{
			Console.WriteLine("CONSUMER START");
			var msg = context.Message;

			await taskAssignmentService.GenerateAsync(context.Message);
		}
	}
}
