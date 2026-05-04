using AutoMapper;
using CleanOpsAi.BuildingBlocks.Application.Exceptions;
using CleanOpsAi.BuildingBlocks.Application.Interfaces;
using CleanOpsAi.BuildingBlocks.Application.Interfaces.Messaging;
using CleanOpsAi.BuildingBlocks.Infrastructure.Events; 
using CleanOpsAi.Modules.TaskOperations.Application.Common.Interfaces.Repositories;
using CleanOpsAi.Modules.TaskOperations.Application.Common.Interfaces.Services;
using CleanOpsAi.Modules.TaskOperations.Application.DTOs;
using CleanOpsAi.Modules.TaskOperations.Application.DTOs.Request;
using CleanOpsAi.Modules.TaskOperations.Application.DTOs.Response;
using CleanOpsAi.Modules.TaskOperations.Domain.Entities;
using CleanOpsAi.Modules.TaskOperations.Domain.Enums; 
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
		private readonly IEventBus _eventBus;
		private readonly IPpeCheckNotifier _notifier;

		public TaskStepExecutionService(
			ITaskStepExecutionRepository taskStepExecutionRepository,
			IDateTimeProvider dateTimeProvider,
			IMapper mapper,
			IEventBus eventBus,
			IPpeCheckNotifier notifier)
		{
			_repository = taskStepExecutionRepository;
			_dateTimeProvider = dateTimeProvider;
			_mapper = mapper;
			_eventBus = eventBus;
			_notifier = notifier;
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

		
		public async Task<TaskStepExecutionDetailDto> GetStepDetailAsync(Guid id, CancellationToken ct = default)
		{
			var step = await _repository.GetByIdAsync(id, ct)
				?? throw new NotFoundException(nameof(TaskStepExecution), id);

			return _mapper.Map<TaskStepExecutionDetailDto>(step);
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

				//if (!string.Equals(type, AiPpeCheckBehavior, StringComparison.OrdinalIgnoreCase))
				//{
				//	throw new BadRequestException("AI PPE check step must be checked before completion.");
				//}

				//if (!string.Equals(status, "PASS", StringComparison.OrdinalIgnoreCase) &&
				//	!string.Equals(status, "FAIL", StringComparison.OrdinalIgnoreCase))
				//{
				//	throw new BadRequestException("AI PPE check step must have a valid PASS or FAIL result before completion.");
				//}

				if (!string.Equals(type, AiPpeCheckBehavior, StringComparison.OrdinalIgnoreCase))
					throw new BadRequestException("AI PPE check step must be checked before completion.");
				 
				if (!string.Equals(status, "PASS", StringComparison.OrdinalIgnoreCase))
					throw new BadRequestException("PPE check did not pass. Please retake photos and check again.");
			}
			catch (JsonException)
			{
				throw new BadRequestException("AI PPE check step must be checked before completion.");
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

		private static bool IsResultPending(string? resultData)
		{
			if (string.IsNullOrWhiteSpace(resultData)) return false;
			try
			{
				using var doc = JsonDocument.Parse(resultData);
				return doc.RootElement.TryGetProperty("status", out var s)
					&& string.Equals(s.GetString(), "PENDING", StringComparison.OrdinalIgnoreCase);
			}
			catch { return false; }
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

		public async Task<TaskStepExecutionPpeCheckResponse> RequestPpeCheckAsync(Guid id, CancellationToken ct = default)
		{
			var (step, requiredItems, imageUrls) = await LoadAndValidatePpeContextAsync(id, ct);
			if (IsResultPending(step.ResultData))
				throw new BadRequestException("PPE check is already in progress.");

			var queued = BuildPpeCheckPendingResponse(step, requiredItems, imageUrls);
			step.ResultData = JsonSerializer.Serialize(queued, JsonOptions);
			await _repository.SaveChangesAsync(ct);

			try
			{
				await _eventBus.PublishAsync(new PpeCheckRequestedEvent
				{
					TaskStepExecutionId = step.Id,
					ImageUrls = imageUrls,
					RequiredObjects = requiredItems.Select(x => x.ActionKey).ToList(),
					MinConfidence = DefaultPpeMinConfidence,
				}, ct);
			}
			catch (Exception ex)
			{
				var failed = BuildPpeCheckErrorResponse(step, requiredItems, imageUrls, ex.Message);
				step.ResultData = JsonSerializer.Serialize(failed, JsonOptions);
				await _repository.SaveChangesAsync(ct);
				return failed;
			}

			return queued;
		}


		public async Task ApplyPpeCheckResultAsync(PpeCheckCompletedEvent evt, CancellationToken ct = default)
		{
			var step = await _repository.GetByIdAsync(evt.TaskStepExecutionId, ct)
				?? throw new NotFoundException(nameof(TaskStepExecution), evt.TaskStepExecutionId);

			if (step.Status != TaskStepExecutionStatus.InProgress)
				return;
			 
			var requiredItems = TryParseRequiredPpe(step.ConfigSnapshot);
			var nameByActionKey = requiredItems.ToDictionary(
				x => x.ActionKey,
				x => x.Name,
				StringComparer.OrdinalIgnoreCase);

			var response = new TaskStepExecutionPpeCheckResponse
			{
				TaskStepExecutionId = evt.TaskStepExecutionId,
				SopStepId = step.SopStepId,
				StepOrder = step.StepOrder,
				CheckedAt = _dateTimeProvider.UtcNow,
				Status = NormalizeStatus(evt.Status),
				Message = evt.Message ?? (evt.Status == "PASS" ? "Meets requirements." : "Missing required PPE items."),
				ImageUrls = evt.ImageUrls,
				RequiredPpe = requiredItems,
				DetectedItems = evt.DetectedItems.Select(x =>
				{
					var actionKey = NormalizeLabel(x.Name);
					// "helmet" → "Nón bảo hộ"
					nameByActionKey.TryGetValue(actionKey, out var displayName);
					return new TaskStepExecutionPpeDetectedItemResponse
					{
						ActionKey = actionKey,
						Name = displayName ?? actionKey,  
						Confidence = x.Confidence,
						ImageIndex = x.ImageIndex,
					};
				}).ToList(),
				MissingItems = evt.MissingItems.Select(actionKey =>
				{
					nameByActionKey.TryGetValue(actionKey, out var displayName);
					return new TaskStepExecutionPpeRequiredItemResponse
					{
						ActionKey = actionKey,
						Name = displayName ?? actionKey,  // ← "gloves" → "Găng tay"
					};
				}).ToList(),
				FailedImages = evt.FailedImages.Select(x => new TaskStepExecutionPpeFailedImageResponse
				{
					ImageUrl = x.ImageUrl ?? string.Empty,
					ImageIndex = x.ImageIndex,
					Error = x.Error ?? string.Empty,
				}).ToList(),
			};

			step.ResultData = JsonSerializer.Serialize(response, JsonOptions);
			await _repository.SaveChangesAsync(ct);

			await _notifier.NotifyAsync(new PpeCheckNotification
			{
				TaskStepExecutionId = step.Id,
				Status = response.Status,
				Message = response.Message,
				MissingItems = evt.MissingItems,
				At = _dateTimeProvider.UtcNow,
			}, ct);
		}
	}
}