using CleanOpsAi.BuildingBlocks.Domain;
using CleanOpsAi.Modules.Scoring.Domain.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace CleanOpsAi.Modules.Scoring.Domain.Entities
{
	[Table("scoring_retrain_batches")]
	public class ScoringRetrainBatch : BaseAuditableEntity
	{
		public DateTime RequestedAtUtc { get; set; }

		public DateTime SourceWindowFromUtc { get; set; }

		public int ReviewedSampleCount { get; set; }

		public int AnnotatedSampleCount { get; set; }

		public int ApprovedAnnotationCount { get; set; }

		public int CalibrationSampleCount { get; set; }

		public ScoringRetrainBatchStatus Status { get; set; } = ScoringRetrainBatchStatus.Queued;

		public DateTime? CompletedAtUtc { get; set; }

		public string? FailureReason { get; set; }

		public bool Promoted { get; set; }

		public string? MetricKey { get; set; }

		public double? CandidateMetric { get; set; }

		public double? BaselineMetric { get; set; }

		public double? MinimumImprovement { get; set; }

		public string? PromotionReason { get; set; }

		public virtual ICollection<ScoringRetrainRun> Runs { get; set; } = new List<ScoringRetrainRun>();
	}
}
