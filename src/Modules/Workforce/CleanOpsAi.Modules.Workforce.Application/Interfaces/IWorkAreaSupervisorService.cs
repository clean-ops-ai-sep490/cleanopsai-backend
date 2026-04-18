using CleanOpsAi.Modules.Workforce.Application.Dtos;
using CleanOpsAi.Modules.Workforce.Application.Dtos.WorkAreaSupervisors; 

namespace CleanOpsAi.Modules.Workforce.Application.Interfaces
{
    public interface IWorkAreaSupervisorService
    {
        Task<WorkAreaSupervisorResponse?> GetByIdAsync(Guid id);
        Task<List<WorkAreaSupervisorResponse>> GetByUserIdAsync(Guid userId);
        Task<PagedResponse<WorkAreaSupervisorResponse>> GetByWorkerIdPaginationAsync(Guid workerId, int pageNumber, int pageSize);
        Task<List<WorkAreaSupervisorResponse>> GetAllAsync();
        Task<PagedResponse<WorkAreaSupervisorResponse>> GetAllPaginationAsync(int pageNumber, int pageSize);
        Task<List<WorkAreaSupervisorResponse>> GetByWorkAreaIdAsync(Guid workAreaId);
        Task<WorkAreaSupervisorAssignResponse> UpdateAsync(WorkAreaSupervisorUpdateRequest request);
        Task<int> DeleteAsync(Guid id);
        Task<PagedResponse<WorkerLiveGpsResponse>> GetWorkersLiveStatusByWorkAreaPagingAsync(Guid workAreaId, int pageNumber, int pageSize, int offlineThresholdMinutes = 10);
        Task<WorkAreaSupervisorAssignResponse> AssignWorkersAsync(WorkAreaSupervisorAssignRequest request);
        Task<int> UnassignWorkerAsync(Guid workAreaId, Guid userId, Guid workerId);
        //Task<WorkAreaSupervisorResponse?> GetSupervisorByWorkAreaAndWorkerAsync(Guid workAreaId, Guid workerId);
        Task<(bool Found, Guid? SupervisorUserId)> GetCommonSupervisorAsync(Guid workAreaId, Guid workerId, Guid workerIdTarget, CancellationToken ct = default);
        Task<PagedResponse<WorkAreaWithLocationResponse>> GetWorkAreasBySupervisorPaginationAsync(Guid supervisorId, int pageNumber, int pageSize);
        Task<PagedResponse<WorkerGroupResponse>> GetUniqueWorkersBySupervisorPagingAsync(Guid supervisorId, int pageNumber, int pageSize);
        Task<List<Guid>> GetManagedWorkerUserIdsBySupervisorAsync(Guid supervisorId, CancellationToken ct = default);
        Task<PagedResponse<WorkAreaSupervisorResponse>> GetWorkersByWorkAreaPagingAsync(Guid workAreaId, int pageNumber, int pageSize);
    }
}
