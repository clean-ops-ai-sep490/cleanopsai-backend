using CleanOpsAi.Modules.Scoring.Application.DTOs.Response;

namespace CleanOpsAi.Modules.Scoring.Application.Common.Interfaces.Services
{
	public interface IScoringJobCache
	{
		Task<ScoringJobDetailResponse?> GetAsync(Guid jobId, CancellationToken ct = default);
		Task SetAsync(ScoringJobDetailResponse response, CancellationToken ct = default);
	}
}
