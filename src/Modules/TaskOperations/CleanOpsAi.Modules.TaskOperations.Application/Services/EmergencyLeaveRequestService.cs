using AutoMapper;
using CleanOpsAi.BuildingBlocks.Application;
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
using System.Threading.Tasks;

namespace CleanOpsAi.Modules.TaskOperations.Application.Services
{
    public class EmergencyLeaveRequestService : IEmergencyLeaveRequestService
    {
        private readonly IEmergencyLeaveRequestRepository _emergencyLeaveRequestRepository;
        private readonly IFileStorageService _fileStorageService;
        private readonly IMapper _mapper;
        private readonly IUserContext _userContext;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly IWorkerQueryService _workerQueryService;
        private readonly ITaskAssignmentRepository _taskAssignmentRepository;
		private readonly INotificationPublisher _notificationPublisher;
        private readonly IIdGenerator _idGenerator;


        private const string ContainerName = "contracts";
        private const string AudioFolder = "audios";

        public EmergencyLeaveRequestService(
            IEmergencyLeaveRequestRepository emergencyLeaveRequestRepository,
            IFileStorageService fileStorageService,
            IMapper mapper,
            IUserContext userContext,
            IDateTimeProvider dateTimeProvider,
            IWorkerQueryService workerQueryService,
            ITaskAssignmentRepository taskAssignmentRepository,
            INotificationPublisher notificationPublisher,
            IIdGenerator idGenerator)
        {
            _emergencyLeaveRequestRepository = emergencyLeaveRequestRepository;
            _fileStorageService = fileStorageService;
            _mapper = mapper;
            _userContext = userContext;
            _dateTimeProvider = dateTimeProvider;
            _workerQueryService = workerQueryService;
            _taskAssignmentRepository = taskAssignmentRepository;
            _notificationPublisher = notificationPublisher;
            _idGenerator = idGenerator;
        }

        public async Task<EmergencyLeaveRequestDto?> GetById(Guid id, CancellationToken ct = default)
        {
            var entity = await _emergencyLeaveRequestRepository.GetByIdExistAsync(id, ct);
            if (entity == null) return null;

            var dto = _mapper.Map<EmergencyLeaveRequestDto>(entity);
            dto.WorkerName = await GetWorkerNameAsync(entity.WorkerId);
            if (entity.TaskAssignmentId.HasValue)
            {
                var task = await _taskAssignmentRepository.GetByIdAsync(entity.TaskAssignmentId.Value, ct);
                dto.TaskName = task?.TaskName;
            }
            return dto;
        }

        public async Task<PaginatedResult<EmergencyLeaveRequestDto>> Gets(PaginationRequest request, CancellationToken ct = default)
        {
            var result = await _emergencyLeaveRequestRepository.GetsPagingAsync(request, ct);

            var dtos = _mapper.Map<List<EmergencyLeaveRequestDto>>(result.Content);

            await EnrichWorkerNamesAsync(dtos);
            await EnrichTaskNamesAsync(dtos, ct);

            return new PaginatedResult<EmergencyLeaveRequestDto>(
                result.PageNumber,
                result.PageSize,
                result.TotalElements,
                dtos);
        }

        public async Task<PaginatedResult<EmergencyLeaveRequestDto>> GetsByWorkerId(Guid workerId, PaginationRequest request, CancellationToken ct = default)
        {
            var result = await _emergencyLeaveRequestRepository.GetsByWorkerIdPagingAsync(workerId, request, ct);

            var dtos = _mapper.Map<List<EmergencyLeaveRequestDto>>(result.Content);

            await EnrichWorkerNamesAsync(dtos);
            await EnrichTaskNamesAsync(dtos, ct);

            return new PaginatedResult<EmergencyLeaveRequestDto>(
                result.PageNumber,
                result.PageSize,
                result.TotalElements,
                dtos);
        }

        public async Task<PaginatedResult<EmergencyLeaveRequestDto>> GetsByTaskAssignmentId(Guid taskAssignmentId, PaginationRequest request, CancellationToken ct = default)
        {
            var result = await _emergencyLeaveRequestRepository.GetsByTaskAssignmentIdPagingAsync(taskAssignmentId, request, ct);

            var dtos = _mapper.Map<List<EmergencyLeaveRequestDto>>(result.Content);

            await EnrichWorkerNamesAsync(dtos);
            await EnrichTaskNamesAsync(dtos, ct);

            return new PaginatedResult<EmergencyLeaveRequestDto>(
                result.PageNumber,
                result.PageSize,
                result.TotalElements,
                dtos);
        }

