using CleanOpsAi.Modules.Workforce.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanOpsAi.Modules.Workforce.Application.Interfaces
{
    public interface IWorkAreaSupervisorRepository
    {
        Task<WorkAreaSupervisor?> GetByIdAsync(Guid id);
        Task<WorkAreaSupervisor?> GetByUserIdAsync(string userId);
        Task<WorkAreaSupervisor?> GetByWorkerIdAsync(Guid workerId);
        Task<List<WorkAreaSupervisor>> GetAllAsync();
        Task<(List<WorkAreaSupervisor> Items, int TotalCount)> GetAllPaginationAsync(int pageNumber, int pageSize);
        Task<List<WorkAreaSupervisor>> GetByWorkAreaIdAsync(Guid workAreaId);
        Task<int> DeleteAsync(Guid id);
        Task<List<WorkerGps>> GetWorkersLatestGpsByWorkAreaIdAsync(Guid workAreaId);
        Task<bool> ExistsAsync(Guid workAreaId, string userId, Guid workerId);
        Task<int> CreateRangeAsync(List<WorkAreaSupervisor> entities);
        Task<WorkAreaSupervisor?> GetByWorkAreaUserWorkerAsync(Guid workAreaId, string userId, Guid workerId);
        Task<int> DeleteByWorkAreaAndSupervisorAsync(Guid workAreaId, string supervisorId);
    }
}
