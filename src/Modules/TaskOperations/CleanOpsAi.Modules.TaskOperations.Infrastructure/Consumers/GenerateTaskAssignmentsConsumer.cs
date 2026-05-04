using CleanOpsAi.BuildingBlocks.Application.Interfaces.Messaging;
using CleanOpsAi.BuildingBlocks.Infrastructure.Events;
using CleanOpsAi.Modules.TaskOperations.Application.Common.Interfaces.Services; 
using MassTransit; 

namespace CleanOpsAi.Modules.TaskOperations.Infrastructure.Consumers
{
	public class GenerateTaskAssignmentsConsumer : IConsumer<GenerateTaskAssignmentsRequestedEvent>
	{
		private readonly ITaskAssignmentService _taskAssignmentService;
		private readonly IEventBus _eventBus;

		public GenerateTaskAssignmentsConsumer(
			ITaskAssignmentService taskAssignmentService,
			IEventBus eventBus)
		{
			_taskAssignmentService = taskAssignmentService;
			_eventBus = eventBus;
		}

		public async Task Consume(ConsumeContext<GenerateTaskAssignmentsRequestedEvent> context)
		{ 
			await _taskAssignmentService.GenerateAsync(context.Message);
			 
			var updateEvent = new TaskAssignmentsGeneratedEvent
			{
				Updates = context.Message.Items.Select(x => new ScheduleUpdateItem
				{
					ScheduleId = x.ScheduleId,
					GeneratedToDate = x.ToDate
				}).ToList()
			};
			 
			await _eventBus.PublishAsync(updateEvent, context.CancellationToken);
		}
	}
}