        public async Task<PaginatedResult<EmergencyLeaveRequestDto>> GetsByStatus(RequestStatus status, PaginationRequest request, CancellationToken ct = default)
        {
            var result = await _emergencyLeaveRequestRepository.GetsByStatusPagingAsync(status, request, ct);

            var dtos = _mapper.Map<List<EmergencyLeaveRequestDto>>(result.Content);

            await EnrichWorkerNamesAsync(dtos);
            await EnrichTaskNamesAsync(dtos, ct);

            return new PaginatedResult<EmergencyLeaveRequestDto>(
                result.PageNumber,
                result.PageSize,
                result.TotalElements,
                dtos);
        }

        public async Task<PaginatedResult<EmergencyLeaveRequestDto>> GetsByDateRange(DateTime from, DateTime to, PaginationRequest request, CancellationToken ct = default)
        {
            var result = await _emergencyLeaveRequestRepository
                .GetsByDateRangePagingAsync(from, to, request, ct);

            var dtos = _mapper.Map<List<EmergencyLeaveRequestDto>>(result.Content);

            await EnrichWorkerNamesAsync(dtos);
            await EnrichTaskNamesAsync(dtos, ct);

            return new PaginatedResult<EmergencyLeaveRequestDto>(
                result.PageNumber,
                result.PageSize,
                result.TotalElements,
                dtos
            );
        }

        public async Task<EmergencyLeaveRequestDto?> Create(CreateEmergencyLeaveRequestDto dto, CancellationToken ct = default)
        {
            var entity = _mapper.Map<EmergencyLeaveRequest>(dto);
            entity.Id = _idGenerator.Generate();

            DateTime leaveDateFrom;
            DateTime leaveDateTo;

            if (dto.TaskAssignmentId.HasValue)
            {
                var task = await _taskAssignmentRepository.GetByIdAsync(dto.TaskAssignmentId.Value, ct);

                if (task == null)
                    throw new ArgumentException("TaskAssignment không tồn tại.");

                entity.TaskAssignmentId = task.Id;

                // LẤY DATE TỪ TASK
                leaveDateFrom = task.ScheduledStartAt;
                leaveDateTo = task.ScheduledEndAt;

                // BLOCK TASK
                if (task.Status != TaskAssignmentStatus.Completed &&
                    task.Status != TaskAssignmentStatus.Block)
                {
                    task.Status = TaskAssignmentStatus.Block;
                    task.LastModified = _dateTimeProvider.UtcNow;
                    task.LastModifiedBy = _userContext.UserId.ToString();

                    var updated = await _taskAssignmentRepository.UpdateAsync(task.Id, task, ct);
                    if (!updated)
                        throw new Exception("Failed to update TaskAssignment.");
                }
                else
                {
                    throw new BadRequestException("Task đã completed hoặc đã bị block.");
                }
            }
            else
            {
                if (!dto.LeaveDateFrom.HasValue || !dto.LeaveDateTo.HasValue)
                    throw new ArgumentException("Phải truyền LeaveDateFrom và LeaveDateTo.");

                leaveDateFrom = dto.LeaveDateFrom.Value;
                leaveDateTo = dto.LeaveDateTo.Value;

                if (leaveDateFrom > leaveDateTo)
                    throw new ArgumentException("LeaveDateFrom phải <= LeaveDateTo.");

                var totalDays = (leaveDateTo - leaveDateFrom).TotalDays + 1;

                if (totalDays > 7)
                    throw new BadRequestException("Thời gian nghỉ tối đa là 7 ngày.");

                // BLOCK ALL TASK TRONG RANGE
                var tasks = await _taskAssignmentRepository
                    .GetTasksByWorkerAndDateRange(dto.WorkerId, leaveDateFrom, leaveDateTo, ct);

                foreach (var task in tasks)
                {
                    if (task.Status != TaskAssignmentStatus.Completed &&
                        task.Status != TaskAssignmentStatus.Block)
                    {
                        task.Status = TaskAssignmentStatus.Block;
                        task.LastModified = _dateTimeProvider.UtcNow;
                        task.LastModifiedBy = _userContext.UserId.ToString();

                        await _taskAssignmentRepository.UpdateAsync(task.Id, task, ct);
                    }
                }
            }

            entity.LeaveDateFrom = leaveDateFrom;
            entity.LeaveDateTo = leaveDateTo;
            entity.Status = RequestStatus.Pending;
            entity.Created = _dateTimeProvider.UtcNow;
            entity.CreatedBy = _userContext.UserId.ToString();

            // AUDIO
            if (dto.AudioStream != null && !string.IsNullOrEmpty(dto.AudioFileName))
            {
                var fileName = $"audios/{dto.AudioFileName}";
                entity.AudioUrl = await _fileStorageService.UploadFileAsync(dto.AudioStream, fileName, "contracts");
            }

            if (!string.IsNullOrEmpty(dto.Transcription))
                entity.Transcription = dto.Transcription;

            await _emergencyLeaveRequestRepository.AddAsync(entity, ct); 

			var dtoResult = _mapper.Map<EmergencyLeaveRequestDto>(entity);
            dtoResult.WorkerName = await GetWorkerNameAsync(entity.WorkerId);

            if (entity.TaskAssignmentId.HasValue)
            {
                var t = await _taskAssignmentRepository.GetByIdAsync(entity.TaskAssignmentId.Value, ct);
                dtoResult.TaskName = t?.TaskName;
            }

            await _notificationPublisher.PublishAsync(new SendNotificationEvent
			{
				Title = "Yêu cầu nghỉ khẩn cấp mới",
				Body = $"{dtoResult.WorkerName ?? "Một nhân viên"} đã gửi yêu cầu nghỉ khẩn cấp.",
				Payload = JsonSerializer.Serialize(new
				{
					type = "EMERGENCY_LEAVE",
					action = "CREATED",
					requestId = entity.Id,
					workerId = entity.WorkerId,
					taskAssignmentId = entity.TaskAssignmentId,
					leaveDateFrom = entity.LeaveDateFrom,
					leaveDateTo = entity.LeaveDateTo
				}),
				SenderType = SenderTypeEnum.Worker,
				SenderId = _userContext.UserId,
				Recipients = new List<NotificationRecipientEvent>
				{
					new()
					{
						RecipientType = RecipientTypeEnum.Manager,
						RecipientId = null // broadcast cho manager
                    }
				}
			}, ct);

			return dtoResult;
        }

