using CleanOpsAi.Modules.Scoring.Application.Common.Interfaces.Repositories;
using CleanOpsAi.Modules.Scoring.Domain.Entities;
using CleanOpsAi.Modules.Scoring.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CleanOpsAi.Modules.Scoring.Infrastructure.Repositories
{
	public class ScoringJobRepository : IScoringJobRepository
	{
		private readonly ScoringDbContext _dbContext;

		public ScoringJobRepository(ScoringDbContext dbContext)
		{
			_dbContext = dbContext;
		}

		public async Task<ScoringJob?> GetByRequestIdAsync(string requestId, CancellationToken ct = default)
		{
			return await _dbContext.ScoringJobs
				.Include(x => x.Results)
				.FirstOrDefaultAsync(x => x.RequestId == requestId, ct);
		}

		public async Task<ScoringJob?> GetByIdWithResultsAsync(Guid jobId, CancellationToken ct = default)
		{
			return await _dbContext.ScoringJobs
				.Include(x => x.Results)
				.FirstOrDefaultAsync(x => x.Id == jobId, ct);
		}

		public async Task ReplaceResultsAsync(Guid jobId, IReadOnlyCollection<ScoringJobResult> results, CancellationToken ct = default)
		{
			var existing = await _dbContext.ScoringJobResults
				.Where(x => x.ScoringJobId == jobId)
				.ToListAsync(ct);

			if (existing.Count > 0)
			{
				_dbContext.ScoringJobResults.RemoveRange(existing);
			}

			if (results.Count > 0)
			{
				await _dbContext.ScoringJobResults.AddRangeAsync(results, ct);
			}
		}

		public async Task InsertAsync(ScoringJob job, CancellationToken ct = default)
		{
			await _dbContext.ScoringJobs.AddAsync(job, ct);
		}

		public Task<int> SaveChangesAsync(CancellationToken ct = default)
		{
			return _dbContext.SaveChangesAsync(ct);
		}
	}
}
