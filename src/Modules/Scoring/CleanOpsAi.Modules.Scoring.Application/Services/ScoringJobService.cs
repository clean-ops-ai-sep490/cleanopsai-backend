using CleanOpsAi.BuildingBlocks.Application;
using CleanOpsAi.BuildingBlocks.Application.Interfaces.Messaging;
using CleanOpsAi.Modules.Scoring.Application.Common.Interfaces.Repositories;
using CleanOpsAi.Modules.Scoring.Application.Common.Interfaces.Services;
using CleanOpsAi.Modules.Scoring.Application.DTOs.Request;
using CleanOpsAi.Modules.Scoring.Application.DTOs.Response;
using CleanOpsAi.Modules.Scoring.Domain.Entities;
using CleanOpsAi.Modules.Scoring.Domain.Enums;
using CleanOpsAi.Modules.Scoring.IntegrationEvents;
using System.Text.Json;

namespace CleanOpsAi.Modules.Scoring.Application.Services
{
	public class ScoringJobService : IScoringJobService
	{
		private const int MaxBatchImages = 5;
		private static readonly JsonSerializerOptions PayloadSerializerOptions = new()
		{
			DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.Never
		};
		private readonly IScoringJobRepository _repository;
		private readonly IScoringJobCache _cache;
		private readonly IScoringInferenceClient _inferenceClient;
		private readonly IEventBus _eventBus;
		private readonly IUserContext _userContext;

		public ScoringJobService(
			IScoringJobRepository repository,
			IScoringJobCache cache,
			IScoringInferenceClient inferenceClient,
			IEventBus eventBus,
			IUserContext userContext)
		{
			_repository = repository;
			_cache = cache;
			_inferenceClient = inferenceClient;
			_eventBus = eventBus;
			_userContext = userContext;
		}

		public async Task<SubmitScoringJobResponse> SubmitAsync(CreateScoringJobRequest request, CancellationToken ct = default)
		{
			request ??= new CreateScoringJobRequest();

			var imageUrls = request.ImageUrls
				.Where(x => !string.IsNullOrWhiteSpace(x))
				.Select(x => x.Trim())
				.Distinct(StringComparer.OrdinalIgnoreCase)
				.ToList();

			if (imageUrls.Count == 0)
			{
				throw new ArgumentException("At least one image URL is required.");
			}

			if (imageUrls.Count > MaxBatchImages)
			{
				throw new ArgumentException($"Maximum {MaxBatchImages} images per request.");
			}

			var requestId = string.IsNullOrWhiteSpace(request.RequestId)
				? Guid.NewGuid().ToString("N")
				: request.RequestId.Trim();

			var existingJob = await _repository.GetByRequestIdAsync(requestId, ct);
			if (existingJob is not null)
			{
				return new SubmitScoringJobResponse
				{
					JobId = existingJob.Id,
					RequestId = existingJob.RequestId,
					Status = existingJob.Status.ToString().ToUpperInvariant(),
					QueuedAt = existingJob.Created,
				};
			}

			var now = DateTime.UtcNow;
			var job = new ScoringJob
			{
				Id = Guid.NewGuid(),
				RequestId = requestId,
				EnvironmentKey = string.IsNullOrWhiteSpace(request.EnvironmentKey)
					? "LOBBY_CORRIDOR"
					: request.EnvironmentKey.Trim().ToUpperInvariant(),
				Status = ScoringJobStatus.Queued,
				SubmittedByUserId = _userContext.IsAuthenticated ? _userContext.UserId : null,
				Created = now,
				LastModified = now,
				CreatedBy = string.IsNullOrWhiteSpace(_userContext.Email) ? null : _userContext.Email,
				LastModifiedBy = string.IsNullOrWhiteSpace(_userContext.Email) ? null : _userContext.Email,
			};

			await _repository.InsertAsync(job, ct);
			await _repository.SaveChangesAsync(ct);

			await _eventBus.PublishAsync(new ScoringJobRequestedEvent
			{
				JobId = job.Id,
				RequestId = job.RequestId,
				EnvironmentKey = job.EnvironmentKey,
				ImageUrls = imageUrls,
				SubmittedByUserId = job.SubmittedByUserId,
			}, ct);

			await _cache.SetAsync(MapToDetail(job), ct);

			return new SubmitScoringJobResponse
			{
				JobId = job.Id,
				RequestId = job.RequestId,
				Status = job.Status.ToString().ToUpperInvariant(),
				QueuedAt = job.Created,
			};
		}

		public async Task<ScoringJobDetailResponse?> GetByIdAsync(Guid jobId, CancellationToken ct = default)
		{
			var cached = await _cache.GetAsync(jobId, ct);
			if (cached is not null)
			{
				return cached;
			}

			var job = await _repository.GetByIdWithResultsAsync(jobId, ct);
			if (job is null)
			{
				return null;
			}

			var response = MapToDetail(job);
			await _cache.SetAsync(response, ct);
			return response;
		}

