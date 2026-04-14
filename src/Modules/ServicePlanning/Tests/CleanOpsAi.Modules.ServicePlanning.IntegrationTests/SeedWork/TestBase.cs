using Microsoft.Extensions.Logging;

namespace CleanOpsAi.Modules.ServicePlanning.IntegrationTests.SeedWork
{
	public class TestBase : IAsyncLifetime
	{

		protected string ConnectionString { get; private set; }

		protected ILogger Logger { get; private set; }

		public Task InitializeAsync()
		{
			throw new NotImplementedException();
		}

		public Task DisposeAsync()
		{
			throw new NotImplementedException();
		}

		
	}
}
