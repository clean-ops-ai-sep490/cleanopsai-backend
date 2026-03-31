using CleanOpsAi.BuildingBlocks.Application.Interfaces.Messaging;
using MassTransit;

namespace CleanOpsAi.BuildingBlocks.Infrastructure.Messaging
{
	public class MassTransitIntegrationBus : IIntegrationBus
	{
		private readonly IScopedClientFactory _clientFactory;
		public MassTransitIntegrationBus(IScopedClientFactory clientFactory)
		{
			_clientFactory = clientFactory;
		}

		public async Task<TResponse> RequestAsync<TRequest, TResponse>(TRequest request, CancellationToken ct = default)
			where TRequest : class
			where TResponse : class
		{
			var client = _clientFactory.CreateRequestClient<TRequest>();
			var response = await client.GetResponse<TResponse>(request);
			return response.Message;
		}
	}
}
