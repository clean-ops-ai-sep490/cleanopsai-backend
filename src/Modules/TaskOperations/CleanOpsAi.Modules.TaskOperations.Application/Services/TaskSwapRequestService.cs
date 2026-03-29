using AutoMapper;
using CleanOpsAi.BuildingBlocks.Application;
using CleanOpsAi.BuildingBlocks.Application.Common;
using CleanOpsAi.BuildingBlocks.Application.Exceptions;
using CleanOpsAi.BuildingBlocks.Application.Interfaces;
using CleanOpsAi.BuildingBlocks.Application.Pagination;
using CleanOpsAi.Modules.TaskOperations.Application.Common.Interfaces.Repositories;
using CleanOpsAi.Modules.TaskOperations.Application.Common.Interfaces.Services;
using CleanOpsAi.Modules.TaskOperations.Application.DTOs.Request;
using CleanOpsAi.Modules.TaskOperations.Application.DTOs.Response;
using CleanOpsAi.Modules.TaskOperations.Domain.Entities;
using CleanOpsAi.Modules.TaskOperations.Domain.Enums; 

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

		public TaskSwapRequestService(ITaskSwapRequestRepository taskSwapRequestRepository,
			ITaskAssignmentRepository taskAssignmentRepository,
			IMapper mapper, IDateTimeProvider dateTimeProvider, IIdGenerator idGenerator,
			IUserContext userContext)
		{
			_taskSwapRequestRepository = taskSwapRequestRepository;
			_taskAssignmentRepository = taskAssignmentRepository;
			_mapper = mapper;
			_dateTimeProvider = dateTimeProvider;
			_idGenerator = idGenerator;
			_userContext = userContext;
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

			var hasPending = await _taskSwapRequestRepository.HasPendingSwapAsync(dto.TaskAssignmentId);
			if (hasPending)
				throw new BadRequestException("Already has pending swap");

			var requesterHasConflict = await _taskAssignmentRepository.HasTimeConflictAsync(
			   excludeTaskId: dto.TaskAssignmentId,        // bỏ qua chính task đang swap
			   assigneeId: dto.RequesterId,
			   scheduledStartAt: targetTask.ScheduledStartAt,
			   scheduledEndAt: targetTask.ScheduledEndAt,
			   ct);
			if (requesterHasConflict)
				throw new BadRequestException("Requester has a conflicting task in that time slot");

			var targetHasConflict = await _taskAssignmentRepository.HasTimeConflictAsync(
				excludeTaskId: dto.TargetTaskAssignmentId,  // bỏ qua chính task đang swap
				assigneeId: dto.TargetWorkerId,
				scheduledStartAt: requesterTask.ScheduledStartAt,
				scheduledEndAt: requesterTask.ScheduledEndAt,
				ct);
			if (targetHasConflict)
				throw new BadRequestException("Target worker has a conflicting task in that time slot");

			var swapRequest = new TaskSwapRequest
			{
				Id = _idGenerator.Generate(),
				TaskAssignmentId = dto.TaskAssignmentId,
				TargetTaskAssignmentId = dto.TargetTaskAssignmentId,
				RequesterId = dto.RequesterId,
				TargetWorkerId = dto.TargetWorkerId,
				Status = SwapRequestStatus.PendingTargetApproval,
				RequesterNote = dto.RequesterNote,
				ExpiredAt = _dateTimeProvider.UtcNow.AddHours(12),
				Created = _dateTimeProvider.UtcNow,
				CreatedBy = _userContext.UserId.ToString()
			};

			await _taskSwapRequestRepository.InsertAsync(swapRequest, ct);
			await _taskSwapRequestRepository.SaveChangesAsync(ct);

			//notifi to Target

			return Result<SwapRequestDto>.Success(_mapper.Map<SwapRequestDto>(swapRequest));
		}

		public async Task<Result<PaginatedResult<SwapCandidateDto>>> GetSwapCandidatesAsync(GetSwapCandidatesDto dto, PaginationRequest paginationRequest, CancellationToken ct = default)
		{
			var requesterTask = await _taskAssignmentRepository.GetByIdAsync(dto.TaskAssignmentId, ct);
			if (requesterTask == null)
				throw new NotFoundException(nameof(TaskAssignment), dto.TaskAssignmentId);


			var today = _dateTimeProvider.UtcNow;
			var startOfWeek = today.AddDays(-(int)today.DayOfWeek + (int)DayOfWeek.Monday);
			var endOfWeek = startOfWeek.AddDays(7);

			var candidates = await _taskAssignmentRepository.GetSwapCandidatesAsync(
				workAreaId: requesterTask.WorkAreaId,
				excludeAssigneeId: requesterTask.AssigneeId,
				scheduledStartAt: requesterTask.ScheduledStartAt,
				scheduledEndAt: requesterTask.ScheduledEndAt,
				weekStart: startOfWeek,
				weekEnd: endOfWeek,
				date: dto.Date,
				preferredStartTime: dto.PreferredStartTime,
				paginationRequest: paginationRequest,
				ct: ct);

			return Result<PaginatedResult<SwapCandidateDto>>.Success(
				_mapper.Map<PaginatedResult<SwapCandidateDto>>(candidates)
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
				return Result.Success();
			}

			swapRequest.Status = SwapRequestStatus.PendingManagerApproval;
			await _taskSwapRequestRepository.SaveChangesAsync(ct);

			// Notify manager
			return Result.Success();
		}

		public async Task<Result> ReviewSwapRequestAsync(ReviewSwapRequestDto dto, CancellationToken ct = default)
		{
			Guid reviewerId = Guid.Parse("512f80aa-4204-48c8-b983-efb6b88efbaa");

			var swapRequest = await _taskSwapRequestRepository.GetByIdWithDetailsAsync(dto.TaskSwapRequestId, ct);
			if (swapRequest == null)
				throw new NotFoundException(nameof(TaskSwapRequest), dto.TaskSwapRequestId); 

			if (swapRequest.Status != SwapRequestStatus.PendingManagerApproval)
				throw new BadRequestException("The request is not in a pending state");

			swapRequest.ReviewedByUserId = reviewerId;
			swapRequest.ReviewNote = dto.ReviewNote;

			if (!dto.IsApproved)
			{
				swapRequest.Status = SwapRequestStatus.RejectedByManager;
				await _taskSwapRequestRepository.SaveChangesAsync();

				//notify faild to worker and target
				//await _notificationService.NotifySwapRejectedByManagerAsync(
				//	swapRequest.RequesterId, swapRequest.TargetWorkerId, swapRequest.Id);
				return Result.Success();
			}

			var requesterTask = swapRequest.TaskAssignment;
			var targetTask = swapRequest.TargetTaskAssignment;

			(requesterTask.AssigneeId, targetTask.AssigneeId) = (targetTask.AssigneeId, requesterTask.AssigneeId);
			swapRequest.Status = SwapRequestStatus.Approved;

			await _taskSwapRequestRepository.SaveChangesAsync();
			//notify faild to worker and target

			return Result.Success();
		}

		private void ValidateRequesterTask(TaskAssignment task, Guid requesterId)
		{
			if (task.AssigneeId != requesterId)
				throw new BadRequestException("Not your task");

			if (task.Status != TaskAssignmentStatus.NotStarted)
				throw new BadRequestException("Task was started or completed");

			if (task.ScheduledStartAt - DateTime.UtcNow < TimeSpan.FromHours(12))
				throw new BadRequestException("Task must be at least 12 hours away");
		}

		private void ValidateTargetTask(TaskAssignment task, Guid targetWorkerId)
		{
			if (task.AssigneeId != targetWorkerId)
				throw new BadRequestException("This task does not belong to this worker");

			if (task.Status != TaskAssignmentStatus.NotStarted)
				throw new BadRequestException("Target task already started/completed");

			if (task.ScheduledStartAt - DateTime.UtcNow < TimeSpan.FromHours(12))
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
			var result = await _taskSwapRequestRepository.GetSwapRequestsPaging(dto.Status, paginationRequest, ct);

			return new PaginatedResult<SwapRequestDto>(
				result.PageNumber,
				result.PageSize,
				result.TotalElements,
				_mapper.Map<List<SwapRequestDto>>(result.Content));
		}
	}
}
