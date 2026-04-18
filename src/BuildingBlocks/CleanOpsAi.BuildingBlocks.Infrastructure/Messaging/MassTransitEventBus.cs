using CleanOpsAi.BuildingBlocks.Application.Interfaces.Messaging;
using MassTransit;

namespace CleanOpsAi.BuildingBlocks.Infrastructure.Messaging
{
	public class MassTransitEventBus : IEventBus
	{
		private readonly IPublishEndpoint _publishEndpoint;

		public MassTransitEventBus(IPublishEndpoint publishEndpoint)
		{
			_publishEndpoint = publishEndpoint;
		}

		public Task PublishAsync<TEvent>(TEvent @event, CancellationToken ct = default)
			where TEvent : class
		{
			return _publishEndpoint.Publish(@event, ct);
		}
	}
}
