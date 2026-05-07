using AutoMapper;
using CleanOpsAi.BuildingBlocks.Application;
using CleanOpsAi.BuildingBlocks.Application.Common;
using CleanOpsAi.BuildingBlocks.Application.Exceptions;
using CleanOpsAi.BuildingBlocks.Application.Interfaces; 
using CleanOpsAi.BuildingBlocks.Application.Pagination;
using CleanOpsAi.BuildingBlocks.Domain.Dtos.Notifications;
using CleanOpsAi.BuildingBlocks.Infrastructure.Events;
using CleanOpsAi.Modules.TaskOperations.Application.Common.Interfaces.Repositories;
using CleanOpsAi.Modules.TaskOperations.Application.Common.Interfaces.Services;
using CleanOpsAi.Modules.TaskOperations.Application.DTOs.Request;
using CleanOpsAi.Modules.TaskOperations.Application.DTOs.Response;
using CleanOpsAi.Modules.TaskOperations.Domain.Entities;
using CleanOpsAi.Modules.TaskOperations.Domain.Enums;
using System.Text.Json;

namespace CleanOpsAi.Modules.TaskOperations.Application.Services
{
	public class TaskSwapRequestService : ITaskSwapRequestService
	{
		private readonly ITaskSwapRequestRepository _taskSwapRequestRepository;
		private readonly ITaskAssignmentRepository _taskAssignmentRepository;
		private readonly IMapper _mapper;
		private readonly IDateTimeProvider _dateTimeProvider;
		private readonly IIdGenerator _idGenerator;
		private readonly IUserContext _userContext; 
		private readonly IWorkerQueryService _workerQueryService;
		private readonly ISopRequirementsQueryService _sopRequirementsQueryService;
		private readonly IWorkerCertificationSkillQueryService _workerCertificationSkillQueryService;
		private readonly ISupervisorQueryService _supervisorQueryService;
		private readonly INotificationPublisher _notificationPublisher;

		public TaskSwapRequestService(
			ITaskSwapRequestRepository taskSwapRequestRepository,
			ITaskAssignmentRepository taskAssignmentRepository,
			IMapper mapper, 
			IDateTimeProvider dateTimeProvider, 
			IIdGenerator idGenerator,
			IUserContext userContext, 
			IWorkerQueryService workerQueryService, 
			ISopRequirementsQueryService sopRequirementsQueryService, 
			IWorkerCertificationSkillQueryService workerCertificationSkillQueryService,
			ISupervisorQueryService supervisorQueryService,
			INotificationPublisher notificationPublisher)
		{
			_taskSwapRequestRepository = taskSwapRequestRepository;
			_taskAssignmentRepository = taskAssignmentRepository;
			_mapper = mapper;
			_dateTimeProvider = dateTimeProvider;
			_idGenerator = idGenerator;
			_userContext = userContext;
			_workerQueryService = workerQueryService;
			_sopRequirementsQueryService = sopRequirementsQueryService;
			_workerCertificationSkillQueryService = workerCertificationSkillQueryService;
			_supervisorQueryService = supervisorQueryService;
			_notificationPublisher = notificationPublisher;
		}

		public async Task<SwapRequestDto> GetById(Guid id, CancellationToken ct = default)
		{
			var swapRequest = await _taskSwapRequestRepository.GetByIdWithDetailsAsync(id, ct);
			if (swapRequest == null)
				throw new NotFoundException(nameof(TaskSwapRequest), id);

			return _mapper.Map<SwapRequestDto>(swapRequest);
		}

		public async Task<Result> CancelSwapRequestAsync(Guid swapRequestId, Guid requesterId, CancellationToken ct = default)
		{
			var swapRequest = await _taskSwapRequestRepository.GetByIdWithDetailsAsync(swapRequestId, ct);
			if (swapRequest == null)
				throw new NotFoundException(nameof(TaskSwapRequest), swapRequestId);

			if (swapRequest.RequesterId != requesterId)
				throw new ForbiddenException("You are not the requester of this swap request");

			swapRequest.Status = SwapRequestStatus.CancelledByRequester;
			await _taskSwapRequestRepository.SaveChangesAsync(ct);

			return Result.Success();
		}

