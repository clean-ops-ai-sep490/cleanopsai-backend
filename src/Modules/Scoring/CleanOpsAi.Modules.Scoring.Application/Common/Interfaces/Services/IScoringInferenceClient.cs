using CleanOpsAi.Modules.Scoring.Application.DTOs.Response;

namespace CleanOpsAi.Modules.Scoring.Application.Common.Interfaces.Services
{
	public interface IScoringInferenceClient
	{
		Task<ScoringInferenceBatchResponse> EvaluateBatchAsync(string environmentKey, IReadOnlyCollection<string> imageUrls, CancellationToken ct = default);
	}
}
