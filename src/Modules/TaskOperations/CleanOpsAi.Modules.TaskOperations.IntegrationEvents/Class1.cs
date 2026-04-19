namespace CleanOpsAi.Modules.TaskOperations.IntegrationEvents
{
	public record PpeCheckRequestedEvent
	{
		public Guid TaskStepExecutionId { get; init; }
		public DateTime RequestedAtUtc { get; init; }
	}
}
