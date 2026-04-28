using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using CleanOpsAi.Modules.Scoring.Application.Common.Interfaces.Services;
using CleanOpsAi.Modules.Scoring.Domain.Entities;
using CleanOpsAi.Modules.Scoring.Infrastructure.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Globalization;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace CleanOpsAi.Modules.Scoring.Infrastructure.Services
{
	public class ScoringAnnotationArtifactService : IScoringAnnotationArtifactService
	{
		private readonly IOptions<ScoringRetrainOptions> _options;
		private readonly IHttpClientFactory _httpClientFactory;
		private readonly ILogger<ScoringAnnotationArtifactService> _logger;

		public ScoringAnnotationArtifactService(
			IOptions<ScoringRetrainOptions> options,
			IHttpClientFactory httpClientFactory,
			ILogger<ScoringAnnotationArtifactService> logger)
		{
			_options = options;
			_httpClientFactory = httpClientFactory;
			_logger = logger;
		}

		public async Task EnsureReviewedSnapshotAsync(
			ScoringAnnotationCandidate candidate,
			ScoringJobResult result,
			CancellationToken ct = default)
		{
			var config = _options.Value;
			if (!config.ObjectStorageEnabled || !config.ReviewedSnapshotsEnabled)
			{
				_logger.LogDebug(
					"Skip reviewed snapshot publish for candidate {CandidateId}. ObjectStorageEnabled={StorageEnabled}, ReviewedSnapshotsEnabled={SnapshotsEnabled}",
					candidate.Id,
					config.ObjectStorageEnabled,
					config.ReviewedSnapshotsEnabled);
				return;
			}

			if (!string.IsNullOrWhiteSpace(candidate.SnapshotBlobKey) && !string.IsNullOrWhiteSpace(candidate.MetadataBlobKey))
			{
				return;
			}

			if (string.IsNullOrWhiteSpace(config.ObjectStorageConnectionString))
			{
				throw new InvalidOperationException("Scoring retrain object storage connection string is empty.");
			}

			var sourceUrl = string.IsNullOrWhiteSpace(candidate.ImageUrl) ? result.Source : candidate.ImageUrl;
			if (!TryParseHttpUri(sourceUrl, out var sourceUri))
			{
				throw new InvalidOperationException(
					$"Reviewed source is not a valid HTTP URL. candidate_id={candidate.Id}, source='{sourceUrl}'.");
			}

			var reviewedAtUtc = candidate.CreatedAtUtc == default
				? DateTime.UtcNow
				: DateTime.SpecifyKind(candidate.CreatedAtUtc, DateTimeKind.Utc);
			var capturedAtUtc = DateTime.UtcNow;
			var reviewToken = reviewedAtUtc.ToString("yyyyMMddTHHmmssfffZ", CultureInfo.InvariantCulture);
			var basePrefix = NormalizePrefix(config.ReviewedSamplesPrefix);
			var resultPrefix = BuildKey(
				basePrefix,
				$"reviewed/{reviewedAtUtc:yyyy}/{reviewedAtUtc:MM}/{reviewedAtUtc:dd}/job-{candidate.JobId:N}/result-{candidate.ResultId:N}/review-{reviewToken}");
			var metadataKey = BuildKey(resultPrefix, "metadata.json");

			var blobService = new BlobServiceClient(config.ObjectStorageConnectionString);
			var containerClient = blobService.GetBlobContainerClient(config.ReviewedSamplesContainer);
			await containerClient.CreateIfNotExistsAsync(cancellationToken: ct);

			var metadataBlob = containerClient.GetBlobClient(metadataKey);
			if (await metadataBlob.ExistsAsync(ct))
			{
				candidate.MetadataBlobKey = metadataKey;
				candidate.SnapshotBlobKey ??= await FindSiblingSnapshotKeyAsync(containerClient, resultPrefix, ct);
				return;
			}

			var httpClient = _httpClientFactory.CreateClient(nameof(ScoringAnnotationArtifactService));
			using var response = await httpClient.GetAsync(
				sourceUri,
				HttpCompletionOption.ResponseHeadersRead,
				ct);
			response.EnsureSuccessStatusCode();

			var contentType = response.Content.Headers.ContentType?.MediaType;
			if (string.IsNullOrWhiteSpace(contentType))
			{
				contentType = GuessContentType(sourceUri.AbsolutePath);
			}

			var extension = GuessExtension(sourceUri.AbsolutePath, contentType);
			var snapshotKey = BuildKey(resultPrefix, $"snapshot{extension}");
			var snapshotBlob = containerClient.GetBlobClient(snapshotKey);

			await using var imageStream = await response.Content.ReadAsStreamAsync(ct);
			var uploadResponse = await snapshotBlob.UploadAsync(
				imageStream,
				new BlobUploadOptions
				{
					HttpHeaders = new BlobHttpHeaders
					{
						ContentType = contentType,
					},
					Metadata = BuildSnapshotBlobMetadata(candidate, reviewToken),
				},
				ct);

			var metadata = BuildReviewedSnapshotMetadata(
				candidate,
				result,
				config.ReviewedSamplesContainer,
				contentType,
				snapshotKey,
				uploadResponse.Value.ETag.ToString(),
				capturedAtUtc,
				reviewedAtUtc);

			await metadataBlob.UploadAsync(
				BinaryData.FromString(metadata.ToJsonString(new JsonSerializerOptions { WriteIndented = true })),
				new BlobUploadOptions
				{
					HttpHeaders = new BlobHttpHeaders { ContentType = "application/json" },
				},
				ct);

			var manifestKey = BuildKey(
				basePrefix,
				$"manifests/reviewed/{reviewedAtUtc:yyyy}/{reviewedAtUtc:MM}/{reviewedAtUtc:dd}/manifest.json");
			await UpsertReviewedManifestAsync(
				containerClient,
				manifestKey,
				candidate,
				snapshotKey,
				metadataKey,
				reviewedAtUtc,
				capturedAtUtc,
				ct);

			candidate.SnapshotBlobKey = snapshotKey;
			candidate.MetadataBlobKey = metadataKey;
			candidate.VisualizationBlobUrl ??= ExtractVisualizationBlobUrl(result.PayloadJson);

			_logger.LogInformation(
				"Published reviewed snapshot for candidate {CandidateId}. snapshot_key={SnapshotKey}, metadata_key={MetadataKey}",
				candidate.Id,
				snapshotKey,
				metadataKey);
		}

		public async Task PublishApprovedAnnotationAsync(
			ScoringAnnotationCandidate candidate,
			ScoringAnnotation annotation,
			CancellationToken ct = default)
		{
			var config = _options.Value;
			if (!config.ObjectStorageEnabled || !config.ReviewedSnapshotsEnabled)
			{
				_logger.LogDebug(
					"Skip annotation artifact publish for candidate {CandidateId}. ObjectStorageEnabled={StorageEnabled}, ReviewedSnapshotsEnabled={SnapshotsEnabled}",
					candidate.Id,
					config.ObjectStorageEnabled,
					config.ReviewedSnapshotsEnabled);
				return;
			}

			if (string.IsNullOrWhiteSpace(config.ObjectStorageConnectionString))
			{
				throw new InvalidOperationException("Scoring retrain object storage connection string is empty.");
			}

			if (string.IsNullOrWhiteSpace(candidate.SnapshotBlobKey) || string.IsNullOrWhiteSpace(candidate.MetadataBlobKey))
			{
				throw new InvalidOperationException($"Annotation candidate {candidate.Id} is missing reviewed snapshot metadata.");
			}

			var approvedAtUtc = candidate.ApprovedAtUtc == default
				? DateTime.UtcNow
				: DateTime.SpecifyKind(candidate.ApprovedAtUtc!.Value, DateTimeKind.Utc);
			var basePrefix = NormalizePrefix(config.ReviewedSamplesPrefix);
			var approvalPrefix = BuildKey(
				basePrefix,
				$"annotations/approved/{approvedAtUtc:yyyy}/{approvedAtUtc:MM}/{approvedAtUtc:dd}/job-{candidate.JobId:N}/result-{candidate.ResultId:N}/candidate-{candidate.Id:N}");
			var annotationKey = BuildKey(approvalPrefix, "annotation.json");

			var root = new JsonObject
			{
				["schema_version"] = 1,
				["candidateId"] = candidate.Id.ToString(),
				["annotationId"] = annotation.Id.ToString(),
				["resultId"] = candidate.ResultId.ToString(),
				["jobId"] = candidate.JobId.ToString(),
				["requestId"] = candidate.RequestId,
				["environmentKey"] = candidate.EnvironmentKey,
				["imageUrl"] = candidate.ImageUrl,
				["visualizationBlobUrl"] = candidate.VisualizationBlobUrl,
				["sourceType"] = candidate.SourceType,
				["originalVerdict"] = candidate.OriginalVerdict,
				["reviewedVerdict"] = candidate.ReviewedVerdict,
				["snapshotKey"] = candidate.SnapshotBlobKey,
				["metadataKey"] = candidate.MetadataBlobKey,
				["approvedAtUtc"] = approvedAtUtc,
				["annotation"] = new JsonObject
				{
					["format"] = "bbox-region-v1",
					["reviewerNote"] = annotation.ReviewerNote,
					["version"] = annotation.Version,
					["createdByUserId"] = annotation.CreatedByUserId?.ToString(),
					["approvedByUserId"] = annotation.ApprovedByUserId?.ToString(),
					["labels"] = JsonNode.Parse(string.IsNullOrWhiteSpace(annotation.LabelsJson) ? "[]" : annotation.LabelsJson),
				}
			};

			var blobService = new BlobServiceClient(config.ObjectStorageConnectionString);
			var containerClient = blobService.GetBlobContainerClient(config.ReviewedSamplesContainer);
			await containerClient.CreateIfNotExistsAsync(cancellationToken: ct);

			var annotationBlob = containerClient.GetBlobClient(annotationKey);
			await annotationBlob.UploadAsync(
				BinaryData.FromString(root.ToJsonString(new JsonSerializerOptions { WriteIndented = true })),
				new BlobUploadOptions
				{
					HttpHeaders = new BlobHttpHeaders { ContentType = "application/json" },
				},
				ct);

			var manifestKey = BuildKey(
				basePrefix,
				$"manifests/annotations/approved/{approvedAtUtc:yyyy}/{approvedAtUtc:MM}/{approvedAtUtc:dd}/manifest.json");
			await UpsertManifestAsync(containerClient, manifestKey, candidate, annotation, annotationKey, approvedAtUtc, ct);

			_logger.LogInformation(
				"Published approved annotation artifact for candidate {CandidateId}. annotation_key={AnnotationKey}",
				candidate.Id,
				annotationKey);
		}

		private static async Task UpsertManifestAsync(
			BlobContainerClient containerClient,
			string manifestKey,
			ScoringAnnotationCandidate candidate,
			ScoringAnnotation annotation,
			string annotationKey,
			DateTime approvedAtUtc,
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
			root["date_utc"] = approvedAtUtc.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
			root["updated_at_utc"] = DateTime.UtcNow;

			var items = root["items"] as JsonArray ?? new JsonArray();
			var existingItem = items
				.OfType<JsonObject>()
				.FirstOrDefault(x => string.Equals(x["candidateId"]?.GetValue<string>(), candidate.Id.ToString(), StringComparison.OrdinalIgnoreCase));
			if (existingItem is not null)
			{
				items.Remove(existingItem);
			}

			items.Add(new JsonObject
			{
				["candidateId"] = candidate.Id.ToString(),
				["annotationId"] = annotation.Id.ToString(),
				["resultId"] = candidate.ResultId.ToString(),
				["jobId"] = candidate.JobId.ToString(),
				["requestId"] = candidate.RequestId,
				["environmentKey"] = candidate.EnvironmentKey,
				["annotationKey"] = annotationKey,
				["snapshotKey"] = candidate.SnapshotBlobKey,
				["metadataKey"] = candidate.MetadataBlobKey,
				["approvedAtUtc"] = approvedAtUtc,
			});

			root["items"] = items;
			await manifestBlob.UploadAsync(
				BinaryData.FromString(root.ToJsonString(new JsonSerializerOptions { WriteIndented = true })),
				new BlobUploadOptions
				{
					HttpHeaders = new BlobHttpHeaders { ContentType = "application/json" },
				},
				ct);
		}

		private static async Task UpsertReviewedManifestAsync(
			BlobContainerClient containerClient,
			string manifestKey,
			ScoringAnnotationCandidate candidate,
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
					["requestId"] = candidate.RequestId,
					["resultId"] = candidate.ResultId.ToString(),
					["jobId"] = candidate.JobId.ToString(),
					["reviewedVerdict"] = candidate.ReviewedVerdict,
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

		private static JsonObject BuildReviewedSnapshotMetadata(
			ScoringAnnotationCandidate candidate,
			ScoringJobResult result,
			string containerName,
			string? contentType,
			string snapshotKey,
			string etag,
			DateTime capturedAtUtc,
			DateTime reviewedAtUtc)
		{
			var reviewNode = ParseHumanReviewNode(result.PayloadJson);

			return new JsonObject
			{
				["schema_version"] = 1,
				["requestId"] = candidate.RequestId,
				["resultId"] = candidate.ResultId.ToString(),
				["jobId"] = candidate.JobId.ToString(),
				["source"] = result.Source,
				["sourceType"] = result.SourceType,
				["visualizationBlobUrl"] = candidate.VisualizationBlobUrl ?? ExtractVisualizationBlobUrl(result.PayloadJson),
				["environmentKey"] = candidate.EnvironmentKey,
				["originalVerdict"] = candidate.OriginalVerdict,
				["reviewedVerdict"] = candidate.ReviewedVerdict,
				["reviewReason"] = reviewNode?["review_reason"]?.GetValue<string>(),
				["reviewedAt"] = reviewedAtUtc,
				["reviewer"] = new JsonObject
				{
					["userId"] = reviewNode?["reviewed_by_user_id"]?.GetValue<string>(),
					["email"] = reviewNode?["reviewed_by_email"]?.GetValue<string>(),
				},
				["snapshot"] = new JsonObject
				{
					["container"] = containerName,
					["key"] = snapshotKey,
					["contentType"] = contentType,
					["etag"] = etag,
				},
				["snapshotKey"] = snapshotKey,
				["capturedAtUtc"] = capturedAtUtc,
			};
		}

		private static JsonObject? ParseHumanReviewNode(string? payloadJson)
		{
			if (string.IsNullOrWhiteSpace(payloadJson))
			{
				return null;
			}

			try
			{
				return JsonNode.Parse(payloadJson)?["human_review"] as JsonObject;
			}
			catch
			{
				return null;
			}
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
			ScoringAnnotationCandidate candidate,
			string reviewToken)
		{
			return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
			{
				["request_id"] = candidate.RequestId,
				["result_id"] = candidate.ResultId.ToString("N"),
				["job_id"] = candidate.JobId.ToString("N"),
				["reviewed_verdict"] = candidate.ReviewedVerdict,
				["reviewed_at_token"] = reviewToken,
				["source_type"] = candidate.SourceType,
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

		private static async Task<string?> FindSiblingSnapshotKeyAsync(
			BlobContainerClient containerClient,
			string resultPrefix,
			CancellationToken ct)
		{
			await foreach (var item in containerClient.GetBlobsAsync(prefix: resultPrefix, cancellationToken: ct))
			{
				if (item.Name.StartsWith($"{resultPrefix}/snapshot", StringComparison.OrdinalIgnoreCase))
				{
					return item.Name;
				}
			}

			return null;
		}
	}
}
