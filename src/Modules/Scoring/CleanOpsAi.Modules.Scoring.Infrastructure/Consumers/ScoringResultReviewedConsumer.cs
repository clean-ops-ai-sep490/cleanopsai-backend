using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using CleanOpsAi.Modules.Scoring.Application.Common.Interfaces.Repositories;
using CleanOpsAi.Modules.Scoring.Domain.Entities;
using CleanOpsAi.Modules.Scoring.Domain.Enums;
using CleanOpsAi.Modules.Scoring.IntegrationEvents;
using CleanOpsAi.Modules.Scoring.Infrastructure.Options;
using MassTransit;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace CleanOpsAi.Modules.Scoring.Infrastructure.Consumers
{
	public class ScoringResultReviewedConsumer : IConsumer<ScoringResultReviewedEvent>
	{
		private readonly IOptions<ScoringRetrainOptions> _options;
		private readonly IHttpClientFactory _httpClientFactory;
		private readonly IScoringJobRepository _repository;
		private readonly ILogger<ScoringResultReviewedConsumer> _logger;

		public ScoringResultReviewedConsumer(
			IOptions<ScoringRetrainOptions> options,
			IHttpClientFactory httpClientFactory,
			IScoringJobRepository repository,
			ILogger<ScoringResultReviewedConsumer> logger)
		{
			_options = options;
			_httpClientFactory = httpClientFactory;
			_repository = repository;
			_logger = logger;
		}

		public async Task Consume(ConsumeContext<ScoringResultReviewedEvent> context)
		{
			var config = _options.Value;
			var message = context.Message;
			var reviewedAtUtc = message.ReviewedAtUtc == default
				? DateTime.UtcNow
				: DateTime.SpecifyKind(message.ReviewedAtUtc, DateTimeKind.Utc);
			var candidate = await EnsureAnnotationCandidateAsync(message, reviewedAtUtc, context.CancellationToken);

			if (!config.ObjectStorageEnabled || !config.ReviewedSnapshotsEnabled)
			{
				_logger.LogDebug(
					"Skip reviewed snapshot upload for result {ResultId}. ObjectStorageEnabled={StorageEnabled}, ReviewedSnapshotsEnabled={SnapshotsEnabled}",
					message.ResultId,
					config.ObjectStorageEnabled,
					config.ReviewedSnapshotsEnabled);
				return;
			}

			if (string.IsNullOrWhiteSpace(config.ObjectStorageConnectionString))
			{
				throw new InvalidOperationException(
					$"ScoringRetrain:ObjectStorageConnectionString is empty while reviewed snapshots are enabled. result_id={message.ResultId}");
			}

			if (!TryParseHttpUri(message.Source, out var sourceUri))
			{
				throw new InvalidOperationException(
					$"Reviewed source is not a valid HTTP URL. result_id={message.ResultId}, source='{message.Source}'.");
			}

			var capturedAtUtc = DateTime.UtcNow;
			var reviewToken = reviewedAtUtc.ToString("yyyyMMddTHHmmssfffZ", CultureInfo.InvariantCulture);

			var basePrefix = NormalizePrefix(config.ReviewedSamplesPrefix);
			var resultPrefix = BuildKey(
				basePrefix,
				$"reviewed/{reviewedAtUtc:yyyy}/{reviewedAtUtc:MM}/{reviewedAtUtc:dd}/job-{message.JobId:N}/result-{message.ResultId:N}/review-{reviewToken}");
			var metadataKey = BuildKey(resultPrefix, "metadata.json");

			var blobService = new BlobServiceClient(config.ObjectStorageConnectionString);
			var containerClient = blobService.GetBlobContainerClient(config.ReviewedSamplesContainer);
			await containerClient.CreateIfNotExistsAsync(cancellationToken: context.CancellationToken);

			var metadataBlob = containerClient.GetBlobClient(metadataKey);
			if (await metadataBlob.ExistsAsync(context.CancellationToken))
			{
				_logger.LogInformation(
					"Reviewed snapshot already exists for result {ResultId}. metadata_key={MetadataKey}",
					message.ResultId,
					metadataKey);
				return;
			}

			var httpClient = _httpClientFactory.CreateClient(nameof(ScoringResultReviewedConsumer));
			using var response = await httpClient.GetAsync(
				sourceUri,
				HttpCompletionOption.ResponseHeadersRead,
				context.CancellationToken);
			response.EnsureSuccessStatusCode();

			var contentType = response.Content.Headers.ContentType?.MediaType;
			if (string.IsNullOrWhiteSpace(contentType))
			{
				contentType = GuessContentType(sourceUri.AbsolutePath);
			}

			var extension = GuessExtension(sourceUri.AbsolutePath, contentType);
			var snapshotKey = BuildKey(resultPrefix, $"snapshot{extension}");
			var snapshotBlob = containerClient.GetBlobClient(snapshotKey);

			await using var imageStream = await response.Content.ReadAsStreamAsync(context.CancellationToken);
			var uploadResponse = await snapshotBlob.UploadAsync(
				imageStream,
				new BlobUploadOptions
				{
					HttpHeaders = new BlobHttpHeaders
					{
						ContentType = contentType,
					},
					Metadata = BuildSnapshotBlobMetadata(message, reviewToken),
				},
				context.CancellationToken);

			var snapshotMetadata = new JsonObject
			{
				["schema_version"] = 1,
				["requestId"] = message.RequestId,
				["resultId"] = message.ResultId.ToString(),
				["jobId"] = message.JobId.ToString(),
				["source"] = message.Source,
				["sourceType"] = message.SourceType,
				["visualizationBlobUrl"] = candidate?.VisualizationBlobUrl,
				["environmentKey"] = message.EnvironmentKey,
				["originalVerdict"] = message.OriginalVerdict,
				["reviewedVerdict"] = message.ReviewedVerdict,
				["reviewReason"] = message.ReviewReason,
				["reviewedAt"] = reviewedAtUtc,
				["reviewer"] = new JsonObject
				{
					["userId"] = message.ReviewedByUserId?.ToString(),
					["email"] = message.ReviewedByEmail,
				},
				["snapshot"] = new JsonObject
				{
					["container"] = config.ReviewedSamplesContainer,
					["key"] = snapshotKey,
					["contentType"] = contentType,
					["etag"] = uploadResponse.Value.ETag.ToString(),
				},
				["snapshotKey"] = snapshotKey,
				["capturedAtUtc"] = capturedAtUtc,
			};

			await metadataBlob.UploadAsync(
				BinaryData.FromString(snapshotMetadata.ToJsonString(new JsonSerializerOptions { WriteIndented = true })),
				new BlobUploadOptions
				{
					HttpHeaders = new BlobHttpHeaders { ContentType = "application/json" },
				},
				context.CancellationToken);

			var dailyManifestKey = BuildKey(
				basePrefix,
				$"manifests/reviewed/{reviewedAtUtc:yyyy}/{reviewedAtUtc:MM}/{reviewedAtUtc:dd}/manifest.json");
			await UpsertDailyManifestAsync(
				containerClient,
				dailyManifestKey,
				message,
				snapshotKey,
				metadataKey,
				reviewedAtUtc,
				capturedAtUtc,
				context.CancellationToken);

			if (candidate is not null)
			{
				candidate.SnapshotBlobKey = snapshotKey;
				candidate.MetadataBlobKey = metadataKey;
				candidate.LastModified = DateTime.UtcNow;
				candidate.LastModifiedBy = "system";
				await _repository.SaveChangesAsync(context.CancellationToken);
			}

			_logger.LogInformation(
				"Uploaded reviewed snapshot for result {ResultId}. snapshot_key={SnapshotKey}, metadata_key={MetadataKey}",
				message.ResultId,
				snapshotKey,
				metadataKey);
		}

		private async Task<ScoringAnnotationCandidate?> EnsureAnnotationCandidateAsync(
			ScoringResultReviewedEvent message,
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

			var result = await _repository.GetResultByIdWithJobAsync(message.ResultId, ct);
			if (result is null)
			{
				_logger.LogWarning(
					"Unable to create annotation candidate because scoring result {ResultId} was not found.",
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

		private static async Task UpsertDailyManifestAsync(
			BlobContainerClient containerClient,
			string manifestKey,
			ScoringResultReviewedEvent message,
			string snapshotKey,
			string metadataKey,
			DateTime reviewedAtUtc,
			DateTime capturedAtUtc,
			CancellationToken ct)
		{
			var manifestBlob = containerClient.GetBlobClient(manifestKey);

			JsonObject root;
			if (await manifestBlob.ExistsAsync(ct))
			{
				var existing = await manifestBlob.DownloadContentAsync(ct);
				root = JsonNode.Parse(existing.Value.Content.ToString()) as JsonObject ?? new JsonObject();
			}
			else
			{
				root = new JsonObject();
			}

			root["schema_version"] = 1;
			root["date_utc"] = reviewedAtUtc.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
			root["updated_at_utc"] = capturedAtUtc;

			var items = root["items"] as JsonArray ?? new JsonArray();
			var duplicate = items
				.OfType<JsonObject>()
				.Any(i => string.Equals(i["metadataKey"]?.GetValue<string>(), metadataKey, StringComparison.Ordinal));

			if (!duplicate)
			{
				items.Add(new JsonObject
				{
					["requestId"] = message.RequestId,
					["resultId"] = message.ResultId.ToString(),
					["jobId"] = message.JobId.ToString(),
					["reviewedVerdict"] = message.ReviewedVerdict,
					["reviewedAt"] = reviewedAtUtc,
					["snapshotKey"] = snapshotKey,
					["metadataKey"] = metadataKey,
					["capturedAtUtc"] = capturedAtUtc,
				});
			}

			root["items"] = items;

			await manifestBlob.UploadAsync(
				BinaryData.FromString(root.ToJsonString(new JsonSerializerOptions { WriteIndented = true })),
				new BlobUploadOptions
				{
					HttpHeaders = new BlobHttpHeaders { ContentType = "application/json" },
				},
				ct);
		}

		private static bool TryParseHttpUri(string? raw, out Uri uri)
		{
			uri = null!;
			if (string.IsNullOrWhiteSpace(raw))
			{
				return false;
			}

			if (!Uri.TryCreate(raw.Trim(), UriKind.Absolute, out var parsed))
			{
				return false;
			}

			if (parsed.Scheme != Uri.UriSchemeHttp && parsed.Scheme != Uri.UriSchemeHttps)
			{
				return false;
			}

			uri = parsed;
			return true;
		}

		private static string NormalizePrefix(string? prefix)
		{
			var normalized = (prefix ?? string.Empty).Trim().Trim('/');
			return string.IsNullOrWhiteSpace(normalized) ? "scoring/retrain-samples" : normalized;
		}

		private static string BuildKey(string prefix, string suffix)
		{
			var left = (prefix ?? string.Empty).Trim('/');
			var right = (suffix ?? string.Empty).Trim('/');

			if (string.IsNullOrWhiteSpace(left))
			{
				return right;
			}

			return string.IsNullOrWhiteSpace(right) ? left : $"{left}/{right}";
		}

		private static string GuessExtension(string path, string? contentType)
		{
			var ext = Path.GetExtension(path)?.Trim().ToLowerInvariant();
			if (ext is ".jpg" or ".jpeg" or ".png" or ".webp" or ".gif" or ".bmp")
			{
				return ext;
			}

			return contentType?.ToLowerInvariant() switch
			{
				"image/jpeg" => ".jpg",
				"image/png" => ".png",
				"image/webp" => ".webp",
				"image/gif" => ".gif",
				"image/bmp" => ".bmp",
				_ => ".bin",
			};
		}

		private static string GuessContentType(string path)
		{
			var ext = Path.GetExtension(path)?.Trim().ToLowerInvariant();
			return ext switch
			{
				".jpg" => "image/jpeg",
				".jpeg" => "image/jpeg",
				".png" => "image/png",
				".webp" => "image/webp",
				".gif" => "image/gif",
				".bmp" => "image/bmp",
				_ => "application/octet-stream",
			};
		}

		private static Dictionary<string, string> BuildSnapshotBlobMetadata(
			ScoringResultReviewedEvent message,
			string reviewToken)
		{
			return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
			{
				["request_id"] = message.RequestId,
				["result_id"] = message.ResultId.ToString("N"),
				["job_id"] = message.JobId.ToString("N"),
				["reviewed_verdict"] = message.ReviewedVerdict,
				["reviewed_at_token"] = reviewToken,
				["source_type"] = message.SourceType,
			};
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
