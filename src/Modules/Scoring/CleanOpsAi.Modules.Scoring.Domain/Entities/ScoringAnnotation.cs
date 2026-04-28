using CleanOpsAi.BuildingBlocks.Domain;
using CleanOpsAi.Modules.Scoring.Domain.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace CleanOpsAi.Modules.Scoring.Domain.Entities
{
	[Table("scoring_annotations")]
	public class ScoringAnnotation : BaseAuditableEntity
	{
		public Guid CandidateId { get; set; }

		public ScoringAnnotationFormat AnnotationFormat { get; set; } = ScoringAnnotationFormat.BboxRegionV1;

		[Column(TypeName = "jsonb")]
		public string LabelsJson { get; set; } = "[]";

		public string? ReviewerNote { get; set; }

		public int Version { get; set; } = 1;

		public Guid? CreatedByUserId { get; set; }

		public Guid? ApprovedByUserId { get; set; }

		public virtual ScoringAnnotationCandidate Candidate { get; set; } = null!;
	}
}
