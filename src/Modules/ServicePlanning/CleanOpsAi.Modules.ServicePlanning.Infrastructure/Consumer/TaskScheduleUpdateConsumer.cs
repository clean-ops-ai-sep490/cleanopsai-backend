using CleanOpsAi.BuildingBlocks.Infrastructure.Events;
using CleanOpsAi.Modules.ServicePlanning.Application.Common.Interfaces.Services;
using MassTransit;

namespace CleanOpsAi.Modules.ServicePlanning.Infrastructure.Consumer
{
	public class TaskScheduleUpdateConsumer : IConsumer<TaskAssignmentsGeneratedEvent>
	{
		private readonly ITaskScheduleService _taskScheduleService;

		public TaskScheduleUpdateConsumer(ITaskScheduleService taskScheduleService)
		{
			_taskScheduleService = taskScheduleService;
		}

		public async Task Consume(ConsumeContext<TaskAssignmentsGeneratedEvent> context)
		{ 
			await _taskScheduleService.UpdateCheckpointsAsync(context.Message.Updates);
		}
	}
}
