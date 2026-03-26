using CleanOpsAi.BuildingBlocks.Application.Pagination;
using CleanOpsAi.Modules.TaskOperations.Domain.Entities;
using CleanOpsAi.Modules.TaskOperations.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanOpsAi.Modules.TaskOperations.Application.Common.Interfaces.Repositories
{
    public interface IEquipmentRequestRepository : IBaseRepo<EquipmentRequest, Guid>
    {
        Task<EquipmentRequest?> GetByIdExistAsync(Guid id, CancellationToken ct = default);
        Task<PaginatedResult<EquipmentRequest>> GetsPagingAsync(PaginationRequest request, CancellationToken ct = default);
        Task<PaginatedResult<EquipmentRequest>> GetsByWorkerIdPagingAsync(Guid workerId, PaginationRequest request, CancellationToken ct = default);
        Task<PaginatedResult<EquipmentRequest>> GetsByTaskAssignmentIdPagingAsync(Guid taskAssignmentId, PaginationRequest request, CancellationToken ct = default);
        Task<PaginatedResult<EquipmentRequest>> GetsByStatusPagingAsync(EquipmentRequestStatus status, PaginationRequest request, CancellationToken ct = default);
        Task<PaginatedResult<EquipmentRequest>> GetsByEquipmentIdPagingAsync(Guid equipmentId, PaginationRequest request, CancellationToken ct = default);
        Task<bool> ExistsAsync(Guid taskAssignmentId, Guid equipmentId, CancellationToken ct = default);
        Task AddAsync(EquipmentRequest request, CancellationToken ct = default);
        Task UpdateAsync(EquipmentRequest request, CancellationToken ct = default);
        Task DeleteAsync(EquipmentRequest request, CancellationToken ct = default);
    }
}
