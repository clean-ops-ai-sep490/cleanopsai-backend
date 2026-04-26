namespace CleanOpsAi.BuildingBlocks.Infrastructure.Events
{ 
    public class AiScoringRequestedEvent
    {
        public Guid ComplianceCheckId { get; set; }

        public Guid TaskStepExecutionId { get; set; }
         
        public string EnvironmentKey { get; set; } = "LOBBY_CORRIDOR";
         
        public List<string> ImageUrls { get; set; } = new();
    }
}
