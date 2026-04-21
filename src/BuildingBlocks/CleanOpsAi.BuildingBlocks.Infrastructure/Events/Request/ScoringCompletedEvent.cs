namespace CleanOpsAi.BuildingBlocks.Infrastructure.Events.Request
{
	public record ScoringCompletedEvent
	{
		public string RequestId { get; init; } = null!;

		public List<ScoringResultItem> Results { get; init; } = new();
	}

	public record ScoringResultItem
	{
		public string ImageUrl { get; init; } = null!;
		public double QualityScore { get; init; }
		public string Verdict { get; init; } = null!;
	}
}
