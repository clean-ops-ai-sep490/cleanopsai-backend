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
        // ✅ CREATE batch
        Task<EquipmentRequestDto> CreateBatch(
            CreateEquipmentRequestBatchDto dto,
            CancellationToken ct = default);

        // ✅ UPDATE (replace items + reason)
        Task<EquipmentRequestDto?> Update(
            Guid id,
            UpdateEquipmentRequestDto dto,
            CancellationToken ct = default);

        // ✅ REVIEW (approve / reject)
        Task<EquipmentRequestDto?> Review(
            Guid id,
            ReviewEquipmentRequestDto dto,
            CancellationToken ct = default);

        // ✅ GET LIST (paging)
        Task<PaginatedResult<EquipmentRequestDto>> Gets(
            PaginationRequest request,
            CancellationToken ct = default);

        // ✅ DELETE (soft delete)
        Task<bool> Delete(
            Guid id,
            CancellationToken ct = default);

        Task<EquipmentRequestDto?> GetById(
            Guid id,
            CancellationToken ct = default);
    }
}
