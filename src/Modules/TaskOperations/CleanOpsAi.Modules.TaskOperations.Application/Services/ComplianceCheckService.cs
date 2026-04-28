using CleanOpsAi.BuildingBlocks.Application;
using CleanOpsAi.BuildingBlocks.Application.Exceptions;
using CleanOpsAi.BuildingBlocks.Application.Interfaces;
using CleanOpsAi.BuildingBlocks.Application.Interfaces.Messaging;
using CleanOpsAi.BuildingBlocks.Application.Pagination;
using CleanOpsAi.BuildingBlocks.Domain.Dtos.Notifications;
using CleanOpsAi.BuildingBlocks.Infrastructure.Events;
using CleanOpsAi.BuildingBlocks.Infrastructure.Events.Request;
using CleanOpsAi.Modules.TaskOperations.Application.Common.Interfaces.Repositories;
using CleanOpsAi.Modules.TaskOperations.Application.Common.Interfaces.Services;
using CleanOpsAi.Modules.TaskOperations.Application.DTOs.Request;
using CleanOpsAi.Modules.TaskOperations.Application.DTOs.Response;
using CleanOpsAi.Modules.TaskOperations.Domain.Entities;
using CleanOpsAi.Modules.TaskOperations.Domain.Enums;
using MassTransit.Middleware;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace CleanOpsAi.Modules.TaskOperations.Application.Services
{ 
    public class ComplianceCheckService : IComplianceCheckService
    { 
        private const double FailThreshold = 50.0;
        private const double PendingThreshold = 80.0;
        private const int FailCountForAutoFail = 2;

        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };

        private readonly IComplianceCheckRepository _complianceRepo;
        private readonly ITaskStepExecutionImageRepository _imageRepo;
        private readonly ITaskStepExecutionRepository _stepExecutionRepo;
        private readonly IEventBus _eventBus;
        private readonly IComplianceNotifier _notifier;
        private readonly ILogger<ComplianceCheckService> _logger;
        private readonly IIdGenerator _idGenerator;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly ISupervisorQueryService _supervisorQueryService;
		private readonly IUserContext _userContext;

		private string environmentKey = "LOBBY_CORRIDOR";


        public ComplianceCheckService(
            IComplianceCheckRepository complianceRepo,
            ITaskStepExecutionImageRepository imageRepo,
            ITaskStepExecutionRepository stepExecutionRepo,
            IEventBus eventBus,
            IComplianceNotifier notifier,
            ILogger<ComplianceCheckService> logger,
            IIdGenerator idGenerator,
            IDateTimeProvider dateTimeProvider,
            ISupervisorQueryService supervisorQueryService,
			IUserContext userContext)
        {
            _complianceRepo = complianceRepo;
            _imageRepo = imageRepo;
            _stepExecutionRepo = stepExecutionRepo;
            _eventBus = eventBus;
            _notifier = notifier;
            _logger = logger;
            _idGenerator = idGenerator;
			_dateTimeProvider = dateTimeProvider;
            _supervisorQueryService = supervisorQueryService;
			_userContext = userContext;
		}

		public async Task<InitiateAiCheckResult> InitiateAiCheckAsync(Guid taskStepExecutionId, CancellationToken ct = default)
		{
			var afterImages = await _imageRepo.GetActiveByExecutionIdAndTypeAsync(
				taskStepExecutionId, ImageType.After, ct);

			if (!afterImages.Any())
			{
				throw new InvalidOperationException(
					$"No 'After' images found for step execution {taskStepExecutionId}. " +
					"Upload images before initiating an AI check.");
			}

			var check = new ComplianceCheck
			{
				Id = _idGenerator.Generate(),
				TaskStepExecutionId = taskStepExecutionId,
				Type = ComplianceCheckType.Automated,
				Status = ComplianceCheckStatus.Pending,
				MinScore = 0,
				FailedImageCount = 0,
				Created = _dateTimeProvider.UtcNow,
				CreatedBy = "system"
			};
			await _complianceRepo.InsertAsync(check, ct);  

			await _eventBus.PublishAsync(new AiScoringRequestedEvent
			{
				ComplianceCheckId = check.Id,
				TaskStepExecutionId = taskStepExecutionId,
				EnvironmentKey = environmentKey,
				ImageUrls = afterImages.Select(img => img.ImageUrl).ToList()
			}, ct);
			 
			check.Status = ComplianceCheckStatus.Processing;
			await _complianceRepo.SaveChangesAsync(ct);
			_logger.LogInformation(
				"AiScoringRequestedEvent published, ComplianceCheck {CheckId} → Processing",
				check.Id);

			return new InitiateAiCheckResult
			{
				ComplianceCheckId = check.Id,
				Type = check.Type,
				Status = check.Status,
				Created = _dateTimeProvider.UtcNow.AddHours(7)
			};
		}


		public async Task ApplyScoringResultsAsync(
	ScoringCompletedEvent evt,
	CancellationToken ct = default)
		{
			// ── 1. Validate RequestId ─────────────────────────────────────────
			if (!Guid.TryParse(evt.RequestId, out var requestIdGuid))
			{ 
				return;
			}
			 
			var check = await _complianceRepo.GetByIdAsync(requestIdGuid, ct);
			if (check == null)
			{
				_logger.LogError(
					"No ComplianceCheck found with ID {RequestId} from scoring results.",
					evt.RequestId);
				return;
			}

			var images = await _imageRepo.GetActiveByExecutionIdAndTypeAsync(check.TaskStepExecutionId, ImageType.After, ct);
			if (images.Count == 0)
			{
				_logger.LogWarning(
					"No images found for ExecutionId {ExecutionId}. Scoring results not applied.",
					check.TaskStepExecutionId);
				return;
			}

			var imageMap = images.ToDictionary(img => img.ImageUrl, StringComparer.OrdinalIgnoreCase);
			foreach (var result in evt.Results)
			{
				if (!imageMap.TryGetValue(result.ImageUrl, out var image))
				{
					_logger.LogWarning(
						"No image record matches URL '{Url}' in execution {ExecutionId}. Skipping.",
						result.ImageUrl, check.TaskStepExecutionId);
					continue;
				}

				image.QualityScore = result.QualityScore;
				image.Verdict = result.Verdict;

				_logger.LogDebug(
					"Image {ImageId}: QualityScore={Score}, Verdict={Verdict}",
					image.Id, result.QualityScore, result.Verdict);
			}
			 
			var scoredValues = images
				.Where(img => img.QualityScore.HasValue)
				.Select(img => img.QualityScore!.Value)
				.ToList();

			double minScore = scoredValues.Count > 0 ? scoredValues.Min() : 0.0;
			int failedImageCount = scoredValues.Count(s => s < FailThreshold);

			// ── 5. Determine compliance status ───────────────────────────────
			var status = DetermineStatus(minScore, failedImageCount);

			// ── 6. Update check ───────────────────────────────────────────────
			check.Status = status;
			check.MinScore = minScore;
			check.FailedImageCount = failedImageCount;
			check.AIResultRaw = JsonSerializer.Serialize(evt, _jsonOptions);

			// ── 7. Resolve supervisor when PendingSupervisor ───────────────
			if (status == ComplianceCheckStatus.PendingSupervisor)
			{
				var execution = await _stepExecutionRepo.GetByIdDetail(check.TaskStepExecutionId, ct);
				if (execution?.TaskAssignment is not null)
				{
					try
					{
						var supervisorId = await _supervisorQueryService.GetSupervisorIdAsync(
							execution.TaskAssignment.WorkAreaId,
							execution.TaskAssignment.AssigneeId,
							ct);

						if (supervisorId.HasValue)
						{
							check.SupervisorId = supervisorId;
							try
							{
								await _eventBus.PublishAsync(new SendNotificationEvent
								{
									Title = "Compliance Check cần duyệt",
									Body = "Có ảnh cần bạn review và phê duyệt.",
									Priority = NotificationPriority.High,
									SenderType = SenderTypeEnum.System,
									Recipients = new List<NotificationRecipientEvent>
									{
										new()
										{
											RecipientType = RecipientTypeEnum.Supervisor,
											RecipientId = supervisorId.Value
										}
									}
								}, ct);
								_logger.LogInformation(
									"Notification sent to Supervisor {SupervisorId} for ComplianceCheck {CheckId}",
									supervisorId, check.Id);
							}
							catch (Exception ex)
							{
								_logger.LogWarning(ex,
									"Failed to send notification to Supervisor {SupervisorId} for ComplianceCheck {CheckId}. Continuing.",
									supervisorId, check.Id);
							}
						}
						else
						{
							_logger.LogWarning(
								"No supervisor found for ComplianceCheck {CheckId}. SupervisorId left null.",
								check.Id);
						}
					}
					catch (Exception ex)
					{
						_logger.LogWarning(ex,
							"Failed to resolve supervisor for ComplianceCheck {CheckId}. Continuing without SupervisorId.",
							check.Id);
					}
				}
				else
				{
					_logger.LogWarning(
						"TaskStepExecution {ExecutionId} not found or has no TaskAssignment. Cannot resolve supervisor.",
						check.TaskStepExecutionId);
				}
			}

			// ── 8. Persist ────────────────────────────────────────────────────
			await _complianceRepo.SaveChangesAsync(ct);
			_logger.LogInformation(
				"ComplianceCheck {CheckId} saved: Status={Status}, MinScore={MinScore}, FailedImageCount={FailedCount}",
				check.Id, status, minScore, failedImageCount);

			
			var notification = new ComplianceCheckNotification
			{
				ComplianceCheckId = check.Id,
				TaskStepExecutionId = check.TaskStepExecutionId,
				Status = status.ToString(),
				CheckedBy = "AI",
				MinScore = minScore,
				FailedImageCount = failedImageCount,
				Action = MapAction(status),
				At = _dateTimeProvider.UtcNow.AddHours(7)
			};

			try
			{
				await _notifier.NotifyAsync(notification, ct);
				_logger.LogInformation(
					"SignalR notification sent for ComplianceCheck {CheckId} (Action={Action})",
					check.Id, notification.Action);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex,
					"SignalR notification failed for ComplianceCheck {CheckId}. " +
					"Client will recover via polling endpoint.",
					check.Id);
			}
		}


		public async Task<ComplianceCheckStatusResponse?> GetCurrentStatusAsync(
            Guid taskStepExecutionId,
            CancellationToken ct = default)
        {
            var check = await _complianceRepo.GetByExecutionIdAndTypeAsync(
                taskStepExecutionId, ComplianceCheckType.Automated, ct);

            if (check is null)
                return null;

            return new ComplianceCheckStatusResponse
            {
                ComplianceCheckId = check.Id,
                TaskStepExecutionId = check.TaskStepExecutionId,
                Status = check.Status.ToString(),
                MinScore = check.MinScore,
                FailedImageCount = check.FailedImageCount,
                UpdatedAt = check.LastModified == default ? null : check.LastModified
            };
        }

        

        /// <summary>
        /// Business rules (unchanged):
        /// <list type="bullet">
        ///   <item><c>FailedImageCount >= 2</c> → Failed</item>
        ///   <item><c>FailedImageCount == 1</c> OR <c>50 &lt;= MinScore &lt; 80</c> → PendingSupervisor</item>
        ///   <item>Otherwise → Passed</item>
        /// </list>
        /// </summary>
        private static ComplianceCheckStatus DetermineStatus(double minScore, int failedImageCount)
        {
            if (failedImageCount >= FailCountForAutoFail)
                return ComplianceCheckStatus.Failed;

            if (failedImageCount == 1 || (minScore >= FailThreshold && minScore < PendingThreshold))
                return ComplianceCheckStatus.PendingSupervisor;

            return ComplianceCheckStatus.Passed;
        }

		private static string MapAction(ComplianceCheckStatus status) => status switch
		{
			ComplianceCheckStatus.Pending => "None",
			ComplianceCheckStatus.Processing => "WaitForAI",
			ComplianceCheckStatus.Passed => "None",
			ComplianceCheckStatus.PendingSupervisor => "WaitForSupervisor",
			ComplianceCheckStatus.Failed => "RetakePhotos",
			_ => "None"
		};

		public async Task<PaginatedResult<PendingSupervisorCheckDto>> GetPendingSupervisorChecksAsync(
		PaginationRequest request,
		CancellationToken ct = default)
		{
			var paged = await _complianceRepo.GetPendingSupervisorChecksAsync(_userContext.UserId ,request, ct);

			var mapped = paged.Content.Select(check => new PendingSupervisorCheckDto
			{
				ComplianceCheckId = check.Id,
				TaskStepExecutionId = check.TaskStepExecutionId,
				MinScore = check.MinScore,
				FailedImageCount = check.FailedImageCount,
				CreatedAt = check.Created.AddHours(7)
			}).ToList();

			return new PaginatedResult<PendingSupervisorCheckDto>(
				paged.PageNumber,
				paged.PageSize,
				paged.TotalElements,
				mapped);
		}

		public async Task<SupervisorCheckDetailDto> GetSupervisorCheckDetailAsync(
		Guid complianceCheckId,
		CancellationToken ct = default)
		{
			var check = await _complianceRepo.GetByIdAsync(complianceCheckId, ct);
			if (check == null) throw new NotFoundException(nameof(ComplianceCheck), complianceCheckId);

			var images = new List<ScoringImageDetailDto>();

			if (!string.IsNullOrEmpty(check.AIResultRaw))
			{
				var raw = JsonSerializer.Deserialize<ScoringCompletedEvent>(
					check.AIResultRaw, _jsonOptions);

				images = raw?.Results.Select(r => new ScoringImageDetailDto
				{
					ImageUrl = r.ImageUrl,
					VisualizationUrl = r.VisualizationBlobUrl,
					QualityScore = r.QualityScore,
					Verdict = r.Verdict
				}).ToList() ?? new();
			}

			return new SupervisorCheckDetailDto
			{
				ComplianceCheckId = check.Id,
				TaskStepExecutionId = check.TaskStepExecutionId,
				MinScore = check.MinScore,
				FailedImageCount = check.FailedImageCount,
				Feedback = check.Feedback,
				CreatedAt = check.Created.AddHours(7),
				Images = images
			};
		}

		public async Task ApplySupervisorReviewAsync(
			Guid complianceCheckId,
			SupervisorReviewRequest request,
			CancellationToken ct = default)
		{
			var check = await _complianceRepo.GetByIdAsync(complianceCheckId, ct);
			if (check == null) throw new NotFoundException(nameof(ComplianceCheck), complianceCheckId); 

			if(check.SupervisorId != _userContext.UserId)
			{
				throw new UnauthorizedAccessException("You are not authorized to review this compliance check.");
			}

			check.Status = request.Approved
				? ComplianceCheckStatus.Passed
				: ComplianceCheckStatus.Failed;
			check.Feedback = request.Feedback;
			check.LastModified = _dateTimeProvider.UtcNow;
			check.LastModifiedBy = _userContext.UserId.ToString();

			await _complianceRepo.SaveChangesAsync(ct);

			await _notifier.NotifyAsync(new ComplianceCheckNotification
			{
				ComplianceCheckId = check.Id,
				TaskStepExecutionId = check.TaskStepExecutionId,
				Status = check.Status.ToString(),
				CheckedBy = _userContext.UserId.ToString(),
				MinScore = check.MinScore,
				FailedImageCount = check.FailedImageCount,
				Action = MapAction(check.Status),
				At = _dateTimeProvider.UtcNow.AddHours(7)
			}, ct);
		}
	}
}
