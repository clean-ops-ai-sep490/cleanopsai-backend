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

        public EquipmentRequestService(
            IEquipmentRequestRepository equipmentRequestRepository,
            IMapper mapper,
            IUserContext userContext,
            IDateTimeProvider dateTimeProvider)
        {
            _equipmentRequestRepository = equipmentRequestRepository;
            _mapper = mapper;
            _userContext = userContext;
            _dateTimeProvider = dateTimeProvider;
        }

        public async Task<EquipmentRequestDto?> GetById(Guid id, CancellationToken ct = default)
        {
            var entity = await _equipmentRequestRepository.GetByIdExistAsync(id, ct);
            if (entity == null) return null;
            return _mapper.Map<EquipmentRequestDto>(entity);
        }

        public async Task<PaginatedResult<EquipmentRequestDto>> Gets(PaginationRequest request, CancellationToken ct = default)
        {
            var result = await _equipmentRequestRepository.GetsPagingAsync(request, ct);
            return new PaginatedResult<EquipmentRequestDto>(
                result.PageNumber,
                result.PageSize,
                result.TotalElements,
                _mapper.Map<List<EquipmentRequestDto>>(result.Content));
        }

        public async Task<PaginatedResult<EquipmentRequestDto>> GetsByWorkerId(Guid workerId, PaginationRequest request, CancellationToken ct = default)
        {
            var result = await _equipmentRequestRepository.GetsByWorkerIdPagingAsync(workerId, request, ct);
            return new PaginatedResult<EquipmentRequestDto>(
                result.PageNumber,
                result.PageSize,
                result.TotalElements,
                _mapper.Map<List<EquipmentRequestDto>>(result.Content));
        }

        public async Task<PaginatedResult<EquipmentRequestDto>> GetsByTaskAssignmentId(Guid taskAssignmentId, PaginationRequest request, CancellationToken ct = default)
        {
            var result = await _equipmentRequestRepository.GetsByTaskAssignmentIdPagingAsync(taskAssignmentId, request, ct);
            return new PaginatedResult<EquipmentRequestDto>(
                result.PageNumber,
                result.PageSize,
                result.TotalElements,
                _mapper.Map<List<EquipmentRequestDto>>(result.Content));
        }

        public async Task<PaginatedResult<EquipmentRequestDto>> GetsByStatus(EquipmentRequestStatus status, PaginationRequest request, CancellationToken ct = default)
        {
            var result = await _equipmentRequestRepository.GetsByStatusPagingAsync(status, request, ct);
            return new PaginatedResult<EquipmentRequestDto>(
                result.PageNumber,
                result.PageSize,
                result.TotalElements,
                _mapper.Map<List<EquipmentRequestDto>>(result.Content));
        }

        public async Task<PaginatedResult<EquipmentRequestDto>> GetsByEquipmentId(Guid equipmentId, PaginationRequest request, CancellationToken ct = default)
        {
            var result = await _equipmentRequestRepository.GetsByEquipmentIdPagingAsync(equipmentId, request, ct);
            return new PaginatedResult<EquipmentRequestDto>(
                result.PageNumber,
                result.PageSize,
                result.TotalElements,
                _mapper.Map<List<EquipmentRequestDto>>(result.Content));
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

            return _mapper.Map<EquipmentRequestDto>(entity);
        }

        public async Task<EquipmentRequestDto?> Update(Guid id, UpdateEquipmentRequestDto dto, CancellationToken ct = default)
        {
            var entity = await _equipmentRequestRepository.GetByIdExistAsync(id, ct);
            if (entity == null) return null;

            _mapper.Map(dto, entity);
            entity.LastModified = _dateTimeProvider.UtcNow;
            entity.LastModifiedBy = _userContext.UserId.ToString();

            await _equipmentRequestRepository.UpdateAsync(entity, ct);
            return _mapper.Map<EquipmentRequestDto>(entity);
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

            return _mapper.Map<EquipmentRequestDto>(entity);
        }

        public async Task<bool> Delete(Guid id, CancellationToken ct = default)
        {
            var entity = await _equipmentRequestRepository.GetByIdExistAsync(id, ct);
            if (entity == null) return false;

            await _equipmentRequestRepository.DeleteAsync(entity, ct);
            return true;
        }
    }
}
