using CleanOpsAi.Modules.ClientManagement.Application.Dtos;
using CleanOpsAi.Modules.ClientManagement.Application.Dtos.Locations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanOpsAi.Modules.ClientManagement.Application.Interfaces
{
    public interface ILocationService
    {
        Task<List<LocationResponse>> GetByIdAsync(Guid id);
        Task<List<LocationResponse>> GetAllAsync();
        Task<PagedResponse<LocationResponse>> GetAllPaginationAsync(int pageNumber, int pageSize);
        Task<LocationResponse> CreateAsync(LocationCreateRequest request);
        Task<LocationResponse> UpdateAsync(Guid id, LocationUpdateRequest request);
        Task<int> DeleteAsync(Guid id);
        Task<List<LocationResponse>> GetByClientIdAsync(Guid clientId);
        Task<PagedResponse<LocationResponse>> GetByClientIdPaginationAsync(Guid clientId, int pageNumber, int pageSize);
    }
}
