using AutoMapper;
using CleanOpsAi.BuildingBlocks.Application;
using CleanOpsAi.BuildingBlocks.Application.Exceptions;
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
using System.Text;
using System.Threading.Tasks;

namespace CleanOpsAi.Modules.TaskOperations.Application.Services
{
    public class AdHocRequestService : IAdHocRequestService
    {
        private readonly IAdHocRequestRepository _repository;
        private readonly IMapper _mapper;
        private readonly IUserContext _userContext;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly IWorkerQueryService _workerQueryService;
        private readonly ISupervisorQueryService _supervisorQueryService;
        private readonly IWorkAreaQueryService _workAreaQueryService;

        public AdHocRequestService(
            IAdHocRequestRepository repository,
            IMapper mapper,
            IUserContext userContext,
            IDateTimeProvider dateTimeProvider,
            IWorkerQueryService workerQueryService,
            ISupervisorQueryService supervisorQueryService,
            IWorkAreaQueryService workAreaQueryService
        )
        {
            _repository = repository;
            _mapper = mapper;
            _userContext = userContext;
            _dateTimeProvider = dateTimeProvider;
            _workerQueryService = workerQueryService;
            _supervisorQueryService = supervisorQueryService;
            _workAreaQueryService = workAreaQueryService;
        }

        public async Task<AdHocRequestDto?> GetById(Guid id, CancellationToken ct = default)
        {
            var entity = await _repository.GetByIdExistAsync(id, ct);
            if (entity == null) return null;

            var dto = _mapper.Map<AdHocRequestDto>(entity);
            dto.WorkerName = await GetWorkerNameAsync(entity.RequestedByWorkerId);

            return dto;
        }

        public async Task<PaginatedResult<AdHocRequestDto>> Gets(PaginationRequest request, CancellationToken ct = default)
        {
            var result = await _repository.GetsPagingAsync(request, ct);

            var dtos = _mapper.Map<List<AdHocRequestDto>>(result.Content);

            await EnrichWorkerNamesAsync(dtos);

            return new PaginatedResult<AdHocRequestDto>(
                result.PageNumber,
                result.PageSize,
                result.TotalElements,
                dtos);
        }

        public async Task<PaginatedResult<AdHocRequestDto>> GetsByWorkerId(Guid workerId, PaginationRequest request, CancellationToken ct = default)
        {
            var result = await _repository.GetsByWorkerIdPagingAsync(workerId, request, ct);

            var dtos = _mapper.Map<List<AdHocRequestDto>>(result.Content);

            await EnrichWorkerNamesAsync(dtos);

            return new PaginatedResult<AdHocRequestDto>(
                result.PageNumber,
                result.PageSize,
                result.TotalElements,
                dtos);
        }

        public async Task<PaginatedResult<AdHocRequestDto>> GetsByTaskAssignmentId(Guid taskAssignmentId, PaginationRequest request, CancellationToken ct = default)
        {
            var result = await _repository.GetsByTaskAssignmentIdPagingAsync(taskAssignmentId, request, ct);

            var dtos = _mapper.Map<List<AdHocRequestDto>>(result.Content);

            await EnrichWorkerNamesAsync(dtos);

            return new PaginatedResult<AdHocRequestDto>(
                result.PageNumber,
                result.PageSize,
                result.TotalElements,
                dtos);
        }

        public async Task<PaginatedResult<AdHocRequestDto>> GetsByStatus(AdHocRequestStatus status, PaginationRequest request, CancellationToken ct = default)
        {
            var result = await _repository.GetsByStatusPagingAsync(status, request, ct);

            var dtos = _mapper.Map<List<AdHocRequestDto>>(result.Content);

            await EnrichWorkerNamesAsync(dtos);

            return new PaginatedResult<AdHocRequestDto>(
                result.PageNumber,
                result.PageSize,
                result.TotalElements,
                dtos);
        }

        public async Task<PaginatedResult<AdHocRequestDto>> GetsByType(AdHocRequestType type, PaginationRequest request, CancellationToken ct = default)
        {
            var result = await _repository.GetsByTypePagingAsync(type, request, ct);

            var dtos = _mapper.Map<List<AdHocRequestDto>>(result.Content);

            await EnrichWorkerNamesAsync(dtos);

            return new PaginatedResult<AdHocRequestDto>(
                result.PageNumber,
                result.PageSize,
                result.TotalElements,
                dtos);
        }

