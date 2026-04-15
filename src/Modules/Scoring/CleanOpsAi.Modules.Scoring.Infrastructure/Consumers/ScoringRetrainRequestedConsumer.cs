using CleanOpsAi.BuildingBlocks.Application.Interfaces.Messaging;
using CleanOpsAi.Modules.Scoring.IntegrationEvents;
using CleanOpsAi.Modules.Scoring.Infrastructure.Options;
using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
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
	public class ScoringRetrainRequestedConsumer : IConsumer<ScoringRetrainRequestedEvent>
	{
		private readonly IEventBus _eventBus;
		private readonly IOptions<ScoringRetrainOptions> _options;
		private readonly ILogger<ScoringRetrainRequestedConsumer> _logger;
		private readonly IHttpClientFactory _httpClientFactory;

		public ScoringRetrainRequestedConsumer(
			IEventBus eventBus,
			IOptions<ScoringRetrainOptions> options,
			ILogger<ScoringRetrainRequestedConsumer> logger,
			IHttpClientFactory httpClientFactory)
		{
			_eventBus = eventBus;
			_options = options;
			_logger = logger;
			_httpClientFactory = httpClientFactory;
		}

		public async Task Consume(ConsumeContext<ScoringRetrainRequestedEvent> context)
		{
			var config = _options.Value;
			var message = context.Message;
			var useExternalCandidate = config.ObjectStorageEnabled
				&& config.UseExternalCandidateForRetrain
				&& !string.IsNullOrWhiteSpace(config.ExternalCandidatePrefix);

			if (!config.RemoteTrainerEnabled && !useExternalCandidate && string.IsNullOrWhiteSpace(config.TrainerCommand))
			{
				_logger.LogWarning("Trainer command is empty. Skip retrain batch {BatchId}.", message.BatchId);
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
					context.CancellationToken);

				if (bridgeExecution.ExitCode != 0)
				{
					await _eventBus.PublishAsync(new ScoringRetrainExecutionResultEvent
					{
						BatchId = message.BatchId,
						CompletedAtUtc = DateTime.UtcNow,
						Succeeded = false,
						ExitCode = bridgeExecution.ExitCode,
						Message = $"Reviewed bridge command failed with exit code {bridgeExecution.ExitCode}.",
					}, context.CancellationToken);

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
			if (config.RemoteTrainerEnabled)
			{
				execution = await RunRemoteTrainerAsync(message, config, context.CancellationToken);
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
					context.CancellationToken);
			}

			await _eventBus.PublishAsync(new ScoringRetrainExecutionResultEvent
			{
				BatchId = message.BatchId,
				CompletedAtUtc = DateTime.UtcNow,
				Succeeded = execution.ExitCode == 0,
				ExitCode = execution.ExitCode,
				Message = execution.ExitCode == 0
					? "Trainer command completed successfully."
					: $"Trainer command failed with exit code {execution.ExitCode}.",
			}, context.CancellationToken);

			if (execution.ExitCode != 0)
			{
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
				gateResult = await EvaluateAndPromoteViaObjectStorageAsync(message, config, useExternalCandidate, context.CancellationToken);
			}
			else
			{
				gateResult = await EvaluatePromotionGateAsync(config, context.CancellationToken);
			}

			if (gateResult.Promoted && !string.IsNullOrWhiteSpace(config.PromotionCommand))
			{
				var promotionExecution = await RunCommandAsync(
					config.PromotionCommand,
					config.TrainerWorkingDirectory,
					Math.Max(30, config.TrainerTimeoutSeconds),
					context.CancellationToken);

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
					context.CancellationToken);

				if (restartExecution.ExitCode != 0)
				{
					gateResult = gateResult with
					{
						Reason = $"{gateResult.Reason} Restart command failed with exit code {restartExecution.ExitCode}. Active model is already promoted in object storage; manual restart is required.",
					};
				}
			}

			await _eventBus.PublishAsync(new ScoringModelPromotionEvaluatedEvent
			{
				BatchId = message.BatchId,
				EvaluatedAtUtc = DateTime.UtcNow,
				MetricKey = $"{config.YoloMapMetricKey}+{config.UnetMiouMetricKey}",
				CandidateMetric = gateResult.CandidateCompositeMetric,
				BaselineMetric = gateResult.BaselineCompositeMetric,
				MinimumImprovement = 0,
				Promoted = gateResult.Promoted,
				Reason = gateResult.Reason,
			}, context.CancellationToken);

			_logger.LogInformation(
				"Scoring model promotion evaluated for batch {BatchId}. Promoted={Promoted}. Candidate={Candidate}. Baseline={Baseline}. Reason={Reason}",
				message.BatchId,
				gateResult.Promoted,
				gateResult.CandidateCompositeMetric,
				gateResult.BaselineCompositeMetric,
				gateResult.Reason);
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
						$"External candidate artifacts not found at prefix '{batchPrefix}'.");
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
						$"Candidate artifacts not found. metrics={candidateMetricsPath}, yolo={candidateYoloPath}, unet={candidateUnetPath}");
				}

				candidateMetrics = await ReadJsonFileAsNodeAsync(candidateMetricsPath, ct);
				await UploadFileAsync(retrainContainer, candidateYoloKey, candidateYoloPath, "application/octet-stream", ct);
				await UploadFileAsync(retrainContainer, candidateUnetKey, candidateUnetPath, "application/octet-stream", ct);
				await UploadFileAsync(retrainContainer, candidateMetricsKey, candidateMetricsPath, "application/json", ct);
			}

			if (candidateMetrics is null)
			{
				return new PromotionGateResult(false, 0, null, null, null, "Candidate metrics JSON cannot be parsed.");
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
					$"Candidate metrics missing keys '{options.YoloMapMetricKey}' or '{options.UnetMiouMetricKey}'.");
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

			var createRequest = new RemoteRetrainJobCreateRequest(
				message.BatchId,
				message.SourceWindowFromUtc,
				message.ReviewedSampleCount,
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

				await Task.Delay(TimeSpan.FromSeconds(pollInterval), linkedCts.Token);
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
					// Ignore kill errors and continue returning timeout result.
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
				return new PromotionGateResult(false, 0, null, null, null, $"Candidate metrics not found at {candidatePath}.");
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
					$"Candidate metrics missing keys '{options.YoloMapMetricKey}' or '{options.UnetMiouMetricKey}'.");
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
						"No complete baseline metrics found. Promote by policy.");
				}

				return new PromotionGateResult(
					false,
					(candidateYoloMap + candidateUnetMiou) / 2.0,
					null,
					candidateYoloMap,
					candidateUnetMiou,
					"No complete baseline metrics found. Promotion skipped by policy.");
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
				reason);
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

		private static async Task<double?> ReadMetricAsync(string filePath, string metricKey, CancellationToken ct)
		{
			if (!File.Exists(filePath))
			{
				return null;
			}

			await using var stream = File.OpenRead(filePath);
			using var doc = await JsonDocument.ParseAsync(stream, cancellationToken: ct);

			JsonElement current = doc.RootElement;
			foreach (var segment in metricKey.Split('.', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
			{
				if (current.ValueKind != JsonValueKind.Object || !current.TryGetProperty(segment, out current))
				{
					return null;
				}
			}

			if (current.ValueKind == JsonValueKind.Number && current.TryGetDouble(out var number))
			{
				return number;
			}

			return null;
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

			if (current is JsonValue value)
			{
				if (value.TryGetValue<double>(out var number))
				{
					return number;
				}
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
				// Archiving is best-effort and should not block promotion.
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
			string Reason);

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
