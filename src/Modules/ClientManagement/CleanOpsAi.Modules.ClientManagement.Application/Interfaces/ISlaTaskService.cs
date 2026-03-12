using CleanOpsAi.Modules.ClientManagement.Application.Dtos;
using CleanOpsAi.Modules.ClientManagement.Application.Dtos.SlaTasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanOpsAi.Modules.ClientManagement.Application.Interfaces
{
    public interface ISlaTaskService
    {
        Task<SlaTaskResponse> GetByIdAsync(Guid id);

        Task<PagedResponse<SlaTaskResponse>> GetAllPaginationAsync(int pageNumber, int pageSize);

        Task<List<SlaTaskResponse>> GetBySlaIdAsync(Guid slaId);

        Task<SlaTaskResponse> CreateAsync(SlaTaskCreateRequest request);

        Task<SlaTaskResponse> UpdateAsync(Guid id, SlaTaskUpdateRequest request);

        Task<int> DeleteAsync(Guid id);
    }
}