        public async Task<AdHocRequestDto?> Create(CreateAdHocRequestDto dto, CancellationToken ct = default)
        {
            // 1. Resolve WorkerId từ UserId hiện tại
            var workerId = await _workerQueryService.GetWorkerIdByUserIdAsync(_userContext.UserId, ct);
            if (workerId == null)
                throw new BadRequestException("Worker profile not found for current user.");

            // 2. Validate dates
            if (dto.RequestDateFrom == null)
                throw new BadRequestException("RequestDateFrom is required.");

            var from = dto.RequestDateFrom;
            var to = dto.RequestDateTo ?? from;

            if (from > to)
                throw new BadRequestException("RequestDateFrom must be <= RequestDateTo.");

            // 3. Map + gán fields
            var entity = _mapper.Map<AdHocRequest>(dto);
            entity.RequestedByWorkerId = workerId.Value;
            entity.RequestDateFrom = from;
            entity.RequestDateTo = to;
            entity.Status = AdHocRequestStatus.Pending;
            entity.Created = _dateTimeProvider.UtcNow;
            entity.CreatedBy = _userContext.UserId.ToString();

            // 4. Resolve ReviewerId (Supervisor) từ WorkAreaId + WorkerId
            var supervisorId = await _supervisorQueryService.GetSupervisorIdAsync(
                dto.WorkAreaId, workerId.Value, ct);
            entity.ReviewedByUserId = supervisorId;

            Console.WriteLine($"DEBUG - WorkAreaId: {dto.WorkAreaId}");
            Console.WriteLine($"DEBUG - WorkerId: {workerId.Value}");
            Console.WriteLine($"DEBUG - SupervisorId resolved: {supervisorId}");

            await _repository.AddAsync(entity, ct);

            // //  PUSH NOTIFICATION (RabbitMQ)
            // await _publishEndpoint.Publish(new AdHocRequestCreatedEvent
            // {
            //     RequestId = entity.Id,
            //     TaskAssignmentId = entity.TaskAssignmentId,
            //     RequestedByWorkerId = entity.RequestedByWorkerId,
            //     RequestType = entity.RequestType,
            //     Reason = entity.Reason,
            //     Description = entity.Description,
            //     CreatedAt = entity.Created
            // }, ct);

            var result = _mapper.Map<AdHocRequestDto>(entity);
            result.WorkerName = await GetWorkerNameAsync(entity.RequestedByWorkerId);
            result.WorkAreaName = await _workAreaQueryService.GetWorkAreaNameAsync(dto.WorkAreaId, ct); 
            result.WorkAreaId = dto.WorkAreaId; 
            if (entity.ReviewedByUserId.HasValue)
                result.ReviewedByUserName = await _supervisorQueryService.GetSupervisorNameAsync(
                    entity.ReviewedByUserId.Value, ct);

            return result;
        }