		public async Task ProcessQueuedJobAsync(Guid jobId, string environmentKey, IReadOnlyCollection<string> imageUrls, CancellationToken ct = default)
		{
			var job = await _repository.GetByIdWithResultsAsync(jobId, ct);
			if (job is null)
			{
				return;
			}

			if (job.Status == ScoringJobStatus.Succeeded)
			{
				return;
			}

			job.Status = ScoringJobStatus.Processing;
			job.RetryCount += 1;
			job.LastModified = DateTime.UtcNow;
			await _repository.SaveChangesAsync(ct);
			await _cache.SetAsync(MapToDetail(job), ct);

			var inference = await _inferenceClient.EvaluateBatchAsync(environmentKey, imageUrls, ct);

			var mappedResults = new List<ScoringJobResult>(inference.Results.Count);
			foreach (var result in inference.Results)
			{
				var sourceType = string.IsNullOrWhiteSpace(result.SourceType) ? "unknown" : result.SourceType;
				var source = string.IsNullOrWhiteSpace(result.Source) ? $"result-{result.Id ?? 0}" : result.Source;
				var verdict = string.IsNullOrWhiteSpace(result.Scoring?.Verdict) ? "UNKNOWN" : result.Scoring!.Verdict!;

				mappedResults.Add(new ScoringJobResult
				{
					Id = Guid.NewGuid(),
					ScoringJobId = job.Id,
					SourceType = sourceType,
					Source = source,
					Verdict = verdict,
					QualityScore = result.Scoring?.QualityScore ?? 0,
					PayloadJson = JsonSerializer.Serialize(result, PayloadSerializerOptions),
					Created = DateTime.UtcNow,
					LastModified = DateTime.UtcNow,
				});
			}

			await _repository.ReplaceResultsAsync(job.Id, mappedResults, ct);
			job.Results = mappedResults;

			job.Status = ScoringJobStatus.Succeeded;
			job.CompletedAt = DateTime.UtcNow;
			job.FailureReason = null;
			job.LastModified = DateTime.UtcNow;

			await _repository.SaveChangesAsync(ct);

			await _eventBus.PublishAsync(new ScoringJobCompletedEvent
			{
				JobId = job.Id,
				RequestId = job.RequestId,
				TotalRequested = inference.Summary.TotalRequested,
				ProcessedCount = inference.Summary.Processed,
				SkippedCount = inference.Summary.Skipped,
				PassCount = inference.Summary.Pass,
				PendingCount = inference.Summary.Pending,
				FailCount = inference.Summary.Fail,
			}, ct);

			await _cache.SetAsync(MapToDetail(job), ct);
		}

		public async Task MarkFailedAsync(Guid jobId, string reason, CancellationToken ct = default)
		{
			var job = await _repository.GetByIdWithResultsAsync(jobId, ct);
			if (job is null)
			{
				return;
			}

			job.Status = ScoringJobStatus.Failed;
			job.CompletedAt = DateTime.UtcNow;
			job.FailureReason = reason.Length > 2000 ? reason[..2000] : reason;
			job.LastModified = DateTime.UtcNow;
			await _repository.SaveChangesAsync(ct);

			await _eventBus.PublishAsync(new ScoringJobFailedEvent
			{
				JobId = job.Id,
				RequestId = job.RequestId,
				Reason = job.FailureReason,
			}, ct);

			await _cache.SetAsync(MapToDetail(job), ct);
		}

		private static ScoringJobDetailResponse MapToDetail(ScoringJob job)
		{
			var orderedResults = job.Results
				.OrderBy(x => x.Created)
				.ToList();

			var passCount = orderedResults.Count(x => string.Equals(x.Verdict, "PASS", StringComparison.OrdinalIgnoreCase));
			var pendingCount = orderedResults.Count(x => string.Equals(x.Verdict, "PENDING", StringComparison.OrdinalIgnoreCase));
			var failCount = orderedResults.Count(x => string.Equals(x.Verdict, "FAIL", StringComparison.OrdinalIgnoreCase));
			var unknownCount = orderedResults.Count - passCount - pendingCount - failCount;

			return new ScoringJobDetailResponse
			{
				JobId = job.Id,
				RequestId = job.RequestId,
				EnvironmentKey = job.EnvironmentKey,
				Status = job.Status.ToString().ToUpperInvariant(),
				RetryCount = job.RetryCount,
				FailureReason = job.FailureReason,
				CreatedAt = job.Created,
				CompletedAt = job.CompletedAt,
				Summary = new ScoringJobResultSummary
				{
					Processed = orderedResults.Count,
					Pass = passCount,
					Pending = pendingCount,
					Fail = failCount,
					Unknown = unknownCount,
				},
				Results = orderedResults
					.Select(x => new ScoringJobResultResponse
					{
						SourceType = x.SourceType,
						Source = x.Source,
						Verdict = x.Verdict,
						QualityScore = x.QualityScore,
						PayloadJson = x.PayloadJson,
					})
					.ToList(),
			};
		}
	}
}
