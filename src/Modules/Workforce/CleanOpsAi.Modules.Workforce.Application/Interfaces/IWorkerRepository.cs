using CleanOpsAi.Modules.Workforce.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanOpsAi.Modules.Workforce.Application.Interfaces
{
    public interface IWorkerRepository
    {
        Task<Worker?> GetByIdAsync(Guid id);

        Task<Worker?> GetByUserIdAsync(string userId);

        Task<List<Worker>> GetAllAsync();

        Task<(List<Worker> Items, int TotalCount)> GetAllPaginationAsync(
            int pageNumber,
            int pageSize);

        Task<int> CreateAsync(Worker worker);

        Task<int> UpdateAsync(Worker worker);

        Task<int> DeleteAsync(Guid id);
    }
}
