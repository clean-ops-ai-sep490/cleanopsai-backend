namespace CleanOpsAi.BuildingBlocks.Application.Interfaces.Messaging
{
	public interface IEventBus
	{
		Task PublishAsync<TEvent>(TEvent @event, CancellationToken ct = default)
			where TEvent : class;
	}
}
