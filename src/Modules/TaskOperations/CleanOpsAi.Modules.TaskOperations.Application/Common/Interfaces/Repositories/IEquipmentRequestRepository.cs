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
        Task<EquipmentRequest?> GetByIdAsync(Guid id, CancellationToken ct = default);

        Task<PaginatedResult<EquipmentRequest>> GetsAsync(
            PaginationRequest request,
            CancellationToken ct = default);

        Task AddAsync(EquipmentRequest entity, CancellationToken ct = default);

        Task UpdateAsync(EquipmentRequest entity, CancellationToken ct = default);

        Task DeleteAsync(EquipmentRequest entity, CancellationToken ct = default);
    }
}
