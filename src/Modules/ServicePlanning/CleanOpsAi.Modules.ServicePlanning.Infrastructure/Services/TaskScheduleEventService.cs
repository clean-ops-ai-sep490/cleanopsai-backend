using CleanOpsAi.BuildingBlocks.Application.Interfaces.Messaging;
using CleanOpsAi.BuildingBlocks.Infrastructure.Events;
using CleanOpsAi.Modules.ServicePlanning.Application.Common.Interfaces.Events;

namespace CleanOpsAi.Modules.ServicePlanning.Infrastructure.Services
{
	public class TaskScheduleEventService : ITaskScheduleEventService
	{
		private readonly IEventBus _eventBus;

		public TaskScheduleEventService(IEventBus eventBus)
		{
			_eventBus = eventBus;
		}

		public async Task RequestGenerateAssignments(GenerateTaskAssignmentsRequestedEvent request, CancellationToken ct)
		{
			 await _eventBus.PublishAsync(request, ct);
		}
	}
}
