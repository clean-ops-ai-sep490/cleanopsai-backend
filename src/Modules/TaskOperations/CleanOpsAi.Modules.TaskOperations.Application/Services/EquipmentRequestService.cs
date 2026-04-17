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
    public class EquipmentRequestService : IEquipmentRequestService
    {
        private readonly IEquipmentRequestRepository _repo;
        private readonly IMapper _mapper;
        private readonly IUserContext _userContext;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly IWorkerQueryService _workerQueryService;
        private readonly IEquipmentQueryService _equipmentQueryService;
        private readonly ITaskAssignmentRepository _taskAssignmentRepository;

        public EquipmentRequestService(
            IEquipmentRequestRepository repo,
            IMapper mapper,
            IUserContext userContext,
            IDateTimeProvider dateTimeProvider,
            IWorkerQueryService workerQueryService,
            IEquipmentQueryService equipmentQueryService,
            ITaskAssignmentRepository taskAssignmentRepository)
        {
            _repo = repo;
            _mapper = mapper;
            _userContext = userContext;
            _dateTimeProvider = dateTimeProvider;
            _workerQueryService = workerQueryService;
            _equipmentQueryService = equipmentQueryService;
            _taskAssignmentRepository = taskAssignmentRepository;
        }

        public async Task<EquipmentRequestDto> CreateBatch(
            CreateEquipmentRequestBatchDto dto,
            CancellationToken ct = default)
        {
            var task = await _taskAssignmentRepository.GetByIdAsync(dto.TaskAssignmentId, ct);

            if (task != null)
            {
                if (task.Status != TaskAssignmentStatus.Completed &&
                    task.Status != TaskAssignmentStatus.Block)
                {
                    task.Status = TaskAssignmentStatus.Block;
                    task.LastModified = _dateTimeProvider.UtcNow;
                    task.LastModifiedBy = _userContext.UserId.ToString();

                    await _taskAssignmentRepository.SaveChangesAsync(ct);
                }
                else
                {
                    throw new BadRequestException("Cannot report issue for a completed or already blocked task.");
                }
            }

            dto.Items = dto.Items
                .GroupBy(x => x.EquipmentId)
                .Select(g => new CreateEquipmentRequestItemDto
                {
                    EquipmentId = g.Key,
                    Quantity = g.Sum(x => x.Quantity)
                }).ToList();

            var entity = new EquipmentRequest
            {
                TaskAssignmentId = dto.TaskAssignmentId,
                WorkerId = dto.WorkerId,
                Reason = dto.Reason,
                Status = EquipmentRequestStatus.Pending,
                Created = _dateTimeProvider.UtcNow,
                CreatedBy = _userContext.UserId.ToString(),

                Items = dto.Items.Select(x => new EquipmentRequestItem
                {
                    EquipmentId = x.EquipmentId,
                    Quantity = x.Quantity
                }).ToList()
            };

            await _repo.AddAsync(entity, ct);

            var result = await MapResult(entity);
            await EnrichAsync(new List<EquipmentRequestDto> { result }, ct);
            return result;
        }

        public async Task<EquipmentRequestDto?> Update(
            Guid id,
            UpdateEquipmentRequestDto dto,
            CancellationToken ct = default)
        {
            var entity = await _repo.GetByIdAsync(id, ct);
            if (entity == null) return null;

            if (dto.Reason != null)
                entity.Reason = string.IsNullOrWhiteSpace(dto.Reason)
                    ? null
                    : dto.Reason.Trim();

            entity.LastModified = _dateTimeProvider.UtcNow;
            entity.LastModifiedBy = _userContext.UserId.ToString();

            if (dto.Items != null)
            {
                entity.Items.Clear();

                if (dto.Items.Any())
                {
                    var items = dto.Items
                        .GroupBy(x => x.EquipmentId)
                        .Select(g => new EquipmentRequestItem
                        {
                            EquipmentId = g.Key,
                            Quantity = g.Sum(x => x.Quantity)
                        });

                    foreach (var item in items)
                    {
                        entity.Items.Add(item);
                    }
                }
            }

            await _repo.UpdateAsync(entity, ct);

            var result = await MapResult(entity);
            await EnrichAsync(new List<EquipmentRequestDto> { result }, ct);
            return result;
        }

        public async Task<EquipmentRequestDto?> Review(
            Guid id,
            ReviewEquipmentRequestDto dto,
            CancellationToken ct = default)
        {
            var entity = await _repo.GetByIdAsync(id, ct);
            if (entity == null) return null;

            entity.Status = dto.Status;
            entity.ReviewedByUserId = _userContext.UserId;
            entity.ApprovedAt = dto.Status == EquipmentRequestStatus.Approved
                ? _dateTimeProvider.UtcNow
                : null;

            await _repo.UpdateAsync(entity, ct);

            var result = await MapResult(entity);
            result.ReviewedByUserName = _userContext.FullName;

            await EnrichAsync(new List<EquipmentRequestDto> { result }, ct);
            return result;
        }

        public async Task<PaginatedResult<EquipmentRequestDto>> Gets(
            PaginationRequest request,
            CancellationToken ct = default)
        {
            var result = await _repo.GetsAsync(request, ct);

            var dtos = _mapper.Map<List<EquipmentRequestDto>>(result.Content);

            await EnrichAsync(dtos, ct);

            return new PaginatedResult<EquipmentRequestDto>(
                result.PageNumber,
                result.PageSize,
                result.TotalElements,
                dtos);
        }

        public async Task<EquipmentRequestDto?> GetById(
            Guid id,
            CancellationToken ct = default)
        {
            var entity = await _repo.GetByIdAsync(id, ct);
            if (entity == null) return null;

            var dto = _mapper.Map<EquipmentRequestDto>(entity);

            await EnrichAsync(new List<EquipmentRequestDto> { dto }, ct);

            return dto;
        }

        public async Task<PaginatedResult<EquipmentRequestDto>> GetByStatus(
            EquipmentRequestStatus status,
            PaginationRequest request,
            CancellationToken ct = default)
        {
            var paged = await _repo.GetByStatusAsync(status, request, ct);

            var dtos = _mapper.Map<List<EquipmentRequestDto>>(paged.Content);

            if (dtos.Count > 0)
            {
                await EnrichAsync(dtos, ct);
            }

            return new PaginatedResult<EquipmentRequestDto>(
                paged.PageNumber,
                paged.PageSize,
                paged.TotalElements,
                dtos);
        }

        public async Task<PaginatedResult<EquipmentRequestDto>> GetByTaskAssignmentId(
            Guid taskAssignmentId,
            PaginationRequest request,
            CancellationToken ct = default)
        {
            var result = await _repo.GetByTaskAssignmentIdAsync(taskAssignmentId, request, ct);

            var dtos = _mapper.Map<List<EquipmentRequestDto>>(result.Content);

            await EnrichAsync(dtos, ct);

            return new PaginatedResult<EquipmentRequestDto>(
                result.PageNumber,
                result.PageSize,
                result.TotalElements,
                dtos);
        }

        public async Task<PaginatedResult<EquipmentRequestDto>> GetByWorkerId(
            Guid workerId,
            PaginationRequest request,
            CancellationToken ct = default)
        {
            var result = await _repo.GetByWorkerIdAsync(workerId, request, ct);

            var dtos = _mapper.Map<List<EquipmentRequestDto>>(result.Content);

            await EnrichAsync(dtos, ct);

            return new PaginatedResult<EquipmentRequestDto>(
                result.PageNumber,
                result.PageSize,
                result.TotalElements,
                dtos);
        }

        public async Task<bool> Delete(Guid id, CancellationToken ct = default)
        {
            var entity = await _repo.GetByIdAsync(id, ct);
            if (entity == null) return false;

            await _repo.DeleteAsync(entity, ct);
            return true;
        }

        private async Task EnrichAsync(List<EquipmentRequestDto> dtos, CancellationToken ct)
        {
            // WORKER
            var workerIds = dtos.Select(x => x.WorkerId).Distinct().ToList();

            var workerDict = workerIds.Any()
                ? await _workerQueryService.GetUserNames(workerIds)
                : new Dictionary<Guid, string>();

            // EQUIPMENT
            var equipmentIds = dtos
                .SelectMany(x => x.Items)
                .Select(x => x.EquipmentId)
                .Distinct()
                .ToList();

            var equipmentDict = equipmentIds.Any()
                ? await _equipmentQueryService.GetNamesAsync(equipmentIds, ct)
                : new Dictionary<Guid, string>();

            // MAP
            foreach (var dto in dtos)
            {
                dto.WorkerName = workerDict.GetValueOrDefault(dto.WorkerId);

                foreach (var item in dto.Items)
                {
                    item.EquipmentName = equipmentDict.GetValueOrDefault(item.EquipmentId);
                }
            }
        }

        private async Task<EquipmentRequestDto> MapResult(EquipmentRequest entity)
        {
            return _mapper.Map<EquipmentRequestDto>(entity);
        }
    }
}
