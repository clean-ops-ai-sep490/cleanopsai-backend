using AutoMapper;
using CleanOpsAi.BuildingBlocks.Application.Exceptions;
using CleanOpsAi.BuildingBlocks.Application.Interfaces;
using CleanOpsAi.BuildingBlocks.Application.Interfaces.Messaging;
using CleanOpsAi.Modules.Scoring.Application.Common.Interfaces.Services;
using CleanOpsAi.Modules.Scoring.Application.DTOs.Response;
using CleanOpsAi.Modules.TaskOperations.Application.Common.Interfaces.Repositories;
using CleanOpsAi.Modules.TaskOperations.Application.Common.Interfaces.Services;
using CleanOpsAi.Modules.TaskOperations.Application.DTOs;
using CleanOpsAi.Modules.TaskOperations.Application.DTOs.Request;
using CleanOpsAi.Modules.TaskOperations.Application.DTOs.Response;
using CleanOpsAi.Modules.TaskOperations.Domain.Entities;
using CleanOpsAi.Modules.TaskOperations.Domain.Enums;
using CleanOpsAi.Modules.TaskOperations.IntegrationEvents;
using System.Text.Json;

namespace CleanOpsAi.Modules.TaskOperations.Application.Services
{
	public class TaskStepExecutionService : ITaskStepExecutionService
	{
		private const string AiPpeCheckBehavior = "ai-ppe-check";
		private const double DefaultPpeMinConfidence = 0.25d;
		private const int DefaultPpeMinPhotos = 1;

		private static readonly JsonSerializerOptions JsonOptions = new()
		{
			PropertyNamingPolicy = JsonNamingPolicy.CamelCase
		};

		private readonly ITaskStepExecutionRepository _repository;
		private readonly IDateTimeProvider _dateTimeProvider;
		private readonly IMapper _mapper;
		private readonly IScoringInferenceClient _inferenceClient;
		private readonly IEventBus _eventBus;

		public TaskStepExecutionService(
			ITaskStepExecutionRepository taskStepExecutionRepository,
			IDateTimeProvider dateTimeProvider,
			IMapper mapper,
			IScoringInferenceClient inferenceClient,
			IEventBus eventBus)
		{
			_repository = taskStepExecutionRepository;
			_dateTimeProvider = dateTimeProvider;
			_mapper = mapper;
			_inferenceClient = inferenceClient;
			_eventBus = eventBus;
		}

		public async Task<TaskStepExecutionDto> CompleteStepAsync(Guid id, SubmitStepExecutionDto dto, CancellationToken ct = default)
		{
			var step = await _repository.GetByIdDetail(id, ct)
				?? throw new NotFoundException(nameof(TaskStepExecution), id);

			var assignment = step.TaskAssignment;

			if (assignment.AssigneeId != dto.WorkerId)
				throw new BadRequestException("Not your task");

			if (assignment.IsAdhocTask)
				throw new BadRequestException("Adhoc task does not support step execution");

			if (step.Status != TaskStepExecutionStatus.InProgress)
				throw new BadRequestException("Step is not in progress");

			if (step.CompletedAt != default)
				throw new BadRequestException("Step already completed");

			if (IsAiPpeCheckStep(step.ConfigSnapshot))
			{
				EnsureStoredPpeCheckCanComplete(step.ResultData);
			}
			else
			{
				step.ResultData = dto.ResultData.GetRawText();
			}

			step.Status = TaskStepExecutionStatus.Completed;
			step.CompletedAt = _dateTimeProvider.UtcNow;

			var nextStep = await _repository.GetNextStepAsync(
					step.TaskAssignmentId,
					step.StepOrder,
					ct);

			if (nextStep != null)
			{
				nextStep.Status = TaskStepExecutionStatus.InProgress;
				nextStep.StartedAt = _dateTimeProvider.UtcNow;
			}

			await _repository.SaveChangesAsync(ct);

			var stepDto = _mapper.Map<TaskStepExecutionDto>(step);
			stepDto.NextStepId = nextStep?.Id;
			stepDto.ConfigSnapshot = JsonSerializer.Deserialize<JsonElement>("{}");

			return stepDto;
		}

