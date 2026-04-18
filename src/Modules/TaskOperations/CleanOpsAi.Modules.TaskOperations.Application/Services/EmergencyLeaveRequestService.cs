using AutoMapper;
using CleanOpsAi.BuildingBlocks.Application;
using CleanOpsAi.BuildingBlocks.Application.Interfaces;
using CleanOpsAi.BuildingBlocks.Application.Pagination;
using CleanOpsAi.Modules.TaskOperations.Application.Common.Interfaces.Repositories;
using CleanOpsAi.Modules.TaskOperations.Application.Common.Interfaces.Services;
using CleanOpsAi.Modules.TaskOperations.Application.DTOs.Request;
using CleanOpsAi.Modules.TaskOperations.Application.DTOs.Response;
using CleanOpsAi.Modules.TaskOperations.Domain.Entities;
using CleanOpsAi.Modules.TaskOperations.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
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

        private const string ContainerName = "contracts";
        private const string AudioFolder = "audios";

        public EmergencyLeaveRequestService(
            IEmergencyLeaveRequestRepository emergencyLeaveRequestRepository,
            IFileStorageService fileStorageService,
            IMapper mapper,
            IUserContext userContext,
            IDateTimeProvider dateTimeProvider,
            IWorkerQueryService workerQueryService)
        {
            _emergencyLeaveRequestRepository = emergencyLeaveRequestRepository;
            _fileStorageService = fileStorageService;
            _mapper = mapper;
            _userContext = userContext;
            _dateTimeProvider = dateTimeProvider;
            _workerQueryService = workerQueryService;
        }

        public async Task<EmergencyLeaveRequestDto?> GetById(Guid id, CancellationToken ct = default)
        {
            var entity = await _emergencyLeaveRequestRepository.GetByIdExistAsync(id, ct);
            if (entity == null) return null;

            var dto = _mapper.Map<EmergencyLeaveRequestDto>(entity);
            dto.WorkerName = await GetWorkerNameAsync(entity.WorkerId);

            return dto;
        }

        public async Task<PaginatedResult<EmergencyLeaveRequestDto>> Gets(PaginationRequest request, CancellationToken ct = default)
        {
            var result = await _emergencyLeaveRequestRepository.GetsPagingAsync(request, ct);

            var dtos = _mapper.Map<List<EmergencyLeaveRequestDto>>(result.Content);

            await EnrichWorkerNamesAsync(dtos);

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

            DateTime leaveDateFrom = default;
            DateTime leaveDateTo = default;

            if (dto.TaskAssignmentId.HasValue)
            {
                entity.TaskAssignmentId = dto.TaskAssignmentId.Value;
            }
            else
            {
                if (!dto.LeaveDateFrom.HasValue || !dto.LeaveDateTo.HasValue)
                    throw new ArgumentException("Phai truyen LeaveDateFrom va LeaveDateTo khi khong co TaskAssignmentId.");

                leaveDateFrom = dto.LeaveDateFrom.Value;
                leaveDateTo = dto.LeaveDateTo.Value;
            }

            if (leaveDateFrom > leaveDateTo)
                throw new ArgumentException("LeaveDateFrom phai nho hon hoac bang LeaveDateTo.");

            entity.LeaveDateFrom = leaveDateFrom;
            entity.LeaveDateTo = leaveDateTo;
            entity.Status = RequestStatus.Pending;
            entity.Created = _dateTimeProvider.UtcNow;
            entity.CreatedBy = _userContext.UserId.ToString();

            if (dto.AudioStream != null && !string.IsNullOrEmpty(dto.AudioFileName))
            {
                var fileName = $"{AudioFolder}/{dto.AudioFileName}";
                entity.AudioUrl = await _fileStorageService.UploadFileAsync(dto.AudioStream, fileName, ContainerName);
            }

            if (!string.IsNullOrEmpty(dto.Transcription))
                entity.Transcription = dto.Transcription;

            await _emergencyLeaveRequestRepository.AddAsync(entity, ct);

            // publish event sau khi tich hop RabbitMQ
            // 
            // await _publishEndpoint.Publish(new EmergencyLeaveRequestCreatedEvent
            // {
            //     RequestId        = entity.Id,
            //     WorkerId         = entity.WorkerId,
            //     TaskAssignmentId = entity.TaskAssignmentId,
            //     LeaveDateFrom    = entity.LeaveDateFrom,
            //     LeaveDateTo      = entity.LeaveDateTo,
            //     AudioUrl         = entity.AudioUrl,
            //     Transcription    = entity.Transcription,
            //     CreatedAt        = entity.Created
            // }, ct);

            var dtoResult = _mapper.Map<EmergencyLeaveRequestDto>(entity);
            dtoResult.WorkerName = await GetWorkerNameAsync(entity.WorkerId);

            return dtoResult;
        }

        public async Task<EmergencyLeaveRequestDto?> Update(Guid id, UpdateEmergencyLeaveRequestDto dto, CancellationToken ct = default)
        {
            var entity = await _emergencyLeaveRequestRepository.GetByIdExistAsync(id, ct);
            if (entity == null) return null;

            var newFrom = dto.LeaveDateFrom ?? entity.LeaveDateFrom;
            var newTo = dto.LeaveDateTo ?? entity.LeaveDateTo;

            if (newFrom > newTo)
                throw new ArgumentException("LeaveDateFrom phai nho hon hoac bang LeaveDateTo.");

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

            var dtoResult = _mapper.Map<EmergencyLeaveRequestDto>(entity);
            dtoResult.WorkerName = await GetWorkerNameAsync(entity.WorkerId);

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

            await _emergencyLeaveRequestRepository.UpdateAsync(entity, ct);

            // notifi to Target - gui mail thong bao ket qua review cho worker
            // var message = new EmergencyLeaveRequestReviewedEvent
            // {
            //     RequestId        = entity.Id,
            //     WorkerId         = entity.WorkerId,
            //     TaskAssignmentId = entity.TaskAssignmentId,
            //     Status           = entity.Status,           // Approved | Rejected
            //     ReviewedByUserId = entity.ReviewedByUserId,
            //     ApprovedAt       = entity.ApprovedAt,
            //     ReviewedAt       = entity.LastModified
            // };
            // string routingKey = entity.Status == RequestStatus.Approved
            //     ? "emergency-leave-request.approved"
            //     : "emergency-leave-request.rejected";

            var dtoResult = _mapper.Map<EmergencyLeaveRequestDto>(entity);
            dtoResult.WorkerName = await GetWorkerNameAsync(entity.WorkerId);
            dtoResult.ReviewedByUserName = reviewByUserName;

            return dtoResult;
        }

        public async Task<bool> Delete(Guid id, CancellationToken ct = default)
        {
            var entity = await _emergencyLeaveRequestRepository.GetByIdExistAsync(id, ct);
            if (entity == null) return false;

            await _emergencyLeaveRequestRepository.DeleteAsync(entity, ct);
            return true;
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
    }
}