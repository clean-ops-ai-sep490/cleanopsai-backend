using CleanOpsAi.Modules.ClientManagement.Application.Dtos;
using CleanOpsAi.Modules.ClientManagement.Application.Dtos.WorkareaDetails;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanOpsAi.Modules.ClientManagement.Application.Interfaces
{
    public interface IWorkAreaDetailService
    {
        Task<WorkAreaDetailResponse?> GetByIdAsync(Guid id);
        Task<PagedResponse<WorkAreaDetailResponse>> GetAllPaginationAsync(int pageNumber, int pageSize);
        Task<PagedResponse<WorkAreaDetailResponse>> GetByWorkAreaIdPaginationAsync(Guid workAreaId, int pageNumber, int pageSize);
        Task<WorkAreaDetailResponse> CreateAsync(WorkAreaDetailCreateRequest request);
        Task<WorkAreaDetailResponse?> UpdateAsync(Guid id, WorkAreaDetailUpdateRequest request);
        Task<int> DeleteAsync(Guid id);
    }
}
