using CleanOpsAi.BuildingBlocks.Application.Interfaces;

namespace CleanOpsAi.BuildingBlocks.Infrastructure.Services
{
	public class DateTimeProvider : IDateTimeProvider
	{
		public DateTime UtcNow => DateTime.UtcNow;

		public DateTime Now => DateTime.Now;
	}
}
