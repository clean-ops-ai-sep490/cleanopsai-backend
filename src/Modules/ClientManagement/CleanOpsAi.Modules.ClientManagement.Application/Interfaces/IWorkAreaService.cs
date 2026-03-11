using CleanOpsAi.Modules.ClientManagement.Application.Dtos;
using CleanOpsAi.Modules.ClientManagement.Application.Dtos.Workareas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanOpsAi.Modules.ClientManagement.Application.Interfaces
{
    public interface IWorkAreaService
    {
        Task<WorkAreaResponse?> GetByIdAsync(Guid id);
        Task<List<WorkAreaResponse>> GetAllAsync();
        Task<PagedResponse<WorkAreaResponse>> GetAllPaginationAsync(int pageNumber, int pageSize);
        Task<PagedResponse<WorkAreaResponse>> GetByZoneIdPaginationAsync(Guid zoneId, int pageNumber, int pageSize);
        Task<WorkAreaResponse> CreateAsync(WorkAreaCreateRequest request);
        Task<WorkAreaResponse?> UpdateAsync(Guid id, WorkAreaUpdateRequest request);
        Task<int> DeleteAsync(Guid id);
    }
}
