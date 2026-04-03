namespace CleanOpsAi.BuildingBlocks.Application.Interfaces.Messaging
{
	public interface IIntegrationBus
	{
		Task<TResponse> RequestAsync<TRequest, TResponse>(TRequest request, CancellationToken ct = default)
		where TRequest : class
		where TResponse : class;
	}
}
