namespace CleanOpsAi.BuildingBlocks.Infrastructure.Events
{
    /// <summary>
    /// Published by the TaskOperations module when an AI compliance check is initiated.
    /// The Scoring module consumes this event to call IScoringJobService.SubmitAsync,
    /// keeping IScoringJobService exclusively inside the Scoring module boundary.
    /// </summary>
    public class AiScoringRequestedEvent
    {
        public Guid ComplianceCheckId { get; set; }

        public Guid TaskStepExecutionId { get; set; }
         
        public string EnvironmentKey { get; set; } = "LOBBY_CORRIDOR";
         
        public List<string> ImageUrls { get; set; } = new();
    }
}
