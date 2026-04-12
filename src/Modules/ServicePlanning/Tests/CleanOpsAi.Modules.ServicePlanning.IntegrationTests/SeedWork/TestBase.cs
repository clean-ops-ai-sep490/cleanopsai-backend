using Microsoft.Extensions.Logging; 

namespace CleanOpsAi.Modules.ServicePlanning.IntegrationTests.SeedWork
{
	public class TestBase
	{
		protected string ConnectionString { get; private set; }

		protected ILogger Logger { get; private set; }
	}
}
