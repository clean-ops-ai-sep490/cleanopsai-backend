using CleanOpsAi.Modules.Workforce.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanOpsAi.Modules.Workforce.Application.Interfaces
{
    public interface IPpeItemRepository
    {
        Task<PpeItem?> GetByIdAsync(Guid id);
        Task<List<PpeItem>> GetAllAsync();
        Task<(List<PpeItem> Items, int TotalCount)> GetAllPaginationAsync(int pageNumber, int pageSize);

        Task<int> CreateAsync(PpeItem entity);
        Task<int> UpdateAsync(PpeItem entity);
        Task<int> DeleteAsync(Guid id);

        Task<List<PpeItem>> GetByActionKeyAsync(string actionKey);
        Task<List<string>> GetAllActionKeysAsync();
    }
}
