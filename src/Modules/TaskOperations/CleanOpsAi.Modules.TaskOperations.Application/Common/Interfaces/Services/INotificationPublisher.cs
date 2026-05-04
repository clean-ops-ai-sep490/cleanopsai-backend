using CleanOpsAi.BuildingBlocks.Infrastructure.Events;

namespace CleanOpsAi.Modules.TaskOperations.Application.Common.Interfaces.Services
{
	public interface INotificationPublisher
	{
		Task PublishAsync(SendNotificationEvent @event, CancellationToken ct = default);
	}
}
