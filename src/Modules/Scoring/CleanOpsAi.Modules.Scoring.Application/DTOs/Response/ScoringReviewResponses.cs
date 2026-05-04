namespace CleanOpsAi.Modules.Scoring.Application.DTOs.Response
{
	public class PendingScoringReviewItemResponse
	{
		public Guid ResultId { get; set; }
		public Guid JobId { get; set; }
		public string RequestId { get; set; } = null!;
		public Guid? SubmittedByUserId { get; set; }
		public Guid? WorkerId { get; set; }
		public string? WorkerName { get; set; }
		public string EnvironmentKey { get; set; } = null!;
		public string SourceType { get; set; } = null!;
		public string Source { get; set; } = null!;
		public string Verdict { get; set; } = null!;
		public double QualityScore { get; set; }
		public DateTime CreatedAt { get; set; }
	}

	public class ScoringResultReviewResponse
	{
		public Guid ResultId { get; set; }
		public Guid JobId { get; set; }
		public string OriginalVerdict { get; set; } = null!;
		public string ReviewedVerdict { get; set; } = null!;
		public string? ReviewReason { get; set; }
		public DateTime ReviewedAtUtc { get; set; }
		public Guid? ReviewedByUserId { get; set; }
		public string? ReviewedByEmail { get; set; }
	}
}