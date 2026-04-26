using CleanOpsAi.Modules.Scoring.Domain.Entities;
using CleanOpsAi.Modules.Scoring.Domain.Enums;

namespace CleanOpsAi.Modules.Scoring.Application.Common.Interfaces.Repositories
{
	public interface IScoringJobRepository
	{
		Task<ScoringJob?> GetByRequestIdAsync(string requestId, CancellationToken ct = default);
		Task<ScoringJob?> GetByIdWithResultsAsync(Guid jobId, CancellationToken ct = default);
		Task<IReadOnlyCollection<ScoringJob>> GetJobsAsync(ScoringJobStatus? status, int take, CancellationToken ct = default);
		Task<IReadOnlyCollection<ScoringJobResult>> GetPendingResultsAsync(
			int take,
			IReadOnlyCollection<Guid>? submittedByUserIds = null,
			CancellationToken ct = default);
		Task<IReadOnlyCollection<ScoringAnnotationCandidate>> GetAnnotationCandidatesAsync(
			ScoringAnnotationCandidateStatus? status,
			string? environmentKey,
			Guid? assignedToUserId,
			DateTime? createdFromUtc,
			int take,
			CancellationToken ct = default);
		Task<IReadOnlyCollection<ScoringAnnotationCandidate>> GetAnnotatedCandidatesForRetrainAsync(DateTime sinceUtc, int take, CancellationToken ct = default);
		Task<IReadOnlyCollection<ScoringAnnotationCandidate>> GetApprovedAnnotationCandidatesForRetrainAsync(DateTime sinceUtc, int take, CancellationToken ct = default);
		Task<IReadOnlyCollection<ScoringJobResult>> GetReviewedResultsForRetrainAsync(DateTime sinceUtc, int take, CancellationToken ct = default);
		Task<ScoringJobResult?> GetResultByIdWithJobAsync(Guid resultId, CancellationToken ct = default);
		Task<ScoringAnnotationCandidate?> GetAnnotationCandidateByIdAsync(Guid candidateId, CancellationToken ct = default);
		Task<ScoringAnnotationCandidate?> GetAnnotationCandidateByResultIdAsync(Guid resultId, CancellationToken ct = default);
		Task<ScoringRetrainBatch?> GetRetrainBatchByIdWithRunsAsync(Guid batchId, CancellationToken ct = default);
		Task<IReadOnlyCollection<ScoringRetrainBatch>> GetRetrainBatchesAsync(ScoringRetrainBatchStatus? status, int take, CancellationToken ct = default);
		Task ReplaceResultsAsync(Guid jobId, IReadOnlyCollection<ScoringJobResult> results, CancellationToken ct = default);
		Task InsertAsync(ScoringJob job, CancellationToken ct = default);
		Task InsertAnnotationCandidateAsync(ScoringAnnotationCandidate candidate, CancellationToken ct = default);
		Task InsertAnnotationAsync(ScoringAnnotation annotation, CancellationToken ct = default);
		Task InsertRetrainBatchAsync(ScoringRetrainBatch batch, CancellationToken ct = default);
		Task InsertRetrainRunAsync(ScoringRetrainRun run, CancellationToken ct = default);
		Task<int> SaveChangesAsync(CancellationToken ct = default);
	}
}
