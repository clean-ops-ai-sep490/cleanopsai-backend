using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using CleanOpsAi.BuildingBlocks.Application.Interfaces.Messaging;
using CleanOpsAi.Modules.Scoring.Application.Common.Interfaces.Repositories;
using CleanOpsAi.Modules.Scoring.Application.Common.Interfaces.Services;
using CleanOpsAi.Modules.Scoring.Domain.Entities;
using CleanOpsAi.Modules.Scoring.Domain.Enums;
using CleanOpsAi.Modules.Scoring.Infrastructure.Options;
using CleanOpsAi.Modules.Scoring.IntegrationEvents;
using MassTransit;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.Globalization;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace CleanOpsAi.Modules.Scoring.Infrastructure.Consumers
{
	public class ScoringRetrainRequestedConsumer : IConsumer<ScoringRetrainRequestedEvent>, IScoringRetrainRequestHandler
	{
		private readonly IEventBus _eventBus;
		private readonly IOptions<ScoringRetrainOptions> _options;
		private readonly ILogger<ScoringRetrainRequestedConsumer> _logger;
		private readonly IHttpClientFactory _httpClientFactory;
		private readonly IScoringJobRepository _repository;

		public ScoringRetrainRequestedConsumer(
			IEventBus eventBus,
			IOptions<ScoringRetrainOptions> options,
			ILogger<ScoringRetrainRequestedConsumer> logger,
			IHttpClientFactory httpClientFactory,
			IScoringJobRepository repository)
		{
			_eventBus = eventBus;
			_options = options;
			_logger = logger;
			_httpClientFactory = httpClientFactory;
			_repository = repository;
		}

		public async Task Consume(ConsumeContext<ScoringRetrainRequestedEvent> context)
		{
			await HandleAsync(context.Message, context.CancellationToken);
		}

		public bool InlineExecutionEnabled => ResolveInlineExecutionEnabled();

		public async Task HandleAsync(ScoringRetrainRequestedEvent message, CancellationToken ct = default)
		{
			var config = _options.Value;
			var now = DateTime.UtcNow;
			var remoteTrainerEnabled = ResolveRemoteTrainerEnabled(config);
			var useExternalCandidate = config.ObjectStorageEnabled
				&& config.UseExternalCandidateForRetrain
				&& !string.IsNullOrWhiteSpace(config.ExternalCandidatePrefix);

			_logger.LogInformation(
				"Scoring retrain batch {BatchId} starting. RemoteTrainerEnabled={RemoteTrainerEnabled}. UseExternalCandidate={UseExternalCandidate}. ExternalCandidatePrefix={ExternalCandidatePrefix}. TrainerCommandConfigured={TrainerCommandConfigured}.",
				message.BatchId,
				remoteTrainerEnabled,
				useExternalCandidate,
				config.ExternalCandidatePrefix,
				!string.IsNullOrWhiteSpace(config.TrainerCommand));

			var batch = await EnsureBatchAsync(message, now, ct);
			var run = new ScoringRetrainRun
			{
				Id = Guid.NewGuid(),
				ScoringRetrainBatchId = batch.Id,
				Status = ScoringRetrainRunStatus.Running,
				Mode = ResolveRunMode(useExternalCandidate, remoteTrainerEnabled),
				StartedAtUtc = now,
				Created = now,
				LastModified = now,
				CreatedBy = "system",
				LastModifiedBy = "system",
			};

			await _repository.InsertRetrainRunAsync(run, ct);
			batch.Status = ScoringRetrainBatchStatus.Running;
			batch.CompletedAtUtc = null;
			batch.FailureReason = null;
			batch.Promoted = false;
			batch.PromotionReason = null;
			batch.LastModified = now;
			batch.LastModifiedBy = "system";
			await _repository.SaveChangesAsync(ct);

			if (!remoteTrainerEnabled && !useExternalCandidate && string.IsNullOrWhiteSpace(config.TrainerCommand))
			{
				var reason = $"Trainer command is empty. Skip retrain batch {message.BatchId}.";
				await MarkBatchFailedAsync(batch, run, reason, 1, ct);
				_logger.LogWarning(reason);
				return;
			}

			if (config.ReviewedBridgeEnabled
				&& message.ReviewedSampleCount > 0
				&& !string.IsNullOrWhiteSpace(config.ReviewedBridgeCommand))
			{
				var bridgeExecution = await RunCommandAsync(
					config.ReviewedBridgeCommand,
					config.TrainerWorkingDirectory,
					Math.Max(30, config.TrainerTimeoutSeconds),
					ct);

				if (bridgeExecution.ExitCode != 0)
				{
					var bridgeReason = $"Reviewed bridge command failed with exit code {bridgeExecution.ExitCode}.";
					await _eventBus.PublishAsync(new ScoringRetrainExecutionResultEvent
					{
						BatchId = message.BatchId,
						CompletedAtUtc = DateTime.UtcNow,
						Succeeded = false,
						ExitCode = bridgeExecution.ExitCode,
						Message = bridgeReason,
					}, ct);

					await MarkBatchFailedAsync(batch, run, bridgeReason, bridgeExecution.ExitCode, ct);
					_logger.LogError(
						"Scoring reviewed bridge command failed for batch {BatchId}. ExitCode={ExitCode}. StdErr={StdErr}",
						message.BatchId,
						bridgeExecution.ExitCode,
						Truncate(bridgeExecution.StandardError, 2000));
					return;
				}

				_logger.LogInformation(
					"Scoring reviewed bridge command completed for batch {BatchId}. StdOut={StdOut}",
					message.BatchId,
					Truncate(bridgeExecution.StandardOutput, 1000));
			}

			CommandExecutionResult execution;
			if (remoteTrainerEnabled)
			{
				execution = await RunRemoteTrainerAsync(message, config, ct);
				if (execution.ExitCode == 0 && !string.IsNullOrWhiteSpace(config.ExternalCandidatePrefix))
				{
					useExternalCandidate = true;
				}
			}
			else if (useExternalCandidate)
			{
				execution = new CommandExecutionResult(
					0,
					string.Empty,
					$"Using external candidate prefix '{config.ExternalCandidatePrefix}'.");
			}
			else
			{
				execution = await RunCommandAsync(
					config.TrainerCommand,
					config.TrainerWorkingDirectory,
					Math.Max(30, config.TrainerTimeoutSeconds),
					ct);
			}

			var executionCompletedAt = DateTime.UtcNow;
			await _eventBus.PublishAsync(new ScoringRetrainExecutionResultEvent
			{
				BatchId = message.BatchId,
				CompletedAtUtc = executionCompletedAt,
				Succeeded = execution.ExitCode == 0,
				ExitCode = execution.ExitCode,
				Message = execution.ExitCode == 0
					? "Trainer command completed successfully."
					: $"Trainer command failed with exit code {execution.ExitCode}.",
			}, ct);

			if (execution.ExitCode != 0)
			{
				await MarkBatchFailedAsync(
					batch,
					run,
					$"Trainer command failed with exit code {execution.ExitCode}. {Truncate(execution.StandardError, 1500)}".Trim(),
					execution.ExitCode,
					ct);

				_logger.LogError(
					"Scoring retrain batch {BatchId} failed. ExitCode={ExitCode}. StdErr={StdErr}",
					message.BatchId,
					execution.ExitCode,
					Truncate(execution.StandardError, 2000));
				return;
			}

			PromotionGateResult gateResult;
			if (config.ObjectStorageEnabled)
			{
				gateResult = await EvaluateAndPromoteViaObjectStorageAsync(message, config, useExternalCandidate, ct);
			}
			else
			{
				gateResult = await EvaluatePromotionGateAsync(config, ct);
			}

			if (gateResult.Promoted && !string.IsNullOrWhiteSpace(config.PromotionCommand))
			{
				var promotionExecution = await RunCommandAsync(
					config.PromotionCommand,
					config.TrainerWorkingDirectory,
					Math.Max(30, config.TrainerTimeoutSeconds),
					ct);

				if (promotionExecution.ExitCode != 0)
				{
					gateResult = gateResult with
					{
						Promoted = false,
						Reason = $"Promotion command failed with exit code {promotionExecution.ExitCode}.",
					};
				}
			}

			if (gateResult.Promoted && !string.IsNullOrWhiteSpace(config.RestartServiceCommand))
			{
				var restartExecution = await RunCommandAsync(
					config.RestartServiceCommand,
					config.TrainerWorkingDirectory,
					Math.Max(30, config.TrainerTimeoutSeconds),
					ct);

				if (restartExecution.ExitCode != 0)
				{
					gateResult = gateResult with
					{
						Reason = $"{gateResult.Reason} Restart command failed with exit code {restartExecution.ExitCode}. Active model is already promoted in object storage; manual restart is required.",
					};
				}
			}

			var finalCompletedAt = DateTime.UtcNow;
			await _eventBus.PublishAsync(new ScoringModelPromotionEvaluatedEvent
			{
				BatchId = message.BatchId,
				EvaluatedAtUtc = finalCompletedAt,
				MetricKey = $"{config.YoloMapMetricKey}+{config.UnetMiouMetricKey}",
				CandidateMetric = gateResult.CandidateCompositeMetric,
				BaselineMetric = gateResult.BaselineCompositeMetric,
				MinimumImprovement = gateResult.MinimumImprovement,
				Promoted = gateResult.Promoted,
				Reason = gateResult.Reason,
			}, ct);

			await MarkBatchCompletedAsync(batch, run, gateResult, finalCompletedAt, ct);

			_logger.LogInformation(
				"Scoring model promotion evaluated for batch {BatchId}. Promoted={Promoted}. Candidate={Candidate}. Baseline={Baseline}. Reason={Reason}",
				message.BatchId,
				gateResult.Promoted,
				gateResult.CandidateCompositeMetric,
				gateResult.BaselineCompositeMetric,
				gateResult.Reason);
		}

		private async Task<ScoringRetrainBatch> EnsureBatchAsync(
			ScoringRetrainRequestedEvent message,
			DateTime now,
			CancellationToken ct)
		{
			var batch = await _repository.GetRetrainBatchByIdWithRunsAsync(message.BatchId, ct);
			if (batch is not null)
			{
				return batch;
			}

			batch = new ScoringRetrainBatch
			{
				Id = message.BatchId,
				RequestedAtUtc = message.RequestedAtUtc,
				SourceWindowFromUtc = message.SourceWindowFromUtc,
				ReviewedSampleCount = message.ReviewedSampleCount,
				ApprovedAnnotationCount = message.ApprovedAnnotationCount,
				CalibrationSampleCount = message.ApprovedAnnotationCount,
				Status = ScoringRetrainBatchStatus.Queued,
				Created = now,
				LastModified = now,
				CreatedBy = "system",
				LastModifiedBy = "system",
			};

			await _repository.InsertRetrainBatchAsync(batch, ct);
			await _repository.SaveChangesAsync(ct);
			return batch;
		}

		private async Task MarkBatchFailedAsync(
			ScoringRetrainBatch batch,
			ScoringRetrainRun run,
			string reason,
			int? exitCode,
			CancellationToken ct)
		{
			var now = DateTime.UtcNow;
			run.Status = ScoringRetrainRunStatus.Failed;
			run.ExitCode = exitCode;
			run.CompletedAtUtc = now;
			run.Message = reason;
			run.LastModified = now;
			run.LastModifiedBy = "system";

			batch.Status = ScoringRetrainBatchStatus.Failed;
			batch.CompletedAtUtc = now;
			batch.FailureReason = reason.Length > 2000 ? reason[..2000] : reason;
			batch.Promoted = false;
			batch.PromotionReason = null;
			batch.LastModified = now;
			batch.LastModifiedBy = "system";

			await _repository.SaveChangesAsync(ct);
		}

		private async Task MarkBatchCompletedAsync(
			ScoringRetrainBatch batch,
			ScoringRetrainRun run,
			PromotionGateResult gateResult,
			DateTime completedAtUtc,
			CancellationToken ct)
		{
			var isOperationalFailure = !gateResult.Promoted
				&& gateResult.Reason.Contains("failed", StringComparison.OrdinalIgnoreCase)
				&& !gateResult.Reason.StartsWith("Rejected:", StringComparison.OrdinalIgnoreCase);

			run.Status = gateResult.Promoted
				? ScoringRetrainRunStatus.Promoted
				: isOperationalFailure
					? ScoringRetrainRunStatus.Failed
					: ScoringRetrainRunStatus.Rejected;
			run.ExitCode = gateResult.Promoted ? 0 : run.ExitCode;
			run.CompletedAtUtc = completedAtUtc;
			run.Message = gateResult.Reason;
			run.LastModified = completedAtUtc;
			run.LastModifiedBy = "system";

			batch.Status = gateResult.Promoted
				? ScoringRetrainBatchStatus.Promoted
				: isOperationalFailure
					? ScoringRetrainBatchStatus.Failed
					: ScoringRetrainBatchStatus.Rejected;
			batch.CompletedAtUtc = completedAtUtc;
			batch.Promoted = gateResult.Promoted;
			batch.MetricKey = gateResult.MetricKey;
			batch.CandidateMetric = gateResult.CandidateCompositeMetric;
			batch.BaselineMetric = gateResult.BaselineCompositeMetric;
			batch.MinimumImprovement = gateResult.MinimumImprovement;
			batch.PromotionReason = gateResult.Reason.Length > 2000 ? gateResult.Reason[..2000] : gateResult.Reason;
			batch.FailureReason = batch.Status == ScoringRetrainBatchStatus.Failed ? batch.PromotionReason : null;
			batch.LastModified = completedAtUtc;
			batch.LastModifiedBy = "system";

			await _repository.SaveChangesAsync(ct);
		}

		private static string ResolveRunMode(bool useExternalCandidate, bool remoteTrainerEnabled)
		{
			if (remoteTrainerEnabled)
			{
				return "remote-trainer";
			}

			return useExternalCandidate ? "external-candidate" : "trainer-command";
		}

		private static bool ResolveRemoteTrainerEnabled(ScoringRetrainOptions options)
		{
			if (options.RemoteTrainerEnabled)
			{
				return true;
			}

			var enabledFromEnv = TryReadBooleanEnvironmentVariable("ScoringRetrain__RemoteTrainerEnabled")
				?? TryReadBooleanEnvironmentVariable("SCORING_RETRAIN_REMOTE_ENABLED")
				?? false;

			if (enabledFromEnv)
			{
				return true;
			}

			var remoteBaseUrl = !string.IsNullOrWhiteSpace(options.RemoteTrainerBaseUrl)
				? options.RemoteTrainerBaseUrl
				: Environment.GetEnvironmentVariable("ScoringRetrain__RemoteTrainerBaseUrl")
					?? Environment.GetEnvironmentVariable("SCORING_RETRAIN_REMOTE_BASE_URL");

			var remoteApiKey = !string.IsNullOrWhiteSpace(options.RemoteTrainerApiKey)
				? options.RemoteTrainerApiKey
				: Environment.GetEnvironmentVariable("ScoringRetrain__RemoteTrainerApiKey")
					?? Environment.GetEnvironmentVariable("SCORING_RETRAIN_REMOTE_API_KEY");

			return !string.IsNullOrWhiteSpace(remoteBaseUrl)
				&& !string.IsNullOrWhiteSpace(remoteApiKey);
		}

		private static bool? TryReadBooleanEnvironmentVariable(string name)
		{
			var raw = Environment.GetEnvironmentVariable(name);
			if (string.IsNullOrWhiteSpace(raw))
			{
				return null;
			}

			if (bool.TryParse(raw, out var parsed))
			{
				return parsed;
			}

			return raw.Trim() switch
			{
				"1" => true,
				"0" => false,
				_ => null,
			};
		}

		private static bool ResolveInlineExecutionEnabled()
		{
			return TryReadBooleanEnvironmentVariable("ScoringRetrain__InlineExecutionEnabled")
				?? TryReadBooleanEnvironmentVariable("SCORING_RETRAIN_INLINE_ENABLED")
				?? false;
		}

		private async Task<PromotionGateResult> EvaluateAndPromoteViaObjectStorageAsync(
			ScoringRetrainRequestedEvent message,
			ScoringRetrainOptions options,
			bool useExternalCandidate,
			CancellationToken ct)
		{
			var blobService = CreateBlobServiceClient(options);
			var modelsContainer = blobService.GetBlobContainerClient(options.ObjectStorageModelsContainer);
			var retrainContainer = blobService.GetBlobContainerClient(options.ObjectStorageRetrainContainer);

			await EnsureContainerExistsAsync(modelsContainer, ct);
			await EnsureContainerExistsAsync(retrainContainer, ct);

			var hasExternalCandidate = useExternalCandidate && !string.IsNullOrWhiteSpace(options.ExternalCandidatePrefix);
			var batchPrefix = hasExternalCandidate
				? (options.ExternalCandidatePrefix ?? string.Empty)
				: BuildObjectKey(options.CandidatePrefix, message.BatchId.ToString("N"));
			var candidateYoloKey = BuildObjectKey(batchPrefix, "yolo/model.pt");
			var candidateUnetKey = BuildObjectKey(batchPrefix, "unet/model.pth");
			var candidateMetricsKey = BuildObjectKey(batchPrefix, "metrics/metrics.json");
			var candidateManifestKey = BuildObjectKey(options.ManifestPrefix, $"candidate_{message.BatchId:N}.json");

			JsonNode? candidateMetrics;
			if (hasExternalCandidate)
			{
				var hasYolo = await ObjectExistsAsync(retrainContainer, candidateYoloKey, ct);
				var hasUnet = await ObjectExistsAsync(retrainContainer, candidateUnetKey, ct);
				if (!hasYolo || !hasUnet)
				{
					return new PromotionGateResult(
						false,
						0,
						null,
						null,
						null,
						$"External candidate artifacts not found at prefix '{batchPrefix}'.",
						$"{options.YoloMapMetricKey}+{options.UnetMiouMetricKey}",
						options.MinimumYoloMapImprovement + options.MinimumUnetMiouImprovement);
				}

				candidateMetrics = await DownloadJsonNodeAsync(retrainContainer, candidateMetricsKey, ct);
			}
			else
			{
				var candidateMetricsPath = ResolvePath(options.CandidateMetricsFile, options.TrainerWorkingDirectory);
				var candidateYoloPath = ResolvePath(options.CandidateYoloModelFile, options.TrainerWorkingDirectory);
				var candidateUnetPath = ResolvePath(options.CandidateUnetModelFile, options.TrainerWorkingDirectory);

				if (!File.Exists(candidateMetricsPath) || !File.Exists(candidateYoloPath) || !File.Exists(candidateUnetPath))
				{
					return new PromotionGateResult(
						false,
						0,
						null,
						null,
						null,
						$"Candidate artifacts not found. metrics={candidateMetricsPath}, yolo={candidateYoloPath}, unet={candidateUnetPath}",
						$"{options.YoloMapMetricKey}+{options.UnetMiouMetricKey}",
						options.MinimumYoloMapImprovement + options.MinimumUnetMiouImprovement);
				}

				candidateMetrics = await ReadJsonFileAsNodeAsync(candidateMetricsPath, ct);
				await UploadFileAsync(retrainContainer, candidateYoloKey, candidateYoloPath, "application/octet-stream", ct);
				await UploadFileAsync(retrainContainer, candidateUnetKey, candidateUnetPath, "application/octet-stream", ct);
				await UploadFileAsync(retrainContainer, candidateMetricsKey, candidateMetricsPath, "application/json", ct);
			}

			if (candidateMetrics is null)
			{
				return new PromotionGateResult(
					false,
					0,
					null,
					null,
					null,
					"Candidate metrics JSON cannot be parsed.",
					$"{options.YoloMapMetricKey}+{options.UnetMiouMetricKey}",
					options.MinimumYoloMapImprovement + options.MinimumUnetMiouImprovement);
			}

			var candidateYoloMap = ReadMetricFromNode(candidateMetrics, options.YoloMapMetricKey);
			var candidateUnetMiou = ReadMetricFromNode(candidateMetrics, options.UnetMiouMetricKey);
			if (candidateYoloMap is null || candidateUnetMiou is null)
			{
				return new PromotionGateResult(
					false,
					0,
					null,
					candidateYoloMap,
					candidateUnetMiou,
					$"Candidate metrics missing keys '{options.YoloMapMetricKey}' or '{options.UnetMiouMetricKey}'.",
					$"{options.YoloMapMetricKey}+{options.UnetMiouMetricKey}",
					options.MinimumYoloMapImprovement + options.MinimumUnetMiouImprovement);
			}

			var baselineMetricsNode = await DownloadJsonNodeAsync(modelsContainer, options.ActiveMetricsObjectKey, ct);
			var baselineYoloMap = ReadMetricFromNode(baselineMetricsNode, options.YoloMapMetricKey);
			var baselineUnetMiou = ReadMetricFromNode(baselineMetricsNode, options.UnetMiouMetricKey);

			var gateResult = EvaluateDualGate(
				candidateYoloMap.Value,
				candidateUnetMiou.Value,
				baselineYoloMap,
				baselineUnetMiou,
				options);

			var candidateManifest = new JsonObject
			{
				["batch_id"] = message.BatchId.ToString(),
				["created_at_utc"] = DateTime.UtcNow,
				["models_container"] = options.ObjectStorageModelsContainer,
				["retrain_container"] = options.ObjectStorageRetrainContainer,
				["candidate_yolo_key"] = candidateYoloKey,
				["candidate_unet_key"] = candidateUnetKey,
				["candidate_metrics_key"] = candidateMetricsKey,
				["candidate_yolo_map"] = candidateYoloMap.Value,
				["candidate_unet_miou"] = candidateUnetMiou.Value,
				["baseline_yolo_map"] = baselineYoloMap,
				["baseline_unet_miou"] = baselineUnetMiou,
				["promoted"] = gateResult.Promoted,
				["reason"] = gateResult.Reason,
			};

			await UploadTextAsync(
				retrainContainer,
				candidateManifestKey,
				candidateManifest.ToJsonString(new JsonSerializerOptions { WriteIndented = true }),
				"application/json",
				ct);

			if (!gateResult.Promoted)
			{
				return gateResult;
			}

			var archivePrefix = BuildObjectKey(options.ArchivePrefix, DateTime.UtcNow.ToString("yyyyMMddHHmmss", CultureInfo.InvariantCulture));
			await TryArchiveObjectAsync(modelsContainer, options.ActiveYoloObjectKey, retrainContainer, BuildObjectKey(archivePrefix, "yolo/model.pt"), ct);
			await TryArchiveObjectAsync(modelsContainer, options.ActiveUnetObjectKey, retrainContainer, BuildObjectKey(archivePrefix, "unet/model.pth"), ct);
			await TryArchiveObjectAsync(modelsContainer, options.ActiveMetricsObjectKey, retrainContainer, BuildObjectKey(archivePrefix, "metrics/metrics.json"), ct);

			await CopyObjectAsync(retrainContainer, candidateYoloKey, modelsContainer, options.ActiveYoloObjectKey, ct);
			await CopyObjectAsync(retrainContainer, candidateUnetKey, modelsContainer, options.ActiveUnetObjectKey, ct);
			await CopyObjectAsync(retrainContainer, candidateMetricsKey, modelsContainer, options.ActiveMetricsObjectKey, ct);

			var promotionManifestKey = BuildObjectKey(options.ManifestPrefix, $"promotion_{message.BatchId:N}.json");
			var promotionManifest = new JsonObject
			{
				["batch_id"] = message.BatchId.ToString(),
				["promoted_at_utc"] = DateTime.UtcNow,
				["models_container"] = options.ObjectStorageModelsContainer,
				["retrain_container"] = options.ObjectStorageRetrainContainer,
				["active_yolo_key"] = options.ActiveYoloObjectKey,
				["active_unet_key"] = options.ActiveUnetObjectKey,
				["active_metrics_key"] = options.ActiveMetricsObjectKey,
				["candidate_yolo_key"] = candidateYoloKey,
				["candidate_unet_key"] = candidateUnetKey,
				["candidate_metrics_key"] = candidateMetricsKey,
				["archive_prefix"] = archivePrefix,
				["candidate_yolo_map"] = candidateYoloMap.Value,
				["candidate_unet_miou"] = candidateUnetMiou.Value,
				["baseline_yolo_map"] = baselineYoloMap,
				["baseline_unet_miou"] = baselineUnetMiou,
				["reason"] = gateResult.Reason,
			};

			await UploadTextAsync(
				retrainContainer,
				promotionManifestKey,
				promotionManifest.ToJsonString(new JsonSerializerOptions { WriteIndented = true }),
				"application/json",
				ct);

			return gateResult;
		}

		private async Task<CommandExecutionResult> RunRemoteTrainerAsync(
			ScoringRetrainRequestedEvent message,
			ScoringRetrainOptions options,
			CancellationToken ct)
		{
			if (!options.ObjectStorageEnabled)
			{
				return new CommandExecutionResult(
					1,
					string.Empty,
					"Remote trainer requires ScoringRetrain:ObjectStorageEnabled=true.");
			}

			if (string.IsNullOrWhiteSpace(options.RemoteTrainerBaseUrl))
			{
				return new CommandExecutionResult(
					1,
					string.Empty,
					"Remote trainer base URL is empty.");
			}

			if (string.IsNullOrWhiteSpace(options.ExternalCandidatePrefix))
			{
				return new CommandExecutionResult(
					1,
					string.Empty,
					"ExternalCandidatePrefix is empty while RemoteTrainerEnabled=true.");
			}

			var createPath = NormalizePath(options.RemoteTrainerCreatePath, "/retrain/jobs");
			var createUri = new Uri(new Uri(options.RemoteTrainerBaseUrl), createPath);

			var client = _httpClientFactory.CreateClient();
			if (!string.IsNullOrWhiteSpace(options.RemoteTrainerApiKey))
			{
				client.DefaultRequestHeaders.Remove("X-Retrain-Api-Key");
				client.DefaultRequestHeaders.Add("X-Retrain-Api-Key", options.RemoteTrainerApiKey);
			}

			var minApprovedAnnotations = Math.Max(1, message.MinApprovedAnnotations);
			var maxSamplesPerBatch = Math.Clamp(message.MaxSamplesPerBatch <= 0 ? message.ApprovedAnnotationCount : message.MaxSamplesPerBatch, 1, 5000);
			var createRequest = new RemoteRetrainJobCreateRequest(
				message.BatchId,
				message.SourceWindowFromUtc,
				message.ReviewedSampleCount,
				message.ApprovedAnnotationCount,
				minApprovedAnnotations,
				maxSamplesPerBatch,
				message.Samples.Select(s => new RemoteRetrainSampleItem(
					s.ResultId,
					s.JobId,
					s.RequestId,
					s.EnvironmentKey,
					s.SourceType,
					s.Source,
					s.ReviewedVerdict,
					s.ReviewedAtUtc,
					s.ReviewedByEmail)).ToList());

			RemoteRetrainJobCreateResponse? createResponse;
			try
			{
				using var httpResponse = await client.PostAsJsonAsync(createUri, createRequest, ct);
				if (!httpResponse.IsSuccessStatusCode)
				{
					var body = await httpResponse.Content.ReadAsStringAsync(ct);
					return new CommandExecutionResult(
						1,
						string.Empty,
						$"Remote trainer submit failed ({(int)httpResponse.StatusCode}). {Truncate(body, 1000)}");
				}

				createResponse = await httpResponse.Content.ReadFromJsonAsync<RemoteRetrainJobCreateResponse>(cancellationToken: ct);
			}
			catch (Exception ex)
			{
				return new CommandExecutionResult(1, string.Empty, $"Remote trainer submit exception: {ex.Message}");
			}

			if (createResponse is null || string.IsNullOrWhiteSpace(createResponse.JobId))
			{
				return new CommandExecutionResult(1, string.Empty, "Remote trainer submit response missing job id.");
			}

			var statusTemplate = options.RemoteTrainerStatusPathTemplate;
			if (string.IsNullOrWhiteSpace(statusTemplate))
			{
				statusTemplate = "/retrain/jobs/{jobId}";
			}

			string statusPath;
			if (statusTemplate.Contains("{jobId}", StringComparison.OrdinalIgnoreCase))
			{
				statusPath = statusTemplate.Replace("{jobId}", Uri.EscapeDataString(createResponse.JobId), StringComparison.OrdinalIgnoreCase);
			}
			else
			{
				statusPath = NormalizePath(statusTemplate, "/retrain/jobs").TrimEnd('/') + "/" + Uri.EscapeDataString(createResponse.JobId);
			}

			var statusUri = new Uri(new Uri(options.RemoteTrainerBaseUrl), statusPath);
			var pollInterval = Math.Max(1, options.RemoteTrainerPollIntervalSeconds);
			var timeoutSeconds = Math.Max(30, options.RemoteTrainerTimeoutSeconds);

			using var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(timeoutSeconds));
			using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(ct, timeoutCts.Token);

			while (true)
			{
				RemoteRetrainJobStatusResponse? statusResponse;
				try
				{
					using var httpResponse = await client.GetAsync(statusUri, linkedCts.Token);
					if (!httpResponse.IsSuccessStatusCode)
					{
						var body = await httpResponse.Content.ReadAsStringAsync(linkedCts.Token);
						return new CommandExecutionResult(
							1,
							string.Empty,
							$"Remote trainer status failed ({(int)httpResponse.StatusCode}). {Truncate(body, 1000)}");
					}

					statusResponse = await httpResponse.Content.ReadFromJsonAsync<RemoteRetrainJobStatusResponse>(cancellationToken: linkedCts.Token);
				}
				catch (OperationCanceledException) when (timeoutCts.IsCancellationRequested)
				{
					return new CommandExecutionResult(
						-1,
						string.Empty,
						$"Remote trainer polling timed out after {timeoutSeconds} seconds.");
				}
				catch (Exception ex)
				{
					return new CommandExecutionResult(1, string.Empty, $"Remote trainer polling exception: {ex.Message}");
				}

				var status = statusResponse?.Status?.Trim().ToLowerInvariant();
				if (status is "completed" or "succeeded" or "success")
				{
					return new CommandExecutionResult(
						0,
						$"Remote trainer job {createResponse.JobId} completed.",
						statusResponse?.Message ?? string.Empty);
				}

				if (status is "failed" or "error" or "cancelled" or "canceled")
				{
					return new CommandExecutionResult(
						1,
						string.Empty,
						$"Remote trainer job {createResponse.JobId} failed. {statusResponse?.Message}");
				}

				try
				{
					await Task.Delay(TimeSpan.FromSeconds(pollInterval), linkedCts.Token);
				}
				catch (OperationCanceledException) when (timeoutCts.IsCancellationRequested)
				{
					return new CommandExecutionResult(
						-1,
						string.Empty,
						$"Remote trainer polling timed out after {timeoutSeconds} seconds.");
				}
			}
		}

		private static async Task<CommandExecutionResult> RunCommandAsync(
			string command,
			string? workingDirectory,
			int timeoutSeconds,
			CancellationToken ct)
		{
			var isWindows = OperatingSystem.IsWindows();
			var shell = isWindows ? "cmd.exe" : "/bin/sh";
			var shellArgs = isWindows ? $"/C {command}" : $"-c \"{command.Replace("\"", "\\\"")}\"";

			var startInfo = new ProcessStartInfo
			{
				FileName = shell,
				Arguments = shellArgs,
				RedirectStandardOutput = true,
				RedirectStandardError = true,
				UseShellExecute = false,
				CreateNoWindow = true,
			};

			if (!string.IsNullOrWhiteSpace(workingDirectory) && Directory.Exists(workingDirectory))
			{
				startInfo.WorkingDirectory = workingDirectory;
			}

			using var process = new Process { StartInfo = startInfo };
			var stdout = new StringBuilder();
			var stderr = new StringBuilder();

			process.OutputDataReceived += (_, args) =>
			{
				if (args.Data is not null)
				{
					stdout.AppendLine(args.Data);
				}
			};
			process.ErrorDataReceived += (_, args) =>
			{
				if (args.Data is not null)
				{
					stderr.AppendLine(args.Data);
				}
			};

			process.Start();
			process.BeginOutputReadLine();
			process.BeginErrorReadLine();

			using var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(timeoutSeconds));
			using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(ct, timeoutCts.Token);

			try
			{
				await process.WaitForExitAsync(linkedCts.Token);
			}
			catch (OperationCanceledException)
			{
				try
				{
					if (!process.HasExited)
					{
						process.Kill(entireProcessTree: true);
					}
				}
				catch
				{
				}

				return new CommandExecutionResult(-1, stdout.ToString(), stderr.ToString() + "Command timed out.");
			}

			return new CommandExecutionResult(process.ExitCode, stdout.ToString(), stderr.ToString());
		}

		private static async Task<PromotionGateResult> EvaluatePromotionGateAsync(
			ScoringRetrainOptions options,
			CancellationToken ct)
		{
			var candidatePath = ResolvePath(options.CandidateMetricsFile, options.TrainerWorkingDirectory);
			var candidateNode = await ReadJsonFileAsNodeAsync(candidatePath, ct);
			if (candidateNode is null)
			{
				return new PromotionGateResult(
					false,
					0,
					null,
					null,
					null,
					$"Candidate metrics not found at {candidatePath}.",
					$"{options.YoloMapMetricKey}+{options.UnetMiouMetricKey}",
					options.MinimumYoloMapImprovement + options.MinimumUnetMiouImprovement);
			}

			var candidateYoloMap = ReadMetricFromNode(candidateNode, options.YoloMapMetricKey);
			var candidateUnetMiou = ReadMetricFromNode(candidateNode, options.UnetMiouMetricKey);
			if (candidateYoloMap is null || candidateUnetMiou is null)
			{
				return new PromotionGateResult(
					false,
					0,
					null,
					candidateYoloMap,
					candidateUnetMiou,
					$"Candidate metrics missing keys '{options.YoloMapMetricKey}' or '{options.UnetMiouMetricKey}'.",
					$"{options.YoloMapMetricKey}+{options.UnetMiouMetricKey}",
					options.MinimumYoloMapImprovement + options.MinimumUnetMiouImprovement);
			}

			return EvaluateDualGate(candidateYoloMap.Value, candidateUnetMiou.Value, null, null, options);
		}

		private static PromotionGateResult EvaluateDualGate(
			double candidateYoloMap,
			double candidateUnetMiou,
			double? baselineYoloMap,
			double? baselineUnetMiou,
			ScoringRetrainOptions options)
		{
			var metricKey = $"{options.YoloMapMetricKey}+{options.UnetMiouMetricKey}";
			var minimumImprovement = options.MinimumYoloMapImprovement + options.MinimumUnetMiouImprovement;

			if (baselineYoloMap is null || baselineUnetMiou is null)
			{
				if (options.PromoteWhenNoBaseline)
				{
					return new PromotionGateResult(
						true,
						(candidateYoloMap + candidateUnetMiou) / 2.0,
						null,
						candidateYoloMap,
						candidateUnetMiou,
						"No complete baseline metrics found. Promote by policy.",
						metricKey,
						minimumImprovement);
				}

				return new PromotionGateResult(
					false,
					(candidateYoloMap + candidateUnetMiou) / 2.0,
					null,
					candidateYoloMap,
					candidateUnetMiou,
					"No complete baseline metrics found. Promotion skipped by policy.",
					metricKey,
					minimumImprovement);
			}

			var requiredYolo = baselineYoloMap.Value + options.MinimumYoloMapImprovement;
			var requiredUnet = baselineUnetMiou.Value + options.MinimumUnetMiouImprovement;
			var yoloPass = candidateYoloMap >= requiredYolo;
			var unetPass = candidateUnetMiou >= requiredUnet;

			var promoted = yoloPass && unetPass;
			var reason = promoted
				? $"Promoted: yolo_map {candidateYoloMap:F4} >= {requiredYolo:F4} and unet_miou {candidateUnetMiou:F4} >= {requiredUnet:F4}."
				: $"Rejected: yolo_map {candidateYoloMap:F4}/{requiredYolo:F4}, unet_miou {candidateUnetMiou:F4}/{requiredUnet:F4}.";

			return new PromotionGateResult(
				promoted,
				(candidateYoloMap + candidateUnetMiou) / 2.0,
				(baselineYoloMap.Value + baselineUnetMiou.Value) / 2.0,
				candidateYoloMap,
				candidateUnetMiou,
				reason,
				metricKey,
				minimumImprovement);
		}

		private static string ResolvePath(string path, string? baseDirectory)
		{
			if (Path.IsPathRooted(path))
			{
				return path;
			}

			var cwd = !string.IsNullOrWhiteSpace(baseDirectory) ? baseDirectory : Directory.GetCurrentDirectory();
			return Path.GetFullPath(Path.Combine(cwd, path));
		}

		private static async Task<JsonNode?> ReadJsonFileAsNodeAsync(string filePath, CancellationToken ct)
		{
			if (!File.Exists(filePath))
			{
				return null;
			}

			await using var stream = File.OpenRead(filePath);
			return await JsonNode.ParseAsync(stream, cancellationToken: ct);
		}

		private static double? ReadMetricFromNode(JsonNode? node, string metricKey)
		{
			if (node is null || string.IsNullOrWhiteSpace(metricKey))
			{
				return null;
			}

			JsonNode? current = node;
			foreach (var segment in metricKey.Split('.', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
			{
				current = current?[segment];
				if (current is null)
				{
					return null;
				}
			}

			if (current is JsonValue value && value.TryGetValue<double>(out var number))
			{
				return number;
			}

			return null;
		}

		private static BlobServiceClient CreateBlobServiceClient(ScoringRetrainOptions options)
		{
			if (string.IsNullOrWhiteSpace(options.ObjectStorageConnectionString))
			{
				throw new InvalidOperationException("ScoringRetrain ObjectStorageConnectionString is empty.");
			}

			return new BlobServiceClient(options.ObjectStorageConnectionString);
		}

		private static async Task EnsureContainerExistsAsync(BlobContainerClient containerClient, CancellationToken ct)
		{
			await containerClient.CreateIfNotExistsAsync(cancellationToken: ct);
		}

		private static async Task UploadFileAsync(
			BlobContainerClient containerClient,
			string objectKey,
			string filePath,
			string contentType,
			CancellationToken ct)
		{
			var blobClient = containerClient.GetBlobClient(objectKey);
			await blobClient.DeleteIfExistsAsync(cancellationToken: ct);

			await using var stream = File.OpenRead(filePath);
			await blobClient.UploadAsync(
				stream,
				new BlobUploadOptions
				{
					HttpHeaders = new BlobHttpHeaders { ContentType = contentType }
				},
				ct);
		}

		private static async Task UploadTextAsync(
			BlobContainerClient containerClient,
			string objectKey,
			string content,
			string contentType,
			CancellationToken ct)
		{
			var blobClient = containerClient.GetBlobClient(objectKey);
			await blobClient.DeleteIfExistsAsync(cancellationToken: ct);

			await blobClient.UploadAsync(
				BinaryData.FromString(content),
				new BlobUploadOptions
				{
					HttpHeaders = new BlobHttpHeaders { ContentType = contentType }
				},
				ct);
		}

		private static async Task CopyObjectAsync(
			BlobContainerClient sourceContainer,
			string sourceKey,
			BlobContainerClient destinationContainer,
			string destinationKey,
			CancellationToken ct)
		{
			var sourceBlob = sourceContainer.GetBlobClient(sourceKey);
			var destinationBlob = destinationContainer.GetBlobClient(destinationKey);

			await destinationBlob.DeleteIfExistsAsync(cancellationToken: ct);
			await destinationBlob.StartCopyFromUriAsync(sourceBlob.Uri, cancellationToken: ct);
			await WaitForCopyCompletionAsync(destinationBlob, ct);
		}

		private static async Task WaitForCopyCompletionAsync(BlobClient destinationBlob, CancellationToken ct)
		{
			for (var attempt = 0; attempt < 120; attempt++)
			{
				var properties = await destinationBlob.GetPropertiesAsync(cancellationToken: ct);
				if (properties.Value.CopyStatus == CopyStatus.Pending)
				{
					await Task.Delay(500, ct);
					continue;
				}

				if (properties.Value.CopyStatus == CopyStatus.Success)
				{
					return;
				}

				throw new InvalidOperationException($"Blob copy failed with status {properties.Value.CopyStatus}.");
			}

			throw new TimeoutException("Timed out waiting for blob copy completion.");
		}

		private static async Task TryArchiveObjectAsync(
			BlobContainerClient sourceContainer,
			string sourceKey,
			BlobContainerClient destinationContainer,
			string destinationKey,
			CancellationToken ct)
		{
			try
			{
				var exists = await ObjectExistsAsync(sourceContainer, sourceKey, ct);
				if (!exists)
				{
					return;
				}

				await CopyObjectAsync(sourceContainer, sourceKey, destinationContainer, destinationKey, ct);
			}
			catch
			{
			}
		}

		private static async Task<bool> ObjectExistsAsync(
			BlobContainerClient containerClient,
			string objectKey,
			CancellationToken ct)
		{
			try
			{
				await containerClient.GetBlobClient(objectKey).GetPropertiesAsync(cancellationToken: ct);
				return true;
			}
			catch (RequestFailedException ex) when (ex.Status == 404)
			{
				return false;
			}
		}

		private static async Task<JsonNode?> DownloadJsonNodeAsync(
			BlobContainerClient containerClient,
			string objectKey,
			CancellationToken ct)
		{
			if (!await ObjectExistsAsync(containerClient, objectKey, ct))
			{
				return null;
			}

			var response = await containerClient.GetBlobClient(objectKey).DownloadContentAsync(ct);
			var json = response.Value.Content.ToString();
			if (string.IsNullOrWhiteSpace(json))
			{
				return null;
			}

			return JsonNode.Parse(json);
		}

		private static string BuildObjectKey(string prefix, string suffix)
		{
			var p = (prefix ?? string.Empty).Trim().Trim('/');
			var s = (suffix ?? string.Empty).Trim().Trim('/');
			if (string.IsNullOrWhiteSpace(p))
			{
				return s;
			}

			return string.IsNullOrWhiteSpace(s) ? p : $"{p}/{s}";
		}

		private static string NormalizePath(string? path, string fallback)
		{
			var raw = string.IsNullOrWhiteSpace(path) ? fallback : path;
			return raw.StartsWith('/') ? raw : "/" + raw;
		}

		private static string Truncate(string value, int maxLength)
		{
			if (string.IsNullOrEmpty(value) || value.Length <= maxLength)
			{
				return value;
			}

			return value[..maxLength];
		}

		private sealed record CommandExecutionResult(int ExitCode, string StandardOutput, string StandardError);

		private sealed record PromotionGateResult(
			bool Promoted,
			double CandidateCompositeMetric,
			double? BaselineCompositeMetric,
			double? CandidateYoloMap,
			double? CandidateUnetMiou,
			string Reason,
			string MetricKey,
			double MinimumImprovement);

		private sealed record RemoteRetrainSampleItem(
			Guid ResultId,
			Guid JobId,
			string RequestId,
			string EnvironmentKey,
			string SourceType,
			string Source,
			string ReviewedVerdict,
			DateTime ReviewedAtUtc,
			string? ReviewedByEmail);

		private sealed record RemoteRetrainJobCreateRequest(
			Guid BatchId,
			DateTime SourceWindowFromUtc,
			int ReviewedSampleCount,
			int ApprovedAnnotationCount,
			int MinApprovedAnnotations,
			int MaxSamplesPerBatch,
			List<RemoteRetrainSampleItem> Samples);

		private sealed record RemoteRetrainJobCreateResponse(
			string JobId,
			string? Status);

		private sealed record RemoteRetrainJobStatusResponse(
			string JobId,
			string Status,
			string? Message);
	}
}