        public async Task<AdHocRequestDto?> Update(Guid id, UpdateAdHocRequestDto dto, CancellationToken ct = default)
        {
            var entity = await _repository.GetByIdExistAsync(id, ct);
            if (entity == null) return null;

            if (entity.Status == AdHocRequestStatus.Approved)
                throw new BadRequestException("Cannot update approved request.");

            if (dto.RequestType.HasValue)
                entity.RequestType = dto.RequestType.Value;

            if (!string.IsNullOrEmpty(dto.Reason))
                entity.Reason = dto.Reason;

            if (!string.IsNullOrEmpty(dto.Description))
                entity.Description = dto.Description;

            var newFrom = (dto.RequestDateFrom.HasValue && dto.RequestDateFrom > DateTime.MinValue)
                ? dto.RequestDateFrom
                : entity.RequestDateFrom;

            var newTo = (dto.RequestDateTo.HasValue && dto.RequestDateTo > DateTime.MinValue)
                ? dto.RequestDateTo
                : entity.RequestDateTo;

            if (newFrom.HasValue && newTo.HasValue && newFrom > newTo)
                throw new BadRequestException("RequestDateFrom must be <= RequestDateTo.");

            entity.RequestDateFrom = newFrom;
            entity.RequestDateTo = newTo;

            if (dto.WorkAreaId.HasValue && dto.WorkAreaId != Guid.Empty)
            {
                var supervisorId = await _supervisorQueryService.GetSupervisorIdAsync(
                    dto.WorkAreaId.Value,
                    entity.RequestedByWorkerId,
                    ct);
                entity.ReviewedByUserId = supervisorId;
            }

            entity.LastModified = _dateTimeProvider.UtcNow;
            entity.LastModifiedBy = _userContext.UserId.ToString();

            await _repository.UpdateAsync(entity, ct);

            // //  PUSH NOTIFICATION (RabbitMQ)
            // await _publishEndpoint.Publish(new AdHocRequestCreatedEvent
            // {
            //     RequestId = entity.Id,
            //     TaskAssignmentId = entity.TaskAssignmentId,
            //     RequestedByWorkerId = entity.RequestedByWorkerId,
            //     RequestType = entity.RequestType,
            //     Reason = entity.Reason,
            //     Description = entity.Description,
            //     CreatedAt = entity.Created
            // }, ct);

            var result = _mapper.Map<AdHocRequestDto>(entity);
            result.WorkerName = await GetWorkerNameAsync(entity.RequestedByWorkerId);

            // ← thêm: enrich WorkAreaName + SupervisorName
            if (dto.WorkAreaId.HasValue && dto.WorkAreaId != Guid.Empty)
                result.WorkAreaName = await _workAreaQueryService.GetWorkAreaNameAsync(
                    dto.WorkAreaId.Value, ct);

            if (entity.ReviewedByUserId.HasValue)
                result.ReviewedByUserName = await _supervisorQueryService.GetSupervisorNameAsync(
                    entity.ReviewedByUserId.Value, ct);

            return result;
        }

        public async Task<AdHocRequestDto?> Review(Guid id, ReviewAdHocRequestDto dto, CancellationToken ct = default)
        {
            var entity = await _repository.GetByIdExistAsync(id, ct);
            if (entity == null) return null;

            entity.Status = dto.Status;
            entity.ApprovedAt = dto.Status == AdHocRequestStatus.Approved
                ? _dateTimeProvider.UtcNow
                : null;


            await _repository.UpdateAsync(entity, ct);

            // PUSH NOTIFICATION (QUAN TRỌNG NHẤT)
            // await _publishEndpoint.Publish(new AdHocRequestReviewedEvent
            // {
            //     RequestId = entity.Id,
            //     RequestedByWorkerId = entity.RequestedByWorkerId,
            //     Status = entity.Status, // Approved | Rejected
            //     ReviewedByUserId = entity.ReviewedByUserId,
            //     ApprovedAt = entity.ApprovedAt,
            //     ReviewedAt = entity.LastModified ?? _dateTimeProvider.UtcNow
            // }, ct);

            var result = _mapper.Map<AdHocRequestDto>(entity);
            result.WorkerName = await GetWorkerNameAsync(entity.RequestedByWorkerId);

            return result;
        }

        public async Task<bool> Delete(Guid id, CancellationToken ct = default)
        {
            var entity = await _repository.GetByIdExistAsync(id, ct);
            if (entity == null) return false;

            await _repository.DeleteAsync(entity, ct);

            return true;
        }

        private async Task<string?> GetWorkerNameAsync(Guid workerId)
        {
            var dict = await _workerQueryService.GetUserNames(new List<Guid> { workerId });
            return dict.GetValueOrDefault(workerId);
        }

        private async Task EnrichWorkerNamesAsync(List<AdHocRequestDto> dtos)
        {
            var workerIds = dtos.Select(x => x.RequestedByWorkerId).Distinct().ToList();
            if (!workerIds.Any()) return;

            var dict = await _workerQueryService.GetUserNames(workerIds);

            foreach (var dto in dtos)
            {
                dto.WorkerName = dict.GetValueOrDefault(dto.RequestedByWorkerId);
            }
        }
    }
}
