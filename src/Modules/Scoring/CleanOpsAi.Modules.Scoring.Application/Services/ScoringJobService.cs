using CleanOpsAi.BuildingBlocks.Application;
using CleanOpsAi.BuildingBlocks.Application.Interfaces.Messaging;
using CleanOpsAi.Modules.Scoring.Application.Common.Interfaces.Repositories;
using CleanOpsAi.Modules.Scoring.Application.Common.Interfaces.Services;
using CleanOpsAi.Modules.Scoring.Application.DTOs.Request;
using CleanOpsAi.Modules.Scoring.Application.DTOs.Response;
using CleanOpsAi.Modules.Scoring.Domain.Entities;
using CleanOpsAi.Modules.Scoring.Domain.Enums;
using CleanOpsAi.Modules.Scoring.IntegrationEvents;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace CleanOpsAi.Modules.Scoring.Application.Services
{
	public class ScoringJobService : IScoringJobService
	{
		private const int MaxBatchImages = 5;
		private static readonly HashSet<string> AllowedReviewedVerdicts = new(StringComparer.OrdinalIgnoreCase)
		{
			"PASS",
			"FAIL",
		};
		private static readonly JsonSerializerOptions PayloadSerializerOptions = new()
		{
			DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.Never
		};
		private readonly IScoringJobRepository _repository;
		private readonly IScoringJobCache _cache;
		private readonly IScoringInferenceClient _inferenceClient;
		private readonly IEventBus _eventBus;
		private readonly IUserContext _userContext;
		private readonly ILogger<ScoringJobService> _logger;

		public ScoringJobService(
			IScoringJobRepository repository,
			IScoringJobCache cache,
			IScoringInferenceClient inferenceClient,
			IEventBus eventBus,
			IUserContext userContext,
			ILogger<ScoringJobService> logger)
		{
			_repository = repository;
			_cache = cache;
			_inferenceClient = inferenceClient;
			_eventBus = eventBus;
			_userContext = userContext;
			_logger = logger;
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

		public async Task<IReadOnlyCollection<PendingScoringReviewItemResponse>> GetPendingResultsAsync(int take = 100, CancellationToken ct = default)
		{
			var pendingResults = await _repository.GetPendingResultsAsync(take, ct);

			return pendingResults
				.Select(x => new PendingScoringReviewItemResponse
				{
					ResultId = x.Id,
					JobId = x.ScoringJobId,
					RequestId = x.ScoringJob.RequestId,
					EnvironmentKey = x.ScoringJob.EnvironmentKey,
					SourceType = x.SourceType,
					Source = x.Source,
					Verdict = x.Verdict,
					QualityScore = x.QualityScore,
					CreatedAt = x.Created,
				})
				.ToList();
		}

		public async Task<ScoringResultReviewResponse?> ReviewPendingResultAsync(Guid resultId, ReviewScoringResultRequest request, CancellationToken ct = default)
		{
			if (resultId == Guid.Empty)
			{
				throw new ArgumentException("Result id cannot be empty.", nameof(resultId));
			}

			if (request is null)
			{
				throw new ArgumentException("Review payload cannot be null.", nameof(request));
			}

			var reviewedVerdict = string.IsNullOrWhiteSpace(request.Verdict)
				? string.Empty
				: request.Verdict.Trim().ToUpperInvariant();

			if (!AllowedReviewedVerdicts.Contains(reviewedVerdict))
			{
				throw new ArgumentException("Reviewed verdict must be PASS or FAIL.", nameof(request));
			}

			var result = await _repository.GetResultByIdWithJobAsync(resultId, ct);
			if (result is null)
			{
				return null;
			}

			var originalVerdict = string.IsNullOrWhiteSpace(result.Verdict)
				? "UNKNOWN"
				: result.Verdict.Trim().ToUpperInvariant();

			if (!string.Equals(originalVerdict, "PENDING", StringComparison.OrdinalIgnoreCase))
			{
				throw new InvalidOperationException($"Only PENDING results can be reviewed. Current verdict: {originalVerdict}.");
			}

			var now = DateTime.UtcNow;
			var reviewReason = string.IsNullOrWhiteSpace(request.Reason)
				? null
				: request.Reason.Trim();

			var payloadNode = BuildPayloadWithHumanReview(
				result.PayloadJson,
				originalVerdict,
				reviewedVerdict,
				reviewReason,
				now,
				_userContext.IsAuthenticated ? _userContext.UserId : null,
				string.IsNullOrWhiteSpace(_userContext.Email) ? null : _userContext.Email);

			result.Verdict = reviewedVerdict;
			result.PayloadJson = payloadNode.ToJsonString(PayloadSerializerOptions);
			result.LastModified = now;
			result.LastModifiedBy = string.IsNullOrWhiteSpace(_userContext.Email) ? null : _userContext.Email;

			if (result.ScoringJob is not null)
			{
				result.ScoringJob.LastModified = now;
				result.ScoringJob.LastModifiedBy = result.LastModifiedBy;
			}

			await _repository.SaveChangesAsync(ct);

			if (result.ScoringJob is not null)
			{
				var updatedJob = await _repository.GetByIdWithResultsAsync(result.ScoringJob.Id, ct);
				if (updatedJob is not null)
				{
					await _cache.SetAsync(MapToDetail(updatedJob), ct);
				}

				await _eventBus.PublishAsync(new ScoringResultReviewedEvent
				{
					JobId = result.ScoringJob.Id,
					ResultId = result.Id,
					RequestId = result.ScoringJob.RequestId,
					EnvironmentKey = result.ScoringJob.EnvironmentKey,
					SourceType = result.SourceType,
					Source = result.Source,
					OriginalVerdict = originalVerdict,
					ReviewedVerdict = reviewedVerdict,
					ReviewReason = reviewReason,
					ReviewedAtUtc = now,
					ReviewedByUserId = _userContext.IsAuthenticated ? _userContext.UserId : null,
					ReviewedByEmail = string.IsNullOrWhiteSpace(_userContext.Email) ? null : _userContext.Email,
				}, ct);
			}

			return new ScoringResultReviewResponse
			{
				ResultId = result.Id,
				JobId = result.ScoringJobId,
				OriginalVerdict = originalVerdict,
				ReviewedVerdict = reviewedVerdict,
				ReviewReason = reviewReason,
				ReviewedAtUtc = now,
				ReviewedByUserId = _userContext.IsAuthenticated ? _userContext.UserId : null,
				ReviewedByEmail = string.IsNullOrWhiteSpace(_userContext.Email) ? null : _userContext.Email,
			};
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
			var visualizationBySource = await BuildVisualizationMapAsync(environmentKey, inference.Results, ct);

			var mappedResults = new List<ScoringJobResult>(inference.Results.Count);
			foreach (var result in inference.Results)
			{
				var sourceType = string.IsNullOrWhiteSpace(result.SourceType) ? "unknown" : result.SourceType;
				var source = string.IsNullOrWhiteSpace(result.Source) ? $"result-{result.Id ?? 0}" : result.Source;
				var verdict = string.IsNullOrWhiteSpace(result.Scoring?.Verdict) ? "UNKNOWN" : result.Scoring!.Verdict!;

				visualizationBySource.TryGetValue(source, out var visualization);

				mappedResults.Add(new ScoringJobResult
				{
					Id = Guid.NewGuid(),
					ScoringJobId = job.Id,
					SourceType = sourceType,
					Source = source,
					Verdict = verdict,
					QualityScore = result.Scoring?.QualityScore ?? 0,
					PayloadJson = BuildResultPayloadJson(result, visualization),
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

		private async Task<Dictionary<string, ScoringVisualizationLinkResponse>> BuildVisualizationMapAsync(
			string environmentKey,
			IReadOnlyCollection<ScoringInferenceResult> inferenceResults,
			CancellationToken ct)
		{
			var sourceUrls = inferenceResults
				.Where(r => string.Equals(r.SourceType, "url", StringComparison.OrdinalIgnoreCase))
				.Select(r => r.Source)
				.Where(s => !string.IsNullOrWhiteSpace(s))
				.Select(s => s!)
				.Distinct(StringComparer.OrdinalIgnoreCase)
				.ToList();

			var visualizationBySource = new Dictionary<string, ScoringVisualizationLinkResponse>(StringComparer.OrdinalIgnoreCase);

			foreach (var sourceUrl in sourceUrls)
			{
				try
				{
					var visualization = await _inferenceClient.EvaluateUrlVisualizeLinkAsync(environmentKey, sourceUrl, ct);
					visualizationBySource[sourceUrl] = visualization;
				}
				catch (Exception ex)
				{
					_logger.LogWarning(
						ex,
						"Failed to generate visualization link for source {SourceUrl} in scoring job enrichment.",
						sourceUrl);
				}
			}

			return visualizationBySource;
		}

		private static string BuildResultPayloadJson(ScoringInferenceResult result, ScoringVisualizationLinkResponse? visualization)
		{
			var payloadNode = JsonSerializer.SerializeToNode(result, PayloadSerializerOptions) as JsonObject
				?? new JsonObject();

			var blobUrl = visualization?.Visualization?.Url;
			if (!string.IsNullOrWhiteSpace(blobUrl))
			{
				payloadNode["visualization_blob_url"] = blobUrl;
			}

			return payloadNode.ToJsonString(PayloadSerializerOptions);
		}

		private static string? ExtractVisualizationBlobUrl(string payloadJson)
		{
			if (string.IsNullOrWhiteSpace(payloadJson))
			{
				return null;
			}

			try
			{
				var node = JsonNode.Parse(payloadJson) as JsonObject;
				if (node is null)
				{
					return null;
				}

				var direct = node["visualization_blob_url"]?.GetValue<string>();
				if (!string.IsNullOrWhiteSpace(direct))
				{
					return direct;
				}

				var legacy = node["visualization"]?["url"]?.GetValue<string>();
				if (!string.IsNullOrWhiteSpace(legacy))
				{
					return legacy;
				}
			}
			catch
			{
				return null;
			}

			return null;
		}

		private static JsonObject BuildPayloadWithHumanReview(
			string payloadJson,
			string originalVerdict,
			string reviewedVerdict,
			string? reviewReason,
			DateTime reviewedAtUtc,
			Guid? reviewedByUserId,
			string? reviewedByEmail)
		{
			var payloadNode = JsonNode.Parse(string.IsNullOrWhiteSpace(payloadJson) ? "{}" : payloadJson) as JsonObject
				?? new JsonObject();

			var reviewEntry = new JsonObject
			{
				["original_verdict"] = originalVerdict,
				["reviewed_verdict"] = reviewedVerdict,
				["review_reason"] = reviewReason,
				["reviewed_at_utc"] = reviewedAtUtc,
				["reviewed_by_user_id"] = reviewedByUserId?.ToString(),
				["reviewed_by_email"] = reviewedByEmail,
			};

			var reviewHistory = payloadNode["human_review_history"] as JsonArray ?? new JsonArray();
			reviewHistory.Add(reviewEntry);
			payloadNode["human_review_history"] = reviewHistory;
			payloadNode["human_review"] = reviewEntry;

			return payloadNode;
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
						VisualizationBlobUrl = ExtractVisualizationBlobUrl(x.PayloadJson),
						PayloadJson = x.PayloadJson,
					})
					.ToList(),
			};
		}
	}
}
