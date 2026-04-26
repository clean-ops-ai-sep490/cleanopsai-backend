using CleanOpsAi.Modules.Scoring.Application.Common.Interfaces.Repositories;
using CleanOpsAi.Modules.Scoring.Application.Common.Interfaces.Services;
using CleanOpsAi.Modules.Scoring.Domain.Entities;
using CleanOpsAi.Modules.Scoring.Domain.Enums;
using CleanOpsAi.Modules.Scoring.IntegrationEvents;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace CleanOpsAi.Modules.Scoring.Infrastructure.Consumers
{
	public class ScoringResultReviewedConsumer : IConsumer<ScoringResultReviewedEvent>
	{
		private readonly IScoringJobRepository _repository;
		private readonly IScoringAnnotationArtifactService _artifactService;
		private readonly ILogger<ScoringResultReviewedConsumer> _logger;

		public ScoringResultReviewedConsumer(
			IScoringJobRepository repository,
			IScoringAnnotationArtifactService artifactService,
			ILogger<ScoringResultReviewedConsumer> logger)
		{
			_repository = repository;
			_artifactService = artifactService;
			_logger = logger;
		}

		public async Task Consume(ConsumeContext<ScoringResultReviewedEvent> context)
		{
			var message = context.Message;
			var reviewedAtUtc = message.ReviewedAtUtc == default
				? DateTime.UtcNow
				: DateTime.SpecifyKind(message.ReviewedAtUtc, DateTimeKind.Utc);

			var result = await _repository.GetResultByIdWithJobAsync(message.ResultId, context.CancellationToken);
			if (result is null)
			{
				_logger.LogWarning(
					"Unable to process reviewed scoring result because result {ResultId} was not found.",
					message.ResultId);
				return;
			}

			var candidate = await EnsureAnnotationCandidateAsync(message, result, reviewedAtUtc, context.CancellationToken);
			if (candidate is null)
			{
				return;
			}

			await _artifactService.EnsureReviewedSnapshotAsync(candidate, result, context.CancellationToken);
			candidate.LastModified = DateTime.UtcNow;
			candidate.LastModifiedBy = "system";
			await _repository.SaveChangesAsync(context.CancellationToken);
		}

		private async Task<ScoringAnnotationCandidate?> EnsureAnnotationCandidateAsync(
			ScoringResultReviewedEvent message,
			ScoringJobResult result,
			DateTime reviewedAtUtc,
			CancellationToken ct)
		{
			if (!string.Equals(message.OriginalVerdict, "PENDING", StringComparison.OrdinalIgnoreCase) ||
				!string.Equals(message.ReviewedVerdict, "FAIL", StringComparison.OrdinalIgnoreCase))
			{
				return null;
			}

			var existing = await _repository.GetAnnotationCandidateByResultIdAsync(message.ResultId, ct);
			if (existing is not null)
			{
				return existing;
			}

			if (result.ScoringJob is null)
			{
				_logger.LogWarning(
					"Unable to create annotation candidate because scoring job is missing for result {ResultId}.",
					message.ResultId);
				return null;
			}

			var candidate = new ScoringAnnotationCandidate
			{
				Id = Guid.NewGuid(),
				ResultId = result.Id,
				JobId = result.ScoringJobId,
				RequestId = result.ScoringJob.RequestId,
				EnvironmentKey = result.ScoringJob.EnvironmentKey,
				ImageUrl = result.Source,
				VisualizationBlobUrl = ExtractVisualizationBlobUrl(result.PayloadJson),
				OriginalVerdict = message.OriginalVerdict,
				ReviewedVerdict = message.ReviewedVerdict,
				SourceType = "reviewed-fail-from-pending",
				CandidateStatus = ScoringAnnotationCandidateStatus.Queued,
				CreatedAtUtc = reviewedAtUtc,
				Created = reviewedAtUtc,
				LastModified = reviewedAtUtc,
				CreatedBy = "system",
				LastModifiedBy = "system",
			};

			await _repository.InsertAnnotationCandidateAsync(candidate, ct);
			await _repository.SaveChangesAsync(ct);
			return candidate;
		}

		private static string? ExtractVisualizationBlobUrl(string payloadJson)
		{
			if (string.IsNullOrWhiteSpace(payloadJson))
			{
				return null;
			}

			try
			{
				var node = System.Text.Json.Nodes.JsonNode.Parse(payloadJson) as System.Text.Json.Nodes.JsonObject;
				var direct = node?["visualization_blob_url"]?.GetValue<string>();
				if (!string.IsNullOrWhiteSpace(direct))
				{
					return direct;
				}

				var legacy = node?["visualization"]?["url"]?.GetValue<string>();
				return string.IsNullOrWhiteSpace(legacy) ? null : legacy;
			}
			catch
			{
				return null;
			}
		}
	}
}
