using CleanOpsAi.BuildingBlocks.Domain;
using CleanOpsAi.Modules.Scoring.Domain.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace CleanOpsAi.Modules.Scoring.Domain.Entities
{
	[Table("scoring_jobs")]
	public class ScoringJob : BaseAuditableEntity
	{
		public string RequestId { get; set; } = null!;

		public string EnvironmentKey { get; set; } = null!;

		public ScoringJobStatus Status { get; set; } = ScoringJobStatus.Queued;

		public int RetryCount { get; set; }

		public Guid? SubmittedByUserId { get; set; }

		public DateTime? CompletedAt { get; set; }

		public string? FailureReason { get; set; }

		public virtual ICollection<ScoringJobResult> Results { get; set; } = new List<ScoringJobResult>();
	}
}
