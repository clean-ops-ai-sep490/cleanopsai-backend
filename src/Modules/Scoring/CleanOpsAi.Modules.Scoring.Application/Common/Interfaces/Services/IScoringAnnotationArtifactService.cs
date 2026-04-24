using CleanOpsAi.Modules.Scoring.Domain.Entities;

namespace CleanOpsAi.Modules.Scoring.Application.Common.Interfaces.Services
{
	public interface IScoringAnnotationArtifactService
	{
		Task EnsureReviewedSnapshotAsync(
			ScoringAnnotationCandidate candidate,
			ScoringJobResult result,
			CancellationToken ct = default);

		Task PublishApprovedAnnotationAsync(
			ScoringAnnotationCandidate candidate,
			ScoringAnnotation annotation,
			CancellationToken ct = default);
	}
}
