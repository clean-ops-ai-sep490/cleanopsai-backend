namespace CleanOpsAi.Modules.Scoring.IntegrationEvents
{
	public record ScoringJobRequestedEvent
	{
		public Guid JobId { get; init; }
		public string RequestId { get; init; } = null!;
		public string EnvironmentKey { get; init; } = null!;
		public List<string> ImageUrls { get; init; } = new();
		public Guid? SubmittedByUserId { get; init; }
	}

	public record ScoringJobCompletedEvent
	{
		public Guid JobId { get; init; }
		public string RequestId { get; init; } = null!;
		public int TotalRequested { get; init; }
		public int ProcessedCount { get; init; }
		public int SkippedCount { get; init; }
		public int PassCount { get; init; }
		public int PendingCount { get; init; }
		public int FailCount { get; init; }
	}

	public record ScoringJobFailedEvent
	{
		public Guid JobId { get; init; }
		public string RequestId { get; init; } = null!;
		public string Reason { get; init; } = null!;
	}
}