		public async Task<Result<SwapRequestDto>> CreateSwapRequestAsync(TaskSwapRequestCreateDto dto, CancellationToken ct = default)
		{ 

			var requesterTask = await _taskAssignmentRepository.GetByIdAsync(dto.TaskAssignmentId, ct);
			if (requesterTask == null)
				throw new NotFoundException(nameof(TaskAssignment), dto.TaskAssignmentId);

			ValidateRequesterTask(requesterTask, dto.RequesterId);

			var targetTask = await _taskAssignmentRepository.GetByIdAsync(dto.TargetTaskAssignmentId, ct);
			if (targetTask == null)
				throw new NotFoundException(nameof(TaskAssignment), dto.TargetTaskAssignmentId);


			ValidateTargetTask(targetTask, dto.TargetWorkerId);  
			ValidateSwapRules(requesterTask, targetTask);

			await ValidateWorkerCompetencyAsync(requesterTask, targetTask, dto.RequesterId, dto.TargetWorkerId, ct);

			var hasPending = await _taskSwapRequestRepository.HasPendingSwapAsync(dto.TaskAssignmentId);
			if (hasPending)
				throw new BadRequestException("Already has pending swap");

			var requesterHasConflict = await _taskAssignmentRepository.HasTimeConflictAsync(
			   excludeTaskId: dto.TaskAssignmentId,       
			   assigneeId: dto.RequesterId,
			   scheduledStartAt: targetTask.ScheduledStartAt,
			   scheduledEndAt: targetTask.ScheduledEndAt,
			   ct);
			if (requesterHasConflict)
				throw new BadRequestException("Requester has a conflicting task in that time slot");

			var targetHasConflict = await _taskAssignmentRepository.HasTimeConflictAsync(
				excludeTaskId: dto.TargetTaskAssignmentId,  
				assigneeId: dto.TargetWorkerId,
				scheduledStartAt: requesterTask.ScheduledStartAt,
				scheduledEndAt: requesterTask.ScheduledEndAt,
				ct);
			if (targetHasConflict)
				throw new BadRequestException("Target worker has a conflicting task in that time slot");

			var userIds = new List<Guid> { dto.RequesterId, dto.TargetWorkerId };
			var userDict = await _workerQueryService.GetUserNames(userIds);

			if (!userDict.TryGetValue(dto.RequesterId, out var requesterName))
				throw new BadRequestException("Requester not found");

			if (!userDict.TryGetValue(dto.TargetWorkerId, out var targetName))
				throw new BadRequestException("Target worker not found");

			var swapRequest = new TaskSwapRequest
			{
				Id = _idGenerator.Generate(),
				TaskAssignmentId = dto.TaskAssignmentId,
				TargetTaskAssignmentId = dto.TargetTaskAssignmentId,
				RequesterId = dto.RequesterId,
				TargetWorkerId = dto.TargetWorkerId,
				RequesterName = userDict[dto.RequesterId],
				TargetWorkerName = userDict[dto.TargetWorkerId],
				Status = SwapRequestStatus.PendingTargetApproval,
				RequesterNote = dto.RequesterNote,
				ExpiredAt = _dateTimeProvider.UtcNow.AddHours(12),
				Created = _dateTimeProvider.UtcNow,
				CreatedBy = _userContext.UserId.ToString()
			};

			await _taskSwapRequestRepository.InsertAsync(swapRequest, ct);
			await _taskSwapRequestRepository.SaveChangesAsync(ct);

			//notifi to Target
			var notification = new SendNotificationEvent
			{
				Title = "Yêu cầu đổi ca",
				Body = $"{requesterName} muốn đổi ca với bạn. Vui lòng xem xét và phản hồi.",
				Payload = JsonSerializer.Serialize(new
				{
					type = "SWAP_REQUEST",
					swapRequestId = swapRequest.Id,
					requesterTaskAssignmentId = swapRequest.TaskAssignmentId,
					targetTaskAssignmentId = swapRequest.TargetTaskAssignmentId,
				}),
				Priority = NotificationPriority.Normal,
				SenderType = SenderTypeEnum.Worker,
				SenderId = _userContext.UserId,
				Recipients = new List<NotificationRecipientEvent>
				{
					new NotificationRecipientEvent
					{
						RecipientType = RecipientTypeEnum.Worker,
						RecipientId = dto.TargetWorkerId  // WorkerId — query theo WorkerId trong FCM
					}
				}
			};

			await _notificationPublisher.PublishAsync(notification, ct);

			return Result<SwapRequestDto>.Success(_mapper.Map<SwapRequestDto>(swapRequest));
		}

