using CleanOpsAi.Modules.Scoring.Application.Common.Interfaces.Repositories;
using CleanOpsAi.Modules.Scoring.Domain.Entities;
using CleanOpsAi.Modules.Scoring.Domain.Enums;
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

		public async Task<IReadOnlyCollection<ScoringJob>> GetJobsAsync(ScoringJobStatus? status, int take, CancellationToken ct = default)
		{
			var safeTake = Math.Clamp(take, 1, 200);
			var query = _dbContext.ScoringJobs
				.Include(x => x.Results)
				.AsQueryable();

			if (status.HasValue)
			{
				query = query.Where(x => x.Status == status.Value);
			}

			return await query
				.OrderByDescending(x => x.Created)
				.Take(safeTake)
				.ToListAsync(ct);
		}

		public async Task<IReadOnlyCollection<ScoringJobResult>> GetPendingResultsAsync(
			int take,
			IReadOnlyCollection<Guid>? submittedByUserIds = null,
			CancellationToken ct = default)
		{
			var safeTake = Math.Clamp(take, 1, 500);

			var query = _dbContext.ScoringJobResults
				.Include(x => x.ScoringJob)
				.Where(x => x.ScoringJob.Status == ScoringJobStatus.Succeeded
					&& x.Verdict == "PENDING");

			if (submittedByUserIds is not null)
			{
				query = query.Where(x =>
					x.ScoringJob.SubmittedByUserId.HasValue &&
					submittedByUserIds.Contains(x.ScoringJob.SubmittedByUserId.Value));
			}

			return await query
				.OrderByDescending(x => x.Created)
				.Take(safeTake)
				.ToListAsync(ct);
		}

		public async Task<IReadOnlyCollection<ScoringJobResult>> GetReviewedResultsForRetrainAsync(DateTime sinceUtc, int take, CancellationToken ct = default)
		{
			var safeTake = Math.Clamp(take, 1, 5000);

			return await _dbContext.ScoringJobResults
				.Include(x => x.ScoringJob)
				.Where(x => x.ScoringJob.Status == ScoringJobStatus.Succeeded
					&& x.LastModified >= sinceUtc
					&& (x.Verdict == "PASS" || x.Verdict == "FAIL")
					&& EF.Functions.ILike(x.PayloadJson, "%\"original_verdict\":\"PENDING\"%"))
				.OrderByDescending(x => x.LastModified)
				.Take(safeTake)
				.ToListAsync(ct);
		}

		public async Task<ScoringJobResult?> GetResultByIdWithJobAsync(Guid resultId, CancellationToken ct = default)
		{
			return await _dbContext.ScoringJobResults
				.Include(x => x.ScoringJob)
				.FirstOrDefaultAsync(x => x.Id == resultId, ct);
		}

		public async Task<ScoringRetrainBatch?> GetRetrainBatchByIdWithRunsAsync(Guid batchId, CancellationToken ct = default)
		{
			return await _dbContext.ScoringRetrainBatches
				.Include(x => x.Runs.OrderByDescending(r => r.StartedAtUtc))
				.FirstOrDefaultAsync(x => x.Id == batchId, ct);
		}

		public async Task<IReadOnlyCollection<ScoringRetrainBatch>> GetRetrainBatchesAsync(ScoringRetrainBatchStatus? status, int take, CancellationToken ct = default)
		{
			var safeTake = Math.Clamp(take, 1, 200);
			var query = _dbContext.ScoringRetrainBatches
				.Include(x => x.Runs)
				.AsQueryable();

			if (status.HasValue)
			{
				query = query.Where(x => x.Status == status.Value);
			}

			return await query
				.OrderByDescending(x => x.RequestedAtUtc)
				.Take(safeTake)
				.ToListAsync(ct);
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

		public async Task InsertRetrainBatchAsync(ScoringRetrainBatch batch, CancellationToken ct = default)
		{
			await _dbContext.ScoringRetrainBatches.AddAsync(batch, ct);
		}

		public async Task InsertRetrainRunAsync(ScoringRetrainRun run, CancellationToken ct = default)
		{
			await _dbContext.ScoringRetrainRuns.AddAsync(run, ct);
		}

		public Task<int> SaveChangesAsync(CancellationToken ct = default)
		{
			return _dbContext.SaveChangesAsync(ct);
		}
	}
}
