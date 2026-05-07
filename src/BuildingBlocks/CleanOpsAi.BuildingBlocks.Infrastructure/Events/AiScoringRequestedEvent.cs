namespace CleanOpsAi.BuildingBlocks.Infrastructure.Events
{
	/// <summary>
	/// Published by TaskOperations when an AI compliance check needs scoring.
	/// </summary>
	public class AiScoringRequestedEvent
	{
		public Guid ComplianceCheckId { get; set; }

		public Guid TaskStepExecutionId { get; set; }

		public string EnvironmentKey { get; set; } = "LOBBY_CORRIDOR";

		public List<string> ImageUrls { get; set; } = new();

		public string? SubmittedByUserId { get; set; }
	}
}