		public async Task<Result<PaginatedResult<SwapCandidateDto>>> GetSwapCandidatesAsync(GetSwapCandidatesDto dto, PaginationRequest paginationRequest, CancellationToken ct = default)
		{
			var requesterTask = await _taskAssignmentRepository.GetByIdAsync(dto.TaskAssignmentId, ct);
			if (requesterTask == null)
				throw new NotFoundException(nameof(TaskAssignment), dto.TaskAssignmentId);

			var now = _dateTimeProvider.UtcNow;
			if (requesterTask.ScheduledStartAt <= now.AddHours(2))
				throw new BadRequestException("Không thể đổi ca cho task bắt đầu trong vòng 2 tiếng");

			var requirements = await _sopRequirementsQueryService.GetSopRequirementsByScheduleId(requesterTask.TaskScheduleId, ct);

			List<Guid>? qualifiedWorkerIds = null;
			if (requirements.Found &&
				(requirements.RequiredSkillIds.Any() || requirements.RequiredCertificationIds.Any()))
			{
				qualifiedWorkerIds = await _workerCertificationSkillQueryService
					.GetQualifiedWorkersAsync(
						requirements.RequiredSkillIds,
						requirements.RequiredCertificationIds,
						ct);
			}


			//var today = _dateTimeProvider.UtcNow;
			//var startOfWeek = today.AddDays(-(int)today.DayOfWeek + (int)DayOfWeek.Monday);
			//var endOfWeek = startOfWeek.AddDays(7);

			var today = _dateTimeProvider.UtcNow;
			var diff = (7 + (int)today.DayOfWeek - (int)DayOfWeek.Monday) % 7;
			var startOfWeek = today.AddDays(-diff).Date;
			var endOfWeek = startOfWeek.AddDays(7);

			var cutoffTime = now.AddHours(2);

			var candidates = await _taskAssignmentRepository.GetSwapCandidatesAsync(
			   requesterTaskId: requesterTask.Id,
			   requesterAssigneeId: requesterTask.AssigneeId,
			   requesterScheduledStartAt: requesterTask.ScheduledStartAt,
			   requesterScheduledEndAt: requesterTask.ScheduledEndAt,
			   workAreaId: requesterTask.WorkAreaId,
			   weekStart: startOfWeek,
			   weekEnd: endOfWeek,
			   cutoffTime: cutoffTime,	
			   date: dto.Date,
			   preferredStartTime: dto.PreferredStartTime,
			   qualifiedWorkerIds: qualifiedWorkerIds,
			   paginationRequest: paginationRequest,
			   ct: ct);

			return Result<PaginatedResult<SwapCandidateDto>>.Success(
				new PaginatedResult<SwapCandidateDto>(
					candidates.PageNumber,
					candidates.PageSize,
					candidates.TotalElements,
					_mapper.Map<List<SwapCandidateDto>>(candidates.Content)
				)
			);
		}

