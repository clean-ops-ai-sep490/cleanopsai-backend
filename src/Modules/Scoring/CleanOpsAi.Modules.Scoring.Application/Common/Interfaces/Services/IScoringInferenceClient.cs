using CleanOpsAi.Modules.Scoring.Application.DTOs.Response;

namespace CleanOpsAi.Modules.Scoring.Application.Common.Interfaces.Services
{
	public interface IScoringInferenceClient
	{
		Task<ScoringInferenceBatchResponse> EvaluateBatchAsync(string environmentKey, IReadOnlyCollection<string> imageUrls, CancellationToken ct = default);
		Task<ScoringVisualizationLinkResponse> EvaluateUrlVisualizeLinkAsync(string environmentKey, string imageUrl, CancellationToken ct = default);
		Task<(byte[] Content, string ContentType)> GetVisualizationImageAsync(string token, CancellationToken ct = default);
	}
}
