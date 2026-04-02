using CleanOpsAi.BuildingBlocks.Infrastructure.Events;
using CleanOpsAi.Modules.TaskOperations.Application.Common.Interfaces.Services; 
using MassTransit; 

namespace CleanOpsAi.Modules.TaskOperations.Infrastructure.Consumers
{
	public class GenerateTaskAssignmentsConsumer(
	ITaskAssignmentService taskAssignmentService)
	: IConsumer<GenerateTaskAssignmentsRequestedEvent>
	{
		public async Task Consume(
			ConsumeContext<GenerateTaskAssignmentsRequestedEvent> context)
		{
			Console.WriteLine("CONSUMER START");
			var msg = context.Message;


			Console.WriteLine($"CONSUMER START - Assignee: {msg.AssigneeName}, Location: {msg.DisplayLocation}, Id: {msg.ScheduleId}");

			await taskAssignmentService.GenerateAsync(context.Message);
		}
	}
}