		public async Task<Result> RespondSwapRequestAsync(RespondSwapRequestDto dto, CancellationToken ct = default)
		{
			var swapRequest = await _taskSwapRequestRepository.GetByIdWithDetailsAsync(dto.SwapRequestId);
			if (swapRequest == null)
				throw new NotFoundException(nameof(TaskSwapRequest), dto.SwapRequestId);

			if (swapRequest.TargetWorkerId != dto.ResponderId)
				throw new ForbiddenException("You are not the one who was asked to swap");

			if(swapRequest.Status != SwapRequestStatus.PendingTargetApproval)
				throw new BadRequestException("The request is not in a pending confirmation state");

			if (DateTime.UtcNow > swapRequest.ExpiredAt)
			{
				swapRequest.Status = SwapRequestStatus.Expired;
				await _taskSwapRequestRepository.SaveChangesAsync(ct);
				throw new BadRequestException("The request has expired.");
			}

			if (!dto.IsAccepted)
			{
				swapRequest.Status = SwapRequestStatus.RejectedByTarget;
				await _taskSwapRequestRepository.SaveChangesAsync(ct);

				// notifi reject to requester
				await _notificationPublisher.PublishAsync(new SendNotificationEvent
				{
					Title = "Yêu cầu đổi ca bị từ chối",
					Body = $"{swapRequest.TargetWorkerName} đã từ chối yêu cầu đổi ca của bạn.",
					SenderType = SenderTypeEnum.Worker,
					Payload = JsonSerializer.Serialize(new
					{
						type = "SWAP_RESPONSE",
						swapRequestId = swapRequest.Id,
						status = swapRequest.Status.ToString(),
						requesterTaskAssignmentId = swapRequest.TaskAssignmentId,
						targetTaskAssignmentId = swapRequest.TargetTaskAssignmentId
					}),
					SenderId = _userContext.UserId,
					Recipients = new List<NotificationRecipientEvent>
					{
						new() { RecipientType = RecipientTypeEnum.Worker, RecipientId = swapRequest.RequesterId }
					}
				}, ct);
				return Result.Success();
			}

			swapRequest.Status = SwapRequestStatus.PendingSupervisorApproval;
			var workAreaId = swapRequest.TaskAssignment.WorkAreaId;
			var supervisorResp = await _supervisorQueryService.GetSupervisorWorkAreasAsync(
				workerId: swapRequest.RequesterId,
				workerIdTarget: swapRequest.TargetWorkerId,
				workAreaId: workAreaId,
				ct: ct);

			if(!supervisorResp.Found || supervisorResp.SupervisorUserId == null)
				throw new BadRequestException("No common supervisor found for approval in same area with worker and target worker");
			
			swapRequest.ReviewedByUserId = supervisorResp.SupervisorUserId.Value; 
			await _taskSwapRequestRepository.SaveChangesAsync(ct);
			 
			await _notificationPublisher.PublishAsync(new SendNotificationEvent
			{
				Title = "Yêu cầu đổi ca cần phê duyệt",
				Body = $"{swapRequest.RequesterName} và {swapRequest.TargetWorkerName} muốn đổi ca, cần bạn phê duyệt.",
				SenderType = SenderTypeEnum.Worker,
				SenderId = _userContext.UserId,
				Recipients = new List<NotificationRecipientEvent>
				{
					new() { RecipientType = RecipientTypeEnum.Supervisor, RecipientId = supervisorResp.SupervisorUserId.Value }
				}
			}, ct);

			await _notificationPublisher.PublishAsync(new SendNotificationEvent
			{
				Title = "Yêu cầu đổi ca đã được chấp nhận",
				Body = $"{swapRequest.TargetWorkerName} đã đồng ý đổi ca, đang chờ supervisor phê duyệt.",
				SenderType = SenderTypeEnum.Worker,
				SenderId = _userContext.UserId,
				Recipients = new List<NotificationRecipientEvent>
				{
					new() { RecipientType = RecipientTypeEnum.Worker, RecipientId = swapRequest.RequesterId }
				}
			}, ct);

			return Result.Success();
		}