		public async Task<TaskStepExecutionPpeCheckResponse> EvaluatePpeAsync(Guid id, CancellationToken ct = default)
		{
			var (step, requiredItems, imageUrls) = await LoadAndValidatePpeContextAsync(id, ct);

			TaskStepExecutionPpeCheckResponse response;
			try
			{
				var evaluation = await _inferenceClient.EvaluatePpeAsync(
					imageUrls,
					requiredItems.Select(x => x.ActionKey).ToList(),
					DefaultPpeMinConfidence,
					ct);

				response = BuildPpeCheckResponse(step, requiredItems, imageUrls, evaluation);
			}
			catch (Exception ex)
			{
				response = BuildPpeCheckErrorResponse(step, requiredItems, imageUrls, ex.Message);
			}

			step.ResultData = JsonSerializer.Serialize(response, JsonOptions);
			await _repository.SaveChangesAsync(ct);

			return response;
		}

		public async Task<TaskStepExecutionPpeCheckJobResponse> EnqueuePpeCheckAsync(Guid id, CancellationToken ct = default)
		{
			var (step, requiredItems, imageUrls) = await LoadAndValidatePpeContextAsync(id, ct);
			var queued = BuildPpeCheckPendingResponse(step, requiredItems, imageUrls);

			step.ResultData = JsonSerializer.Serialize(queued, JsonOptions);
			await _repository.SaveChangesAsync(ct);

			try
			{
				await _eventBus.PublishAsync(new PpeCheckRequestedEvent
				{
					TaskStepExecutionId = step.Id,
					RequestedAtUtc = _dateTimeProvider.UtcNow,
				}, ct);
			}
			catch (Exception ex)
			{
				var failed = BuildPpeCheckErrorResponse(step, requiredItems, imageUrls, ex.Message);
				step.ResultData = JsonSerializer.Serialize(failed, JsonOptions);
				await _repository.SaveChangesAsync(ct);
				return BuildPpeCheckJobResponse(step.Id, failed);
			}

			return BuildPpeCheckJobResponse(step.Id, queued);
		}

		public async Task<TaskStepExecutionPpeCheckJobResponse> GetPpeCheckJobStatusAsync(Guid id, CancellationToken ct = default)
		{
			var step = await _repository.GetByIdAsync(id, ct)
				?? throw new NotFoundException(nameof(TaskStepExecution), id);

			var response = TryParsePpeCheckResponse(step.ResultData);
			if (response is null)
			{
				return new TaskStepExecutionPpeCheckJobResponse
				{
					JobId = step.Id,
					TaskStepExecutionId = step.Id,
					Status = "NOT_STARTED",
					Message = "PPE check job has not been submitted.",
					UpdatedAt = _dateTimeProvider.UtcNow,
					Result = null,
				};
			}

			return BuildPpeCheckJobResponse(step.Id, response);
		}

		public async Task ProcessQueuedPpeCheckAsync(Guid id, CancellationToken ct = default)
		{
			try
			{
				await EvaluatePpeAsync(id, ct);
			}
			catch (Exception ex)
			{
				await TryStoreQueuedPpeErrorAsync(id, ex.Message, ct);
			}
		}

		public async Task<TaskStepExecutionDetailDto> GetStepDetailAsync(Guid id, CancellationToken ct = default)
		{
			var step = await _repository.GetByIdAsync(id, ct)
				?? throw new NotFoundException(nameof(TaskStepExecution), id);

			return _mapper.Map<TaskStepExecutionDetailDto>(step);
		}

		private void EnsureStoredPpeCheckCanComplete(string resultData)
		{
			if (string.IsNullOrWhiteSpace(resultData))
			{
				throw new BadRequestException("AI PPE check step must be checked before completion.");
			}

			try
			{
				using var document = JsonDocument.Parse(resultData);
				var root = document.RootElement;
				var type = root.TryGetProperty("type", out var typeElement)
					? typeElement.GetString()
					: null;
				var status = root.TryGetProperty("status", out var statusElement)
					? statusElement.GetString()
					: null;

				if (!string.Equals(type, AiPpeCheckBehavior, StringComparison.OrdinalIgnoreCase))
				{
					throw new BadRequestException("AI PPE check step must be checked before completion.");
				}

				if (!string.Equals(status, "PASS", StringComparison.OrdinalIgnoreCase) &&
					!string.Equals(status, "FAIL", StringComparison.OrdinalIgnoreCase))
				{
					throw new BadRequestException("AI PPE check step must have a valid PASS or FAIL result before completion.");
				}
			}
			catch (JsonException)
			{
				throw new BadRequestException("AI PPE check step must be checked before completion.");
			}
		}

