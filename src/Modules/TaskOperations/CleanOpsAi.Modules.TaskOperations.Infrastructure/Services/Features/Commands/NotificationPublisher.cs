using CleanOpsAi.BuildingBlocks.Application.Interfaces.Messaging;
using CleanOpsAi.BuildingBlocks.Infrastructure.Events;
using CleanOpsAi.Modules.TaskOperations.Application.Common.Interfaces.Services; 

namespace CleanOpsAi.Modules.TaskOperations.Infrastructure.Services.Features.Commands
{
	public class NotificationPublisher : INotificationPublisher
	{
		private readonly IEventBus _eventBus;

		public NotificationPublisher(IEventBus eventBus)
		{
			_eventBus = eventBus;
		}

		public Task PublishAsync(SendNotificationEvent @event, CancellationToken ct = default)
		{
			return _eventBus.PublishAsync(@event, ct);
		}
	}
}
