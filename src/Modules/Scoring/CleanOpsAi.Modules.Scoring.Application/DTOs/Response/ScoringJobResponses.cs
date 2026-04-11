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
		public string PayloadJson { get; set; } = "{}";
	}
}