        public async Task<EmergencyLeaveRequestDto?> Update(Guid id, UpdateEmergencyLeaveRequestDto dto, CancellationToken ct = default)
        {
            var entity = await _emergencyLeaveRequestRepository.GetByIdExistAsync(id, ct);
            if (entity == null) return null;

            if (entity.Status == RequestStatus.Approved)
                throw new BadRequestException("Không thể update request đã được duyệt.");

            // ===== OLD RANGE =====
            var oldFrom = entity.LeaveDateFrom;
            var oldTo = entity.LeaveDateTo;

            // ===== NEW RANGE (partial update) =====
            var newFrom = dto.LeaveDateFrom ?? oldFrom;
            var newTo = dto.LeaveDateTo ?? oldTo;

            // ===== VALIDATE =====
            if (newFrom > newTo)
                throw new ArgumentException("LeaveDateFrom phải <= LeaveDateTo.");

            var totalDays = (newTo - newFrom).TotalDays + 1;
            if (totalDays > 7)
                throw new BadRequestException("Thời gian nghỉ tối đa là 7 ngày.");

            // ===== SYNC TASK =====
            var oldTasks = await _taskAssignmentRepository
                .GetTasksByWorkerAndDateRange(entity.WorkerId, oldFrom, oldTo, ct);

            var newTasks = await _taskAssignmentRepository
                .GetTasksByWorkerAndDateRange(entity.WorkerId, newFrom, newTo, ct);

            var oldTaskIds = oldTasks.Select(x => x.Id).ToHashSet();
            var newTaskIds = newTasks.Select(x => x.Id).ToHashSet();

            // UNBLOCK task không còn trong range mới
            foreach (var task in oldTasks.Where(x => !newTaskIds.Contains(x.Id)))
            {
                if (task.Status == TaskAssignmentStatus.Block)
                {
                    task.Status = TaskAssignmentStatus.InProgress;
                    task.LastModified = _dateTimeProvider.UtcNow;
                    task.LastModifiedBy = _userContext.UserId.ToString();

                    await _taskAssignmentRepository.UpdateAsync(task.Id, task, ct);
                }
            }

            // BLOCK task mới thêm vào range
            foreach (var task in newTasks.Where(x => !oldTaskIds.Contains(x.Id)))
            {
                if (task.Status != TaskAssignmentStatus.Completed &&
                    task.Status != TaskAssignmentStatus.Block)
                {
                    task.Status = TaskAssignmentStatus.Block;
                    task.LastModified = _dateTimeProvider.UtcNow;
                    task.LastModifiedBy = _userContext.UserId.ToString();

                    await _taskAssignmentRepository.UpdateAsync(task.Id, task, ct);
                }
            }

            // UPDATE ENTITY
            entity.LeaveDateFrom = newFrom;
            entity.LeaveDateTo = newTo;

            if (dto.AudioStream != null && !string.IsNullOrEmpty(dto.AudioFileName))
            {
                var fileName = $"{AudioFolder}/{dto.AudioFileName}";
                entity.AudioUrl = await _fileStorageService.UploadFileAsync(dto.AudioStream, fileName, ContainerName);
            }

            if (!string.IsNullOrEmpty(dto.Transcription))
                entity.Transcription = dto.Transcription;

            entity.LastModified = _dateTimeProvider.UtcNow;
            entity.LastModifiedBy = _userContext.UserId.ToString();

            await _emergencyLeaveRequestRepository.UpdateAsync(entity, ct);

            // ===== PUBLISH EVENT (đặt tại đây) =====
            // publish event sau khi tich hop RabbitMQ
            // 
            // await _publishEndpoint.Publish(new EmergencyLeaveRequestUpdatedEvent
            // {
            //     RequestId     = entity.Id,
            //     WorkerId      = entity.WorkerId,
            //     LeaveDateFrom = entity.LeaveDateFrom,
            //     LeaveDateTo   = entity.LeaveDateTo,
            //     Reason        = entity.Reason,
            //     AudioUrl      = entity.AudioUrl,
            //     Transcription = entity.Transcription,
            //     UpdatedAt     = entity.LastModified!.Value
            // }, ct);

            // MAP DTO 
            var dtoResult = _mapper.Map<EmergencyLeaveRequestDto>(entity);
            dtoResult.WorkerName = await GetWorkerNameAsync(entity.WorkerId);

            if (entity.TaskAssignmentId.HasValue)
            {
                var task = await _taskAssignmentRepository.GetByIdAsync(entity.TaskAssignmentId.Value, ct);
                dtoResult.TaskName = task?.TaskName;
            }

            return dtoResult;
        }

