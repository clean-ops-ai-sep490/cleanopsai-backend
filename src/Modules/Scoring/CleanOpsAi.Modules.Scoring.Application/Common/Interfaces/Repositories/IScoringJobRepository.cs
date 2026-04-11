using CleanOpsAi.Modules.Scoring.Domain.Entities;

namespace CleanOpsAi.Modules.Scoring.Application.Common.Interfaces.Repositories
{
	public interface IScoringJobRepository
	{
		Task<ScoringJob?> GetByRequestIdAsync(string requestId, CancellationToken ct = default);
		Task<ScoringJob?> GetByIdWithResultsAsync(Guid jobId, CancellationToken ct = default);
		Task ReplaceResultsAsync(Guid jobId, IReadOnlyCollection<ScoringJobResult> results, CancellationToken ct = default);
		Task InsertAsync(ScoringJob job, CancellationToken ct = default);
		Task<int> SaveChangesAsync(CancellationToken ct = default);
	}
}