		private static bool IsAiPpeCheckStep(string configSnapshot)
		{
			if (string.IsNullOrWhiteSpace(configSnapshot))
			{
				return false;
			}

			try
			{
				using var document = JsonDocument.Parse(configSnapshot);
				if (!document.RootElement.TryGetProperty("schema", out var schema))
				{
					return false;
				}

				if (!schema.TryGetProperty("x-behavior", out var behavior))
				{
					return false;
				}

				return string.Equals(behavior.GetString(), AiPpeCheckBehavior, StringComparison.OrdinalIgnoreCase);
			}
			catch (JsonException)
			{
				return false;
			}
		}

		private static List<TaskStepExecutionPpeRequiredItemResponse> ParseRequiredPpe(string configSnapshot)
		{
			try
			{
				using var document = JsonDocument.Parse(configSnapshot);
				if (!document.RootElement.TryGetProperty("detail", out var detail))
				{
					throw new BadRequestException("AI PPE check step is missing configuration detail.");
				}

				if (!detail.TryGetProperty("requiredPPE", out var requiredPpe) || requiredPpe.ValueKind != JsonValueKind.Array)
				{
					throw new BadRequestException("AI PPE check step is missing requiredPPE configuration.");
				}

				var results = new List<TaskStepExecutionPpeRequiredItemResponse>();
				var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
				foreach (var item in requiredPpe.EnumerateArray())
				{
					if (!item.TryGetProperty("actionKey", out var actionKeyElement))
					{
						throw new BadRequestException("Each required PPE item must define an actionKey.");
					}

					var actionKey = NormalizeLabel(actionKeyElement.GetString());
					if (string.IsNullOrWhiteSpace(actionKey))
					{
						throw new BadRequestException("Each required PPE item must define a non-empty actionKey.");
					}

					if (!seen.Add(actionKey))
					{
						continue;
					}

					var name = item.TryGetProperty("name", out var nameElement)
						? nameElement.GetString()
						: null;

					results.Add(new TaskStepExecutionPpeRequiredItemResponse
					{
						ActionKey = actionKey,
						Name = string.IsNullOrWhiteSpace(name) ? actionKey : name.Trim(),
					});
				}

				return results;
			}
			catch (JsonException)
			{
				throw new BadRequestException("AI PPE check step has invalid JSON configuration.");
			}
		}

		private async Task<(TaskStepExecution step, List<TaskStepExecutionPpeRequiredItemResponse> requiredItems, List<string> imageUrls)> LoadAndValidatePpeContextAsync(Guid id, CancellationToken ct)
		{
			var step = await _repository.GetByIdDetail(id, ct)
				?? throw new NotFoundException(nameof(TaskStepExecution), id);

			if (step.Status != TaskStepExecutionStatus.InProgress)
			{
				throw new BadRequestException("Step must be in progress to run PPE check.");
			}

			if (!IsAiPpeCheckStep(step.ConfigSnapshot))
			{
				throw new BadRequestException("This step is not configured for AI PPE check.");
			}

			var requiredItems = ParseRequiredPpe(step.ConfigSnapshot);
			if (requiredItems.Count == 0)
			{
				throw new BadRequestException("This AI PPE check step does not define any required PPE items.");
			}

			var imageUrls = step.TaskStepExecutionImages
				.Where(x => !x.IsDeleted && x.ImageType == ImageType.Ppe && !string.IsNullOrWhiteSpace(x.ImageUrl))
				.OrderBy(x => x.Created)
				.Select(x => x.ImageUrl)
				.Distinct(StringComparer.OrdinalIgnoreCase)
				.ToList();

			if (imageUrls.Count < DefaultPpeMinPhotos)
			{
				throw new BadRequestException("At least one PPE image is required before running AI PPE check.");
			}

			return (step, requiredItems, imageUrls);
		}

