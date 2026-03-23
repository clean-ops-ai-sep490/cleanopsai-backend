using CleanOpsAi.Modules.Workforce.Application.Dtos;
using CleanOpsAi.Modules.Workforce.Application.Dtos.WorkAreaSupervisors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanOpsAi.Modules.Workforce.Application.Interfaces
{
    public interface IWorkAreaSupervisorService
    {
        Task<WorkAreaSupervisorResponse?> GetByIdAsync(Guid id);
        Task<WorkAreaSupervisorResponse?> GetByUserIdAsync(string userId);
        Task<WorkAreaSupervisorResponse?> GetByWorkerIdAsync(Guid workerId);
        Task<List<WorkAreaSupervisorResponse>> GetAllAsync();
        Task<PagedResponse<WorkAreaSupervisorResponse>> GetAllPaginationAsync(int pageNumber, int pageSize);
        Task<List<WorkAreaSupervisorResponse>> GetByWorkAreaIdAsync(Guid workAreaId);
        Task<WorkAreaSupervisorAssignResponse> UpdateAsync(WorkAreaSupervisorUpdateRequest request);
        Task<int> DeleteAsync(Guid id);
        Task<List<WorkerGpsSimpleResponse>> GetWorkersLatestGpsByWorkAreaIdAsync(Guid workAreaId);
        Task<WorkAreaSupervisorAssignResponse> AssignWorkersAsync(WorkAreaSupervisorAssignRequest request);
        Task<int> UnassignWorkerAsync(Guid workAreaId, string userId, Guid workerId);
    }
}
