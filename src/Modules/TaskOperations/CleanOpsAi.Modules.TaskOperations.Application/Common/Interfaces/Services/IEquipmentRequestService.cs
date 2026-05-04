using CleanOpsAi.BuildingBlocks.Application.Pagination;
using CleanOpsAi.Modules.TaskOperations.Application.DTOs.Request;
using CleanOpsAi.Modules.TaskOperations.Application.DTOs.Response;
using CleanOpsAi.Modules.TaskOperations.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanOpsAi.Modules.TaskOperations.Application.Common.Interfaces.Services
{
    public interface IEquipmentRequestService
    {
        Task<EquipmentRequestDto> CreateBatch(
            CreateEquipmentRequestBatchDto dto,
            CancellationToken ct = default);

        Task<EquipmentRequestDto?> Update(
            Guid id,
            UpdateEquipmentRequestDto dto,
            CancellationToken ct = default);

        Task<EquipmentRequestDto?> Review(
            Guid id,
            ReviewEquipmentRequestDto dto,
            CancellationToken ct = default);

        Task<PaginatedResult<EquipmentRequestDto>> Gets(
            PaginationRequest request,
            CancellationToken ct = default);

        Task<bool> Delete(
            Guid id,
            CancellationToken ct = default);

        Task<EquipmentRequestDto?> GetById(
            Guid id,
            CancellationToken ct = default);

        Task<PaginatedResult<EquipmentRequestDto>> GetByStatus(
            EquipmentRequestStatus status, 
            PaginationRequest request,
            CancellationToken ct = default);

        Task<PaginatedResult<EquipmentRequestDto>> GetByTaskAssignmentId(
            Guid taskAssignmentId,
            PaginationRequest request,
            CancellationToken ct = default);

        Task<PaginatedResult<EquipmentRequestDto>> GetByWorkerId(
            Guid workerId,
            PaginationRequest request,
            CancellationToken ct = default);

    }
}