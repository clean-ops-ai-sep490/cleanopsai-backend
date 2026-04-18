using CleanOpsAi.Modules.Scoring.Application.DTOs.Request;
using CleanOpsAi.Modules.Scoring.Application.DTOs.Response;

namespace CleanOpsAi.Modules.Scoring.Application.Common.Interfaces.Services
{
	public interface IScoringJobService
	{
		Task<SubmitScoringJobResponse> SubmitAsync(CreateScoringJobRequest request, CancellationToken ct = default);
		Task<ScoringJobDetailResponse?> GetByIdAsync(Guid jobId, CancellationToken ct = default);
		Task<IReadOnlyCollection<ScoringJobListItemResponse>> GetJobsAsync(string? status = null, int take = 50, CancellationToken ct = default);
		Task<IReadOnlyCollection<PendingScoringReviewItemResponse>> GetPendingResultsAsync(int take = 100, CancellationToken ct = default);
		Task<IReadOnlyCollection<ScoringRetrainBatchListItemResponse>> GetRetrainBatchesAsync(string? status = null, int take = 50, CancellationToken ct = default);
		Task<ScoringRetrainBatchDetailResponse?> GetRetrainBatchByIdAsync(Guid batchId, CancellationToken ct = default);
		Task<ScoringRetrainBatchDetailResponse> TriggerRetrainAsync(TriggerScoringRetrainRequest request, CancellationToken ct = default);
		Task<ScoringResultReviewResponse?> ReviewPendingResultAsync(Guid resultId, ReviewScoringResultRequest request, CancellationToken ct = default);
		Task ProcessQueuedJobAsync(Guid jobId, string environmentKey, IReadOnlyCollection<string> imageUrls, CancellationToken ct = default);
		Task MarkFailedAsync(Guid jobId, string reason, CancellationToken ct = default);
	}
}
