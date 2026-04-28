using CleanOpsAi.Modules.Workforce.Domain.Entities; 

namespace CleanOpsAi.Modules.Workforce.Application.Interfaces
{
    public interface IWorkAreaSupervisorRepository
    {
        Task<WorkAreaSupervisor?> GetByIdAsync(Guid id);
        Task<List<WorkAreaSupervisor>> GetByUserIdAsync(Guid userId);
        Task<(List<WorkAreaSupervisor> Items, int TotalCount)> GetByWorkerIdPaginationAsync(Guid workerId, int pageNumber, int pageSize);
        Task<List<WorkAreaSupervisor>> GetAllAsync();
        Task<(List<WorkAreaSupervisor> Items, int TotalCount)> GetAllPaginationAsync(int pageNumber, int pageSize);
        Task<List<WorkAreaSupervisor>> GetByWorkAreaIdAsync(Guid workAreaId);
        Task<int> DeleteAsync(Guid id);
        Task<List<WorkerGps>> GetLatestGpsByWorkAreaAsync(Guid workAreaId);
        Task<bool> ExistsAsync(Guid workAreaId, Guid userId, Guid workerId);
        Task<int> CreateRangeAsync(List<WorkAreaSupervisor> entities);
        Task<WorkAreaSupervisor?> GetByWorkAreaUserWorkerAsync(Guid workAreaId, Guid userId, Guid workerId);
        Task<int> DeleteByWorkAreaAndSupervisorAsync(Guid workAreaId, Guid supervisorId);
        Task<WorkAreaSupervisor?> GetByWorkAreaAndWorkerAsync(Guid workAreaId, Guid workerId);
        Task<List<Guid>> GetSupervisorIdsAsync(Guid workAreaId, Guid workerId, CancellationToken ct = default);
        Task<List<WorkAreaSupervisor>> GetWorkersBySupervisorIdAsync(Guid supervisorId);
        Task<(List<WorkAreaSupervisor> Items, int TotalCount)> GetWorkersByWorkAreaPagingAsync(Guid workAreaId, int pageNumber, int pageSize);

    } 
}
