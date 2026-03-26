using AutoMapper;
using CleanOpsAi.BuildingBlocks.Application.Common;
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

		public TaskSwapRequestService(ITaskSwapRequestRepository taskSwapRequestRepository,
			ITaskAssignmentRepository taskAssignmentRepository,
			IMapper mapper, IDateTimeProvider dateTimeProvider, IIdGenerator idGenerator)
		{
			_taskSwapRequestRepository = taskSwapRequestRepository;
			_taskAssignmentRepository = taskAssignmentRepository;
			_mapper = mapper;
			_dateTimeProvider = dateTimeProvider;
			_idGenerator = idGenerator;
		}

		public async Task<Result<SwapRequestDto?>> GetById(Guid id, CancellationToken ct = default)
		{
			var swapRequest = await _taskSwapRequestRepository.GetByIdWithDetailsAsync(id, ct);
			if (swapRequest == null)
				return Result<SwapRequestDto?>.Success(null);

			return Result<SwapRequestDto?>.Success(_mapper.Map<SwapRequestDto>(swapRequest));
		}

		public async Task<Result> CancelSwapRequestAsync(Guid swapRequestId, Guid requesterId, CancellationToken ct = default)
		{
			var swapRequest = await _taskSwapRequestRepository.GetByIdWithDetailsAsync(swapRequestId, ct);
			if (swapRequest == null)
				return Result.Failure("No swap requests found");

			swapRequest.Status = SwapRequestStatus.CancelledByRequester;
			await _taskSwapRequestRepository.SaveChangesAsync(ct);

			return Result.Success();
		}

		public async Task<Result<SwapRequestDto>> CreateSwapRequestAsync(TaskSwapRequestCreateDto dto, Guid userId, CancellationToken ct = default)
		{ 

			var requesterTask = await _taskAssignmentRepository.GetByIdAsync(dto.TaskAssignmentId, ct);
			if (requesterTask == null)
				return Result<SwapRequestDto>.Failure("TaskAssignment not exists");

			var requesterValidation = ValidateRequesterTask(requesterTask, dto.RequesterId);
			if (!requesterValidation.Succeeded)
				return Result<SwapRequestDto>.Failure(requesterValidation.Errors);

			var targetTask = await _taskAssignmentRepository.GetByIdAsync(dto.TargetTaskAssignmentId, ct);
			if (targetTask == null)
				return Result<SwapRequestDto>.Failure("Target task not exists");


			var targetValidation = ValidateTargetTask(targetTask, dto.TargetWorkerId);
			if (!targetValidation.Succeeded)
				return Result<SwapRequestDto>.Failure(targetValidation.Errors);

			var swapValidation = ValidateSwapRules(requesterTask, targetTask);
			if (!swapValidation.Succeeded)
				return Result<SwapRequestDto>.Failure(swapValidation.Errors);

			var hasPending = await _taskSwapRequestRepository.HasPendingSwapAsync(dto.TaskAssignmentId);
			if (hasPending)
				return Result<SwapRequestDto>.Failure("Already has pending swap");

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
				CreatedBy = userId.ToString()
			};

			await _taskSwapRequestRepository.InsertAsync(swapRequest, ct);
			await _taskSwapRequestRepository.SaveChangesAsync(ct);

			//notifi to Target

			return Result<SwapRequestDto>.Success(_mapper.Map<SwapRequestDto>(swapRequest));
		}

		public Task<Result<PaginatedResult<SwapCandidateDto>>> GetSwapCandidatesAsync(GetSwapCandidatesDto dto, PaginationRequest paginationRequest, CancellationToken ct = default)
		{
			throw new NotImplementedException();
		}

		public async Task<Result> RespondSwapRequestAsync(RespondSwapRequestDto dto, CancellationToken ct = default)
		{
			var swapRequest = await _taskSwapRequestRepository.GetByIdWithDetailsAsync(dto.SwapRequestId);
			if (swapRequest == null)
				return Result.Failure("No swap requests found");

			if (swapRequest.TargetWorkerId != dto.ResponderId)
				return Result.Failure("You are not the one who was asked to swap");

			if(swapRequest.Status != SwapRequestStatus.PendingTargetApproval)
				return Result.Failure("The request is not in a pending confirmation state");

			if (DateTime.UtcNow > swapRequest.ExpiredAt)
			{
				swapRequest.Status = SwapRequestStatus.Expired;
				await _taskSwapRequestRepository.SaveChangesAsync(ct);
				return Result.Failure("The request has expired.");
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
				return Result.Failure("No swap requests found");

			if (swapRequest.Status != SwapRequestStatus.PendingManagerApproval)
				return Result.Failure("The request is not in a pending state");

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

		private Result ValidateRequesterTask(TaskAssignment task, Guid requesterId)
		{
			if (task.AssigneeId != requesterId)
				return Result.Failure("Not your task");

			if (task.Status != TaskAssignmentStatus.NotStarted)
				return Result.Failure("Task was started or completed");

			if (task.ScheduledStartAt - DateTime.UtcNow < TimeSpan.FromHours(12))
				return Result.Failure("Task must be at least 12 hours away");

			return Result.Success();
		}

		private Result ValidateTargetTask(TaskAssignment task, Guid targetWorkerId)
		{
			if (task.AssigneeId != targetWorkerId)
				return Result.Failure("This task does not belong to this worker");

			if (task.Status != TaskAssignmentStatus.NotStarted)
				return Result.Failure("Target task already started/completed");

			if (task.ScheduledStartAt - DateTime.UtcNow < TimeSpan.FromHours(12))
				return Result.Failure("Target task too close");

			return Result.Success();
		}

		private Result ValidateSwapRules(TaskAssignment a, TaskAssignment b)
		{
			if (a.WorkAreaId != b.WorkAreaId)
				return Result.Failure("Different work areas");

			var daysDiff = Math.Abs((a.ScheduledStartAt.Date - b.ScheduledStartAt.Date).Days);
			if (daysDiff > 7)
				return Result.Failure("Tasks must be within same week");

			return Result.Success();
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