		private TaskStepExecutionPpeCheckResponse BuildPpeCheckResponse(
			TaskStepExecution step,
			List<TaskStepExecutionPpeRequiredItemResponse> requiredItems,
			List<string> imageUrls,
			PpeEvaluationResponse evaluation)
		{
			var normalizedStatus = NormalizeStatus(evaluation.Status);
			var requiredByActionKey = requiredItems.ToDictionary(x => x.ActionKey, StringComparer.OrdinalIgnoreCase);
			var missingKeys = (evaluation.MissingItems ?? new List<string>())
				.Select(NormalizeLabel)
				.Where(x => !string.IsNullOrWhiteSpace(x))
				.Distinct(StringComparer.OrdinalIgnoreCase)
				.ToList();

			return new TaskStepExecutionPpeCheckResponse
			{
				TaskStepExecutionId = step.Id,
				SopStepId = step.SopStepId,
				StepOrder = step.StepOrder,
				CheckedAt = _dateTimeProvider.UtcNow,
				Status = normalizedStatus,
				Message = string.IsNullOrWhiteSpace(evaluation.Message)
					? (normalizedStatus == "PASS" ? "Meets requirements." : "Missing required PPE items.")
					: evaluation.Message.Trim(),
				RequiredPpe = requiredItems,
				ImageUrls = imageUrls,
				DetectedItems = (evaluation.DetectedItems ?? new List<PpeDetectedItemResponse>())
					.Where(x => !string.IsNullOrWhiteSpace(x.Name))
					.Select(x =>
					{
						var actionKey = NormalizeLabel(x.Name);
						requiredByActionKey.TryGetValue(actionKey, out var requiredItem);
						return new TaskStepExecutionPpeDetectedItemResponse
						{
							ActionKey = actionKey,
							Name = requiredItem?.Name ?? actionKey,
							Confidence = x.Confidence,
							ImageIndex = x.ImageIndex,
						};
					})
					.ToList(),
				MissingItems = missingKeys.Select(actionKey =>
				{
					requiredByActionKey.TryGetValue(actionKey, out var requiredItem);
					return new TaskStepExecutionPpeRequiredItemResponse
					{
						ActionKey = actionKey,
						Name = requiredItem?.Name ?? actionKey,
					};
				}).ToList(),
				FailedImages = (evaluation.FailedImages ?? new List<PpeFailedImageResponse>())
					.Select(x => new TaskStepExecutionPpeFailedImageResponse
					{
						ImageUrl = x.ImageUrl ?? string.Empty,
						ImageIndex = x.ImageIndex,
						Error = x.Error ?? string.Empty,
					})
					.ToList(),
			};
		}

		private TaskStepExecutionPpeCheckResponse BuildPpeCheckErrorResponse(
			TaskStepExecution step,
			List<TaskStepExecutionPpeRequiredItemResponse> requiredItems,
			List<string> imageUrls,
			string errorMessage)
		{
			return new TaskStepExecutionPpeCheckResponse
			{
				TaskStepExecutionId = step.Id,
				SopStepId = step.SopStepId,
				StepOrder = step.StepOrder,
				CheckedAt = _dateTimeProvider.UtcNow,
				Status = "ERROR",
				Message = string.IsNullOrWhiteSpace(errorMessage)
					? "AI PPE check failed."
					: $"AI PPE check failed: {errorMessage}",
				RequiredPpe = requiredItems,
				ImageUrls = imageUrls,
				DetectedItems = new List<TaskStepExecutionPpeDetectedItemResponse>(),
				MissingItems = new List<TaskStepExecutionPpeRequiredItemResponse>(),
				FailedImages = imageUrls.Select((imageUrl, index) => new TaskStepExecutionPpeFailedImageResponse
				{
					ImageUrl = imageUrl,
					ImageIndex = index,
					Error = string.IsNullOrWhiteSpace(errorMessage) ? "AI PPE check failed." : errorMessage,
				}).ToList(),
			};
		}

