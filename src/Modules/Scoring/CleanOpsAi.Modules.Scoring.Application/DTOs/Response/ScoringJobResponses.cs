namespace CleanOpsAi.Modules.Scoring.Application.DTOs.Response
{
	public class SubmitScoringJobResponse
	{
		public Guid JobId { get; set; }
		public string RequestId { get; set; } = null!;
		public string Status { get; set; } = null!;
		public DateTime QueuedAt { get; set; }
	}

	public class ScoringJobDetailResponse
	{
		public Guid JobId { get; set; }
		public string RequestId { get; set; } = null!;
		public string EnvironmentKey { get; set; } = null!;
		public string Status { get; set; } = null!;
		public int RetryCount { get; set; }
		public string? FailureReason { get; set; }
		public DateTime CreatedAt { get; set; }
		public DateTime? CompletedAt { get; set; }
		public ScoringJobResultSummary Summary { get; set; } = new();
		public List<ScoringJobResultResponse> Results { get; set; } = new();
	}

	public class ScoringJobListItemResponse
	{
		public Guid JobId { get; set; }
		public string RequestId { get; set; } = null!;
		public string EnvironmentKey { get; set; } = null!;
		public string Status { get; set; } = null!;
		public int RetryCount { get; set; }
		public string? FailureReason { get; set; }
		public DateTime CreatedAt { get; set; }
		public DateTime? CompletedAt { get; set; }
		public ScoringJobResultSummary Summary { get; set; } = new();
	}

	public class ScoringJobResultSummary
	{
		public int Processed { get; set; }
		public int Pass { get; set; }
		public int Pending { get; set; }
		public int Fail { get; set; }
		public int Unknown { get; set; }
	}

	public class ScoringJobResultResponse
	{
		public string SourceType { get; set; } = null!;
		public string Source { get; set; } = null!;
		public string Verdict { get; set; } = null!;
		public double QualityScore { get; set; }
		public string? VisualizationBlobUrl { get; set; }
		public string PayloadJson { get; set; } = "{}";
	}

	public class ScoringRetrainBatchDetailResponse
	{
		public Guid BatchId { get; set; }
		public string Status { get; set; } = null!;
		public DateTime RequestedAtUtc { get; set; }
		public DateTime SourceWindowFromUtc { get; set; }
		public int ReviewedSampleCount { get; set; }
		public int AnnotatedSampleCount { get; set; }
		public int ApprovedAnnotationCount { get; set; }
		public int CalibrationSampleCount { get; set; }
		public DateTime? CompletedAtUtc { get; set; }
		public string? FailureReason { get; set; }
		public bool Promoted { get; set; }
		public string? MetricKey { get; set; }
		public double? CandidateMetric { get; set; }
		public double? BaselineMetric { get; set; }
		public double? MinimumImprovement { get; set; }
		public string? PromotionReason { get; set; }
		public List<ScoringRetrainRunResponse> Runs { get; set; } = new();
	}

	public class ScoringRetrainBatchListItemResponse
	{
		public Guid BatchId { get; set; }
		public string Status { get; set; } = null!;
		public DateTime RequestedAtUtc { get; set; }
		public DateTime SourceWindowFromUtc { get; set; }
		public int ReviewedSampleCount { get; set; }
		public int AnnotatedSampleCount { get; set; }
		public int ApprovedAnnotationCount { get; set; }
		public int CalibrationSampleCount { get; set; }
		public DateTime? CompletedAtUtc { get; set; }
		public bool Promoted { get; set; }
		public string? FailureReason { get; set; }
		public string? PromotionReason { get; set; }
		public string? MetricKey { get; set; }
		public double? CandidateMetric { get; set; }
		public double? BaselineMetric { get; set; }
		public double? MinimumImprovement { get; set; }
		public int RunCount { get; set; }
		public DateTime? LatestRunStartedAtUtc { get; set; }
		public List<ScoringRetrainRunResponse> Runs { get; set; } = new();
	}

	public class ScoringRetrainRunResponse
	{
		public Guid RunId { get; set; }
		public string Status { get; set; } = null!;
		public string Mode { get; set; } = null!;
		public DateTime StartedAtUtc { get; set; }
		public DateTime? CompletedAtUtc { get; set; }
		public int? ExitCode { get; set; }
		public string? Message { get; set; }

		public string? Logs { get; set; }
	}
}
