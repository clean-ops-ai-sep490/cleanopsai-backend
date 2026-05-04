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

	public record ScoringResultReviewedEvent
	{
		public Guid JobId { get; init; }
		public Guid ResultId { get; init; }
		public string RequestId { get; init; } = null!;
		public string EnvironmentKey { get; init; } = null!;
		public string SourceType { get; init; } = null!;
		public string Source { get; init; } = null!;
		public string OriginalVerdict { get; init; } = null!;
		public string ReviewedVerdict { get; init; } = null!;
		public string? ReviewReason { get; init; }
		public DateTime ReviewedAtUtc { get; init; }
		public Guid? ReviewedByUserId { get; init; }
		public string? ReviewedByEmail { get; init; }
	}

	public record ScoringRetrainSampleItem
	{
		public Guid ResultId { get; init; }
		public Guid JobId { get; init; }
		public string RequestId { get; init; } = null!;
		public string EnvironmentKey { get; init; } = null!;
		public string SourceType { get; init; } = null!;
		public string Source { get; init; } = null!;
		public string ReviewedVerdict { get; init; } = null!;
		public DateTime ReviewedAtUtc { get; init; }
		public string? ReviewedByEmail { get; init; }
	}

	public record ScoringRetrainRequestedEvent
	{
		public Guid BatchId { get; init; }
		public DateTime RequestedAtUtc { get; init; }
		public DateTime SourceWindowFromUtc { get; init; }
		public int ReviewedSampleCount { get; init; }
		public List<ScoringRetrainSampleItem> Samples { get; init; } = new();
	}

	public record ScoringRetrainExecutionResultEvent
	{
		public Guid BatchId { get; init; }
		public DateTime CompletedAtUtc { get; init; }
		public bool Succeeded { get; init; }
		public int ExitCode { get; init; }
		public string Message { get; init; } = null!;
	}

	public record ScoringModelPromotionEvaluatedEvent
	{
		public Guid BatchId { get; init; }
		public DateTime EvaluatedAtUtc { get; init; }
		public string MetricKey { get; init; } = null!;
		public double CandidateMetric { get; init; }
		public double? BaselineMetric { get; init; }
		public double MinimumImprovement { get; init; }
		public bool Promoted { get; init; }
		public string Reason { get; init; } = null!;
	} 
}