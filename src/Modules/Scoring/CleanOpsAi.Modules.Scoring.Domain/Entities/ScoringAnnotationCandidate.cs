using CleanOpsAi.BuildingBlocks.Domain;
using CleanOpsAi.Modules.Scoring.Domain.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace CleanOpsAi.Modules.Scoring.Domain.Entities
{
	[Table("scoring_annotation_candidates")]
	public class ScoringAnnotationCandidate : BaseAuditableEntity
	{
		public Guid ResultId { get; set; }

		public Guid JobId { get; set; }

		public string RequestId { get; set; } = null!;

		public string EnvironmentKey { get; set; } = null!;

		public string ImageUrl { get; set; } = null!;

		public string? VisualizationBlobUrl { get; set; }

		public string OriginalVerdict { get; set; } = null!;

		public string ReviewedVerdict { get; set; } = null!;

		public string SourceType { get; set; } = "reviewed-fail-from-pending";

		public ScoringAnnotationCandidateStatus CandidateStatus { get; set; } = ScoringAnnotationCandidateStatus.Queued;

		public Guid? AssignedToUserId { get; set; }

		public DateTime CreatedAtUtc { get; set; }

		public DateTime? SubmittedAtUtc { get; set; }

		public DateTime? ApprovedAtUtc { get; set; }

		public string? SnapshotBlobKey { get; set; }

		public string? MetadataBlobKey { get; set; }

		public virtual ScoringJobResult Result { get; set; } = null!;

		public virtual ScoringAnnotation? Annotation { get; set; }
	}
}
