using CleanOpsAi.Modules.ClientManagement.Application.Dtos;
using CleanOpsAi.Modules.ClientManagement.Application.Dtos.Zones;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanOpsAi.Modules.ClientManagement.Application.Interfaces
{
    public interface IZoneService
    {
        Task<ZoneResponse?> GetByIdAsync(Guid id);
        Task<List<ZoneResponse>> GetAllAsync();
        Task<PagedResponse<ZoneResponse>> GetAllPaginationAsync(int pageNumber, int pageSize);
        Task<PagedResponse<ZoneResponse>> GetByLocationIdPaginationAsync(Guid locationId, int pageNumber, int pageSize);
        Task<ZoneResponse> CreateAsync(ZoneCreateRequest request);
        Task<ZoneResponse?> UpdateAsync(Guid id, ZoneUpdateRequest request);
        Task<int> DeleteAsync(Guid id);
    }
}
