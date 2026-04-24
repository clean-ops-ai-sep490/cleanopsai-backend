using CleanOpsAi.BuildingBlocks.Domain;
using System.ComponentModel.DataAnnotations.Schema;

namespace CleanOpsAi.Modules.Scoring.Domain.Entities
{
	[Table("scoring_job_results")]
	public class ScoringJobResult : BaseAuditableEntity
	{
		public Guid ScoringJobId { get; set; }

		public string SourceType { get; set; } = null!;

		public string Source { get; set; } = null!;

		public string Verdict { get; set; } = null!;

		public double QualityScore { get; set; }

		[Column(TypeName = "jsonb")]
		public string PayloadJson { get; set; } = "{}";

		public virtual ScoringJob ScoringJob { get; set; } = null!;

		public virtual ICollection<ScoringAnnotationCandidate> AnnotationCandidates { get; set; } = new List<ScoringAnnotationCandidate>();
	}
}
