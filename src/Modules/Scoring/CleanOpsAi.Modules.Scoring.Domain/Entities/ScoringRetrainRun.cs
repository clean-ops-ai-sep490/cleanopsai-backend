using CleanOpsAi.BuildingBlocks.Domain;
using CleanOpsAi.Modules.Scoring.Domain.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace CleanOpsAi.Modules.Scoring.Domain.Entities
{
	[Table("scoring_retrain_runs")]
	public class ScoringRetrainRun : BaseAuditableEntity
	{
		public Guid ScoringRetrainBatchId { get; set; }

		public ScoringRetrainRunStatus Status { get; set; } = ScoringRetrainRunStatus.Queued;

		public string Mode { get; set; } = null!;

		public DateTime StartedAtUtc { get; set; }

		public DateTime? CompletedAtUtc { get; set; }

		public int? ExitCode { get; set; }

		public string? Message { get; set; }

		public virtual ScoringRetrainBatch Batch { get; set; } = null!;
	}
}