        public async Task<EmergencyLeaveRequestDto?> Review(Guid id, ReviewEmergencyLeaveRequestDto dto, CancellationToken ct = default)
        {
            var entity = await _emergencyLeaveRequestRepository.GetByIdExistAsync(id, ct);
            if (entity == null) return null;

            entity.Status = dto.Status;
            entity.ReviewedByUserId = _userContext.UserId;
            entity.ApprovedAt = dto.Status == RequestStatus.Approved
                ? _dateTimeProvider.UtcNow
                : null;

            var reviewByUserName = _userContext.FullName;

            // Lấy đúng task
            List<TaskAssignment> tasks;
            if (entity.TaskAssignmentId.HasValue)
            {
                var task = await _taskAssignmentRepository.GetByIdAsync(entity.TaskAssignmentId.Value, ct);
                tasks = task != null ? new List<TaskAssignment> { task } : new List<TaskAssignment>();
            }
            else
            {
                tasks = await _taskAssignmentRepository
                    .GetTasksByWorkerAndDateRange(entity.WorkerId, entity.LeaveDateFrom, entity.LeaveDateTo, ct);
            }

            // APPROVED
            if (dto.Status == RequestStatus.Approved)
            {
                foreach (var task in tasks)
                {
                    if (task.Status == TaskAssignmentStatus.NotStarted ||
                        task.Status == TaskAssignmentStatus.InProgress)
                    {
                        task.Status = TaskAssignmentStatus.Block;
                        task.LastModified = _dateTimeProvider.UtcNow;
                        task.LastModifiedBy = _userContext.UserId.ToString();

                        await _taskAssignmentRepository.UpdateAsync(task.Id, task, ct);
                    }
                }
            }

            // REJECTED
            if (dto.Status == RequestStatus.Rejected)
            {
                foreach (var task in tasks)
                {
                    if (task.Status == TaskAssignmentStatus.Block)
                    {
                        task.Status = TaskAssignmentStatus.InProgress;
                        task.LastModified = _dateTimeProvider.UtcNow;
                        task.LastModifiedBy = _userContext.UserId.ToString();

                        await _taskAssignmentRepository.UpdateAsync(task.Id, task, ct);
                    }
                }
            }

            // đảm bảo persist task changes
            await _taskAssignmentRepository.SaveChangesAsync(ct);

            // persist request
            await _emergencyLeaveRequestRepository.UpdateAsync(entity, ct);

            // ================= PUBLISH EVENT =================
            //var message = new EmergencyLeaveRequestReviewedEvent
            //{
            //    RequestId = entity.Id,
            //    WorkerId = entity.WorkerId,
            //    TaskAssignmentId = entity.TaskAssignmentId,
            //    Status = entity.Status,
            //    ReviewedByUserId = entity.ReviewedByUserId,
            //    ApprovedAt = entity.ApprovedAt,
            //    ReviewedAt = entity.LastModified
            //};

            //var routingKey = entity.Status == RequestStatus.Approved
            //    ? "emergency-leave-request.approved"
            //    : "emergency-leave-request.rejected";

            //await _publishEndpoint.Publish(message, context =>
            //{
            //    context.SetRoutingKey(routingKey);
            //}, ct);
            // =================================================

            var dtoResult = _mapper.Map<EmergencyLeaveRequestDto>(entity);
            dtoResult.WorkerName = await GetWorkerNameAsync(entity.WorkerId);
            dtoResult.ReviewedByUserName = reviewByUserName;
            if (entity.TaskAssignmentId.HasValue)
            {
                var task = await _taskAssignmentRepository.GetByIdAsync(entity.TaskAssignmentId.Value, ct);
                dtoResult.TaskName = task?.TaskName;
            }

            return dtoResult;
        }

