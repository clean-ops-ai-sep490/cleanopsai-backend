namespace CleanOpsAi.BuildingBlocks.Infrastructure.Events.Request
{
	public class SopStepsRequested
	{
		public Guid TaskScheduleId { get; init; }
	}

	public record SopStepsIntegrated  
	{
		public bool Found { get; init; }
		public string? Metadata { get; init; }
	} 
}