		public async Task<Result> ReviewSwapRequestAsync(ReviewSwapRequestDto dto, CancellationToken ct = default)
		{

			var swapRequest = await _taskSwapRequestRepository.GetByIdWithDetailsAsync(dto.TaskSwapRequestId, ct);
			if (swapRequest == null)
				throw new NotFoundException(nameof(TaskSwapRequest), dto.TaskSwapRequestId); 

			if (swapRequest.Status != SwapRequestStatus.PendingSupervisorApproval)
				throw new BadRequestException("The request is not in a pending state");

			swapRequest.ReviewedByUserId = _userContext.UserId;
			swapRequest.ReviewerName = _userContext.FullName;
			swapRequest.ReviewNote = dto.ReviewNote;

			if (!dto.IsApproved)
			{
				swapRequest.Status = SwapRequestStatus.RejectedBySupervisor;
				await _taskSwapRequestRepository.SaveChangesAsync();

				await _notificationPublisher.PublishAsync(new SendNotificationEvent
				{
					Title = "Yêu cầu đổi ca bị từ chối",
					Body = $"Supervisor {swapRequest.ReviewerName} đã từ chối yêu cầu đổi ca. Lý do: {dto.ReviewNote}",
					SenderType = SenderTypeEnum.Supervisor,
					SenderId = _userContext.UserId,
					Payload = JsonSerializer.Serialize(new
					{
						type = "SWAP_REVIEW",
						swapRequestId = swapRequest.Id,
						status = swapRequest.Status.ToString(),
						isApproved = dto.IsApproved,
						reviewerId = _userContext.UserId,
						reviewerName = swapRequest.ReviewerName,
						note = dto.ReviewNote,
						requesterTaskAssignmentId = swapRequest.TaskAssignmentId,
						targetTaskAssignmentId = swapRequest.TargetTaskAssignmentId
					}),
					Recipients = new List<NotificationRecipientEvent>
			{
				new() { RecipientType = RecipientTypeEnum.Worker, RecipientId = swapRequest.RequesterId },
				new() { RecipientType = RecipientTypeEnum.Worker, RecipientId = swapRequest.TargetWorkerId }
			}
				}, ct);
				return Result.Success();
			}

			var requesterTask = swapRequest.TaskAssignment;
			var targetTask = swapRequest.TargetTaskAssignment;

			(requesterTask.AssigneeId, targetTask.AssigneeId) = (targetTask.AssigneeId, requesterTask.AssigneeId);
			(requesterTask.AssigneeName, targetTask.AssigneeName) = (targetTask.AssigneeName, requesterTask.AssigneeName);
			swapRequest.Status = SwapRequestStatus.Approved;

			await _taskSwapRequestRepository.SaveChangesAsync();

			//notify faild to worker and target
			await _notificationPublisher.PublishAsync(new SendNotificationEvent
			{
				Title = "Yêu cầu đổi ca đã được phê duyệt",
				Body = $"Supervisor {swapRequest.ReviewerName} đã phê duyệt yêu cầu đổi ca giữa {swapRequest.RequesterName} và {swapRequest.TargetWorkerName}.",
				SenderType = SenderTypeEnum.Supervisor,
				SenderId = _userContext.UserId,
				Recipients = new List<NotificationRecipientEvent>
		{
			new() { RecipientType = RecipientTypeEnum.Worker, RecipientId = swapRequest.RequesterId },
			new() { RecipientType = RecipientTypeEnum.Worker, RecipientId = swapRequest.TargetWorkerId }
		}
			}, ct);

			return Result.Success();
		}

		private void ValidateRequesterTask(TaskAssignment task, Guid requesterId)
		{
			if (task.AssigneeId != requesterId)
				throw new BadRequestException("Not your task");

			if (task.Status != TaskAssignmentStatus.NotStarted)
				throw new BadRequestException("Task was started or completed");

			if (task.ScheduledStartAt - DateTime.UtcNow < TimeSpan.FromHours(2))
				throw new BadRequestException("Task must be at least 2 hours away");
		}