		private TaskStepExecutionPpeCheckResponse BuildPpeCheckPendingResponse(
			TaskStepExecution step,
			List<TaskStepExecutionPpeRequiredItemResponse> requiredItems,
			List<string> imageUrls)
		{
			return new TaskStepExecutionPpeCheckResponse
			{
				TaskStepExecutionId = step.Id,
				SopStepId = step.SopStepId,
				StepOrder = step.StepOrder,
				CheckedAt = _dateTimeProvider.UtcNow,
				Status = "PENDING",
				Message = "PPE check job has been queued.",
				RequiredPpe = requiredItems,
				ImageUrls = imageUrls,
				DetectedItems = new List<TaskStepExecutionPpeDetectedItemResponse>(),
				MissingItems = new List<TaskStepExecutionPpeRequiredItemResponse>(),
				FailedImages = new List<TaskStepExecutionPpeFailedImageResponse>(),
			};
		}

		private TaskStepExecutionPpeCheckJobResponse BuildPpeCheckJobResponse(Guid jobId, TaskStepExecutionPpeCheckResponse response)
		{
			return new TaskStepExecutionPpeCheckJobResponse
			{
				JobId = jobId,
				TaskStepExecutionId = response.TaskStepExecutionId,
				Status = NormalizeJobStatus(response.Status),
				Message = response.Message,
				UpdatedAt = response.CheckedAt,
				Result = response,
			};
		}

		private async Task TryStoreQueuedPpeErrorAsync(Guid id, string errorMessage, CancellationToken ct)
		{
			var step = await _repository.GetByIdDetail(id, ct);
			if (step is null || step.Status != TaskStepExecutionStatus.InProgress || !IsAiPpeCheckStep(step.ConfigSnapshot))
			{
				return;
			}

			var requiredItems = TryParseRequiredPpe(step.ConfigSnapshot);
			var imageUrls = step.TaskStepExecutionImages
				.Where(x => !x.IsDeleted && x.ImageType == ImageType.Ppe && !string.IsNullOrWhiteSpace(x.ImageUrl))
				.OrderBy(x => x.Created)
				.Select(x => x.ImageUrl)
				.Distinct(StringComparer.OrdinalIgnoreCase)
				.ToList();

			var response = BuildPpeCheckErrorResponse(step, requiredItems, imageUrls, errorMessage);
			step.ResultData = JsonSerializer.Serialize(response, JsonOptions);
			await _repository.SaveChangesAsync(ct);
		}

		private static List<TaskStepExecutionPpeRequiredItemResponse> TryParseRequiredPpe(string configSnapshot)
		{
			try
			{
				return ParseRequiredPpe(configSnapshot);
			}
			catch
			{
				return new List<TaskStepExecutionPpeRequiredItemResponse>();
			}
		}

		private static TaskStepExecutionPpeCheckResponse? TryParsePpeCheckResponse(string? resultData)
		{
			if (string.IsNullOrWhiteSpace(resultData))
			{
				return null;
			}

			try
			{
				var response = JsonSerializer.Deserialize<TaskStepExecutionPpeCheckResponse>(resultData, JsonOptions);
				if (!string.Equals(response?.Type, AiPpeCheckBehavior, StringComparison.OrdinalIgnoreCase))
				{
					return null;
				}

				return response;
			}
			catch
			{
				return null;
			}
		}

		private static string NormalizeJobStatus(string? status)
		{
			var normalized = NormalizeStatus(status);
			return normalized switch
			{
				"PASS" => "SUCCEEDED",
				"FAIL" => "SUCCEEDED",
				"PENDING" => "PENDING",
				"ERROR" => "FAILED",
				_ => "PROCESSING",
			};
		}

		private static string NormalizeStatus(string? status)
		{
			if (string.IsNullOrWhiteSpace(status))
			{
				return "ERROR";
			}

			return status.Trim().ToUpperInvariant();
		}

		private static string NormalizeLabel(string? label)
		{
			return string.IsNullOrWhiteSpace(label)
				? string.Empty
				: label.Trim().ToLowerInvariant();
		}
	}
}