        public async Task<bool> Delete(Guid id, CancellationToken ct = default)
        {
            var entity = await _emergencyLeaveRequestRepository.GetByIdExistAsync(id, ct);
            if (entity == null) return false;

            await _emergencyLeaveRequestRepository.DeleteAsync(entity, ct);
            return true;
        }

        public async Task<PaginatedResult<EmergencyLeaveRequestDto>> GetsByWorkerCurrentMonth(
            Guid workerId,
            PaginationRequest request,
            CancellationToken ct = default)
        {
            var now = _dateTimeProvider.UtcNow;

            var result = await _emergencyLeaveRequestRepository
                .GetsByWorkerCurrentMonthPagingAsync(workerId, now, request, ct);

            var dtos = _mapper.Map<List<EmergencyLeaveRequestDto>>(result.Content);

            await EnrichWorkerNamesAsync(dtos);
            await EnrichTaskNamesAsync(dtos, ct);

            return new PaginatedResult<EmergencyLeaveRequestDto>(
                result.PageNumber,
                result.PageSize,
                result.TotalElements,
                dtos);
        }

        private async Task<string?> GetWorkerNameAsync(Guid workerId)
        {
            var dict = await _workerQueryService.GetUserNames(new List<Guid> { workerId });
            return dict.GetValueOrDefault(workerId);
        }

        private async Task EnrichWorkerNamesAsync(List<EmergencyLeaveRequestDto> dtos)
        {
            var workerIds = dtos.Select(x => x.WorkerId).Distinct().ToList();
            if (!workerIds.Any()) return;

            var dict = await _workerQueryService.GetUserNames(workerIds);

            foreach (var dto in dtos)
            {
                dto.WorkerName = dict.GetValueOrDefault(dto.WorkerId);
            }
        }
        private async Task EnrichTaskNamesAsync(List<EmergencyLeaveRequestDto> dtos, CancellationToken ct)
        {
            var taskIds = dtos
                .Where(x => x.TaskAssignmentId.HasValue)
                .Select(x => x.TaskAssignmentId!.Value)
                .Distinct()
                .ToList();

            if (!taskIds.Any()) return;

            var tasks = await _taskAssignmentRepository.GetByIdsAsync(taskIds, ct);

            var dict = tasks.ToDictionary(x => x.Id, x => x.TaskName);

            foreach (var dto in dtos)
            {
                if (dto.TaskAssignmentId.HasValue)
                {
                    dto.TaskName = dict.GetValueOrDefault(dto.TaskAssignmentId.Value);
                }
            }
        }
    }
}