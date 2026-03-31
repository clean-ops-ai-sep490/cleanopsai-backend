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
using System.Text;
using System.Threading.Tasks;

namespace CleanOpsAi.Modules.TaskOperations.Application.Services
{
    public class EquipmentRequestService : IEquipmentRequestService
    {
        private readonly IEquipmentRequestRepository _equipmentRequestRepository;
        private readonly IMapper _mapper;
        private readonly IUserContext _userContext;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly IWorkerQueryService _workerQueryService;

        public EquipmentRequestService(
            IEquipmentRequestRepository equipmentRequestRepository,
            IMapper mapper,
            IUserContext userContext,
            IDateTimeProvider dateTimeProvider,
            IWorkerQueryService workerQueryService)
        {
            _equipmentRequestRepository = equipmentRequestRepository;
            _mapper = mapper;
            _userContext = userContext;
            _dateTimeProvider = dateTimeProvider;
            _workerQueryService = workerQueryService;
        }

        public async Task<EquipmentRequestDto?> GetById(Guid id, CancellationToken ct = default)
        {
            var entity = await _equipmentRequestRepository.GetByIdExistAsync(id, ct);
            if (entity == null) return null;
            var dto = _mapper.Map<EquipmentRequestDto>(entity);

            var dict = await _workerQueryService.GetUserNames(new List<Guid> { entity.WorkerId });

            dto.WorkerName = dict.GetValueOrDefault(entity.WorkerId);

            return dto;
        }

        public async Task<PaginatedResult<EquipmentRequestDto>> Gets(PaginationRequest request, CancellationToken ct = default)
        {
            var result = await _equipmentRequestRepository.GetsPagingAsync(request, ct);

            var dtos = _mapper.Map<List<EquipmentRequestDto>>(result.Content);

            await EnrichWorkerNamesAsync(dtos);

            return new PaginatedResult<EquipmentRequestDto>(
                result.PageNumber,
                result.PageSize,
                result.TotalElements,
                dtos);
        }

        public async Task<PaginatedResult<EquipmentRequestDto>> GetsByWorkerId(Guid workerId, PaginationRequest request, CancellationToken ct = default)
        {
            var result = await _equipmentRequestRepository.GetsByWorkerIdPagingAsync(workerId, request, ct);

            var dtos = _mapper.Map<List<EquipmentRequestDto>>(result.Content);

            await EnrichWorkerNamesAsync(dtos);

            return new PaginatedResult<EquipmentRequestDto>(
                result.PageNumber,
                result.PageSize,
                result.TotalElements,
                dtos);
        }

        public async Task<PaginatedResult<EquipmentRequestDto>> GetsByTaskAssignmentId(Guid taskAssignmentId, PaginationRequest request, CancellationToken ct = default)
        {
            var result = await _equipmentRequestRepository.GetsByTaskAssignmentIdPagingAsync(taskAssignmentId, request, ct);

            var dtos = _mapper.Map<List<EquipmentRequestDto>>(result.Content);

            await EnrichWorkerNamesAsync(dtos);

            return new PaginatedResult<EquipmentRequestDto>(
                result.PageNumber,
                result.PageSize,
                result.TotalElements,
                dtos);
        }

        public async Task<PaginatedResult<EquipmentRequestDto>> GetsByStatus(EquipmentRequestStatus status, PaginationRequest request, CancellationToken ct = default)
        {
            var result = await _equipmentRequestRepository.GetsByStatusPagingAsync(status, request, ct);

            var dtos = _mapper.Map<List<EquipmentRequestDto>>(result.Content);

            await EnrichWorkerNamesAsync(dtos);

            return new PaginatedResult<EquipmentRequestDto>(
                result.PageNumber,
                result.PageSize,
                result.TotalElements,
                dtos);
        }

        public async Task<PaginatedResult<EquipmentRequestDto>> GetsByEquipmentId(Guid equipmentId, PaginationRequest request, CancellationToken ct = default)
        {
            var result = await _equipmentRequestRepository.GetsByEquipmentIdPagingAsync(equipmentId, request, ct);

            var dtos = _mapper.Map<List<EquipmentRequestDto>>(result.Content);

            await EnrichWorkerNamesAsync(dtos);

            return new PaginatedResult<EquipmentRequestDto>(
                result.PageNumber,
                result.PageSize,
                result.TotalElements,
                dtos);
        }

        public async Task<EquipmentRequestDto?> Create(CreateEquipmentRequestDto dto, CancellationToken ct = default)
        {
            var entity = _mapper.Map<EquipmentRequest>(dto);
            entity.Status = EquipmentRequestStatus.Pending;
            entity.Created = _dateTimeProvider.UtcNow;
            entity.CreatedBy = _userContext.UserId.ToString();

            await _equipmentRequestRepository.AddAsync(entity, ct);

            // notifi to Target - thong bao cho reviewer/manager co request moi
            // var message = new EquipmentRequestCreatedEvent
            // {
            //     RequestId    = entity.Id,
            //     WorkerId     = entity.WorkerId,
            //     EquipmentId  = entity.EquipmentId,
            //     Quantity     = entity.Quantity,
            //     Reason       = entity.Reason,
            //     CreatedAt    = entity.Created
            // };

            var dtoResult = _mapper.Map<EquipmentRequestDto>(entity);
            dtoResult.WorkerName = await GetWorkerNameAsync(entity.WorkerId);

            return dtoResult;
        }

        public async Task<EquipmentRequestDto?> Update(Guid id, UpdateEquipmentRequestDto dto, CancellationToken ct = default)
        {
            var entity = await _equipmentRequestRepository.GetByIdExistAsync(id, ct);
            if (entity == null) return null;

            _mapper.Map(dto, entity);
            entity.LastModified = _dateTimeProvider.UtcNow;
            entity.LastModifiedBy = _userContext.UserId.ToString();

            await _equipmentRequestRepository.UpdateAsync(entity, ct);

            // notifi to Target - thong bao cho reviewer/manager co request moi
            // var message = new EquipmentRequestCreatedEvent
            // {
            //     RequestId    = entity.Id,
            //     WorkerId     = entity.WorkerId,
            //     EquipmentId  = entity.EquipmentId,
            //     Quantity     = entity.Quantity,
            //     Reason       = entity.Reason,
            //     CreatedAt    = entity.Created
            // };

            var dtoResult = _mapper.Map<EquipmentRequestDto>(entity);
            dtoResult.WorkerName = await GetWorkerNameAsync(entity.WorkerId);

            return dtoResult;
        }

        public async Task<EquipmentRequestDto?> Review(Guid id, ReviewEquipmentRequestDto dto, CancellationToken ct = default)
        {
            var entity = await _equipmentRequestRepository.GetByIdExistAsync(id, ct);
            if (entity == null) return null;

            entity.Status = dto.Status;
            entity.ReviewedByUserId = _userContext.UserId;
            entity.ApprovedAt = dto.Status == EquipmentRequestStatus.Approved
                ? _dateTimeProvider.UtcNow
                : null;

            var reviewByUserName = _userContext.FullName;

            await _equipmentRequestRepository.UpdateAsync(entity, ct);

            // notifi to Target - gui mail thong bao ket qua review cho worker
            // var message = new EquipmentRequestReviewedEvent
            // {
            //     RequestId        = entity.Id,
            //     WorkerId         = entity.WorkerId,
            //     EquipmentId      = entity.EquipmentId,
            //     Status           = entity.Status,           // Approved | Rejected
            //     ReviewedByUserId = entity.ReviewedByUserId,
            //     ApprovedAt       = entity.ApprovedAt,
            //     ReviewedAt       = entity.LastModified
            // };
            // string routingKey = entity.Status == EquipmentRequestStatus.Approved
            //     ? "equipment-request.approved"
            //     : "equipment-request.rejected";

            var dtoResult = _mapper.Map<EquipmentRequestDto>(entity);
            dtoResult.WorkerName = await GetWorkerNameAsync(entity.WorkerId);
            dtoResult.ReviewedByUserName = reviewByUserName;

            return dtoResult;
        }

        public async Task<bool> Delete(Guid id, CancellationToken ct = default)
        {
            var entity = await _equipmentRequestRepository.GetByIdExistAsync(id, ct);
            if (entity == null) return false;

            await _equipmentRequestRepository.DeleteAsync(entity, ct);
            return true;
        }

        private async Task EnrichWorkerNamesAsync(List<EquipmentRequestDto> dtos)
        {
            var workerIds = dtos
                .Select(x => x.WorkerId)
                .Distinct()
                .ToList();

            if (!workerIds.Any()) return;

            var dict = await _workerQueryService.GetUserNames(workerIds);

            foreach (var dto in dtos)
            {
                dto.WorkerName = dict.GetValueOrDefault(dto.WorkerId);
            }
        }

        private async Task<string?> GetWorkerNameAsync(Guid workerId)
        {
            var dict = await _workerQueryService.GetUserNames(new List<Guid> { workerId });
            return dict.GetValueOrDefault(workerId);
        }
    }
}
