using CleanOpsAi.Modules.Workforce.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanOpsAi.Modules.Workforce.Application.Interfaces
{
    public interface IWorkerGpsRepository
    {
        Task<WorkerGps?> GetByIdAsync(Guid id);

        Task<List<WorkerGps>> GetAllAsync();

        Task<(List<WorkerGps> Items, int TotalCount)> GetAllPaginationAsync(int pageNumber, int pageSize);

        Task<int> CreateAsync(WorkerGps entity);

        Task<int> UpdateAsync(WorkerGps entity);

        Task<int> DeleteAsync(Guid id);
    }
}
