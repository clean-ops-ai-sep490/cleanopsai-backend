using CleanOpsAi.BuildingBlocks.Application.Interfaces.Messaging;
using CleanOpsAi.Modules.Scoring.IntegrationEvents;
using CleanOpsAi.Modules.Scoring.Infrastructure.Options;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using MassTransit;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.Globalization;
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

		public ScoringRetrainRequestedConsumer(
			IEventBus eventBus,
			IOptions<ScoringRetrainOptions> options,
			ILogger<ScoringRetrainRequestedConsumer> logger)
		{
			_eventBus = eventBus;
			_options = options;
			_logger = logger;
		}

		public async Task Consume(ConsumeContext<ScoringRetrainRequestedEvent> context)
		{
			var config = _options.Value;
			var message = context.Message;
			var useExternalCandidate = config.ObjectStorageEnabled
				&& !string.IsNullOrWhiteSpace(config.ExternalCandidatePrefix);

			if (!useExternalCandidate && string.IsNullOrWhiteSpace(config.TrainerCommand))
			{
				_logger.LogWarning("Trainer command is empty. Skip retrain batch {BatchId}.", message.BatchId);
				return;
			}

			CommandExecutionResult execution;
			if (useExternalCandidate)
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
				gateResult = await EvaluateAndPromoteViaObjectStorageAsync(message, config, context.CancellationToken);
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
			CancellationToken ct)
		{
			using var s3 = CreateS3Client(options);
			await EnsureBucketExistsAsync(s3, options.ObjectStorageBucket, ct);

			var hasExternalCandidate = !string.IsNullOrWhiteSpace(options.ExternalCandidatePrefix);
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
				var hasYolo = await ObjectExistsAsync(s3, options.ObjectStorageBucket, candidateYoloKey, ct);
				var hasUnet = await ObjectExistsAsync(s3, options.ObjectStorageBucket, candidateUnetKey, ct);
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

				candidateMetrics = await DownloadJsonNodeAsync(s3, options.ObjectStorageBucket, candidateMetricsKey, ct);
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
				await UploadFileAsync(s3, options.ObjectStorageBucket, candidateYoloKey, candidateYoloPath, "application/octet-stream", ct);
				await UploadFileAsync(s3, options.ObjectStorageBucket, candidateUnetKey, candidateUnetPath, "application/octet-stream", ct);
				await UploadFileAsync(s3, options.ObjectStorageBucket, candidateMetricsKey, candidateMetricsPath, "application/json", ct);
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

			var baselineMetricsNode = await DownloadJsonNodeAsync(s3, options.ObjectStorageBucket, options.ActiveMetricsObjectKey, ct);
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
				["bucket"] = options.ObjectStorageBucket,
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
				s3,
				options.ObjectStorageBucket,
				candidateManifestKey,
				candidateManifest.ToJsonString(new JsonSerializerOptions { WriteIndented = true }),
				"application/json",
				ct);

			if (!gateResult.Promoted)
			{
				return gateResult;
			}

			var archivePrefix = BuildObjectKey(options.ArchivePrefix, DateTime.UtcNow.ToString("yyyyMMddHHmmss", CultureInfo.InvariantCulture));
			await TryArchiveObjectAsync(s3, options.ObjectStorageBucket, options.ActiveYoloObjectKey, BuildObjectKey(archivePrefix, "yolo/model.pt"), ct);
			await TryArchiveObjectAsync(s3, options.ObjectStorageBucket, options.ActiveUnetObjectKey, BuildObjectKey(archivePrefix, "unet/model.pth"), ct);
			await TryArchiveObjectAsync(s3, options.ObjectStorageBucket, options.ActiveMetricsObjectKey, BuildObjectKey(archivePrefix, "metrics/metrics.json"), ct);

			await CopyObjectAsync(s3, options.ObjectStorageBucket, candidateYoloKey, options.ActiveYoloObjectKey, ct);
			await CopyObjectAsync(s3, options.ObjectStorageBucket, candidateUnetKey, options.ActiveUnetObjectKey, ct);
			await CopyObjectAsync(s3, options.ObjectStorageBucket, candidateMetricsKey, options.ActiveMetricsObjectKey, ct);

			var promotionManifestKey = BuildObjectKey(options.ManifestPrefix, $"promotion_{message.BatchId:N}.json");
			var promotionManifest = new JsonObject
			{
				["batch_id"] = message.BatchId.ToString(),
				["promoted_at_utc"] = DateTime.UtcNow,
				["bucket"] = options.ObjectStorageBucket,
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
				s3,
				options.ObjectStorageBucket,
				promotionManifestKey,
				promotionManifest.ToJsonString(new JsonSerializerOptions { WriteIndented = true }),
				"application/json",
				ct);

			return gateResult;
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

		private static IAmazonS3 CreateS3Client(ScoringRetrainOptions options)
		{
			var config = new AmazonS3Config
			{
				ServiceURL = options.ObjectStorageServiceUrl,
				ForcePathStyle = options.ObjectStorageForcePathStyle,
				UseHttp = !options.ObjectStorageUseSsl,
			};

			var credentials = new BasicAWSCredentials(options.ObjectStorageAccessKey, options.ObjectStorageSecretKey);
			return new AmazonS3Client(credentials, config);
		}

		private static async Task EnsureBucketExistsAsync(IAmazonS3 s3, string bucketName, CancellationToken ct)
		{
			if (await Amazon.S3.Util.AmazonS3Util.DoesS3BucketExistV2Async(s3, bucketName))
			{
				return;
			}

			await s3.PutBucketAsync(new PutBucketRequest
			{
				BucketName = bucketName,
			}, ct);
		}

		private static async Task UploadFileAsync(
			IAmazonS3 s3,
			string bucketName,
			string objectKey,
			string filePath,
			string contentType,
			CancellationToken ct)
		{
			await s3.PutObjectAsync(new PutObjectRequest
			{
				BucketName = bucketName,
				Key = objectKey,
				FilePath = filePath,
				ContentType = contentType,
			}, ct);
		}

		private static async Task UploadTextAsync(
			IAmazonS3 s3,
			string bucketName,
			string objectKey,
			string content,
			string contentType,
			CancellationToken ct)
		{
			await s3.PutObjectAsync(new PutObjectRequest
			{
				BucketName = bucketName,
				Key = objectKey,
				ContentBody = content,
				ContentType = contentType,
			}, ct);
		}

		private static async Task CopyObjectAsync(
			IAmazonS3 s3,
			string bucketName,
			string sourceKey,
			string destinationKey,
			CancellationToken ct)
		{
			await s3.CopyObjectAsync(new CopyObjectRequest
			{
				SourceBucket = bucketName,
				SourceKey = sourceKey,
				DestinationBucket = bucketName,
				DestinationKey = destinationKey,
			}, ct);
		}

		private static async Task TryArchiveObjectAsync(
			IAmazonS3 s3,
			string bucketName,
			string sourceKey,
			string destinationKey,
			CancellationToken ct)
		{
			try
			{
				var exists = await ObjectExistsAsync(s3, bucketName, sourceKey, ct);
				if (!exists)
				{
					return;
				}

				await CopyObjectAsync(s3, bucketName, sourceKey, destinationKey, ct);
			}
			catch
			{
				// Archiving is best-effort and should not block promotion.
			}
		}

		private static async Task<bool> ObjectExistsAsync(IAmazonS3 s3, string bucketName, string objectKey, CancellationToken ct)
		{
			try
			{
				await s3.GetObjectMetadataAsync(bucketName, objectKey, ct);
				return true;
			}
			catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
			{
				return false;
			}
		}

		private static async Task<JsonNode?> DownloadJsonNodeAsync(
			IAmazonS3 s3,
			string bucketName,
			string objectKey,
			CancellationToken ct)
		{
			if (!await ObjectExistsAsync(s3, bucketName, objectKey, ct))
			{
				return null;
			}

			using var response = await s3.GetObjectAsync(bucketName, objectKey, ct);
			await using var stream = response.ResponseStream;
			return await JsonNode.ParseAsync(stream, cancellationToken: ct);
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
	}
}
