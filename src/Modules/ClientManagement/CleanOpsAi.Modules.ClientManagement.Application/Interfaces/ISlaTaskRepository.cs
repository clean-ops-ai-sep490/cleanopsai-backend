using CleanOpsAi.Modules.ClientManagement.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanOpsAi.Modules.ClientManagement.Application.Interfaces
{
    public interface ISlaTaskRepository
    {
        Task<SlaTask> GetByIdAsync(Guid id);

        Task<List<SlaTask>> GetAllAsync();

        Task<(List<SlaTask> Items, int TotalCount)> GetAllPaginationAsync(int pageNumber, int pageSize);

        Task<List<SlaTask>> GetBySlaIdAsync(Guid slaId);

        Task<int> CreateAsync(SlaTask slaTask);

        Task<int> UpdateAsync(SlaTask slaTask);

        Task<int> DeleteAsync(Guid id);
    }
}
