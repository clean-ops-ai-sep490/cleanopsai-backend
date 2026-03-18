using CleanOpsAi.Modules.Workforce.Application.Dtos;
using CleanOpsAi.Modules.Workforce.Application.Dtos.Equipments;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanOpsAi.Modules.Workforce.Application.Interfaces
{
    public interface IEquipmentService
    {
        Task<List<EquipmentResponse>?> GetByIdAsync(Guid id);

        Task<List<EquipmentResponse>> GetAllAsync();

        Task<PagedResponse<EquipmentResponse>> GetAllPaginationAsync(int pageNumber, int pageSize);

        Task<EquipmentResponse?> CreateAsync(EquipmentCreateRequest request);

        Task<EquipmentResponse?> UpdateAsync(Guid id, EquipmentUpdateRequest request);

        Task<int> DeleteAsync(Guid id);
    }
}
