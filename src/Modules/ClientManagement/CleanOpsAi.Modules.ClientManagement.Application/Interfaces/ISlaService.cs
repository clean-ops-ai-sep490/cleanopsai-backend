using CleanOpsAi.Modules.ClientManagement.Application.Dtos;
using CleanOpsAi.Modules.ClientManagement.Application.Dtos.Slas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanOpsAi.Modules.ClientManagement.Application.Interfaces
{
    public interface ISlaService
    {
        Task<List<SlaResponse>> GetByIdAsync(Guid id);

        Task<List<SlaResponse>> GetAllAsync();

        Task<PagedResponse<SlaResponse>> GetAllPaginationAsync(int pageNumber, int pageSize);

        Task<PagedResponse<SlaResponse>> FilterAsync(
            Guid? workAreaId,
            Guid? contractId,
            int pageNumber,
            int pageSize);

        Task<SlaResponse> CreateAsync(SlaCreateRequest request);

        Task<SlaResponse> UpdateAsync(Guid id, SlaUpdateRequest request);

        Task<int> DeleteAsync(Guid id);
    }
}