		private void ValidateTargetTask(TaskAssignment task, Guid targetWorkerId)
		{
			if (task.AssigneeId != targetWorkerId)
				throw new BadRequestException("This task does not belong to this worker");

			if (task.Status != TaskAssignmentStatus.NotStarted)
				throw new BadRequestException("Target task already started/completed");

			if (task.ScheduledStartAt - DateTime.UtcNow < TimeSpan.FromHours(2))
				throw new BadRequestException("Target task too close");
		}

		private void ValidateSwapRules(TaskAssignment a, TaskAssignment b)
		{
			if (a.WorkAreaId != b.WorkAreaId)
				throw new BadRequestException("Different work areas");

			var daysDiff = Math.Abs((a.ScheduledStartAt.Date - b.ScheduledStartAt.Date).Days);
			if (daysDiff > 7)
				throw new BadRequestException("Tasks must be within same week");
		}

		public async Task<PaginatedResult<SwapRequestDto>> GetSwapRequestsAsync(GetSwapRequestsDto dto, PaginationRequest paginationRequest, CancellationToken ct = default)
		{
			if(_userContext.Role != "Supervisor")
			{
				throw new ForbiddenException("Only supervisors can view swap requests list");
			}
			var result = await _taskSwapRequestRepository.GetSwapRequestsPaging(_userContext.UserId , dto.Status, paginationRequest, ct);

			return new PaginatedResult<SwapRequestDto>(
				result.PageNumber,
				result.PageSize,
				result.TotalElements,
				_mapper.Map<List<SwapRequestDto>>(result.Content));
		}

		private async Task ValidateWorkerCompetencyAsync(
			TaskAssignment requesterTask,
			TaskAssignment targetTask,
			Guid requesterId,
			Guid targetWorkerId,
			CancellationToken ct)
		{
			// Take requirements song song
			var taskA = _sopRequirementsQueryService.GetSopRequirementsByScheduleId(requesterTask.TaskScheduleId, ct);

			var taskB = _sopRequirementsQueryService
				.GetSopRequirementsByScheduleId(targetTask.TaskScheduleId, ct); 

			await Task.WhenAll(taskA, taskB);

			var reqA = taskA.Result;
			var reqB = taskB.Result;

			// B phải đủ điều kiện làm task A
			if (reqA.Found && (reqA.RequiredSkillIds.Any() || reqA.RequiredCertificationIds.Any()))
			{
				var bQualified = await _workerCertificationSkillQueryService.IsWorkerQualifiedAsync(
					targetWorkerId,
					reqA.RequiredSkillIds,
					reqA.RequiredCertificationIds,
					ct);

				if (!bQualified)
					throw new BadRequestException("Worker B does not meet requirements for Worker A's task");
			}

			// A phải đủ điều kiện làm task B
			if (reqB.Found && (reqB.RequiredSkillIds.Any() || reqB.RequiredCertificationIds.Any()))
			{
				var aQualified = await _workerCertificationSkillQueryService.IsWorkerQualifiedAsync(
					requesterId,
					reqB.RequiredSkillIds,
					reqB.RequiredCertificationIds,
					ct);

				if (!aQualified)
					throw new BadRequestException("Worker A does not meet requirements for Worker B's task");
			}
		}

		//my swap requests: cả sent và received
		public async Task<PaginatedResult<SwapRequestDto>> GetMySwapRequestsAsync(GetMySwapRequestsDto dto, SwapRequestStatus? status, PaginationRequest paginationRequest, CancellationToken ct = default)
		{
			var result = await _taskSwapRequestRepository.GetMySwapRequestsPaging(
			dto.WorkerId,
			dto.Perspective,
			dto.Status,
			paginationRequest,
			ct);

			return new PaginatedResult<SwapRequestDto>(
			result.PageNumber,
			result.PageSize,
			result.TotalElements,
			_mapper.Map<List<SwapRequestDto>>(result.Content));
		} 
	}
}