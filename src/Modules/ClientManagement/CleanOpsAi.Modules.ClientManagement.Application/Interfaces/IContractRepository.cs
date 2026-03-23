using CleanOpsAi.Modules.ClientManagement.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanOpsAi.Modules.ClientManagement.Application.Interfaces
{
    public interface IContractRepository
    {
        Task<Contract?> GetByIdAsync(Guid id);
        Task<List<Contract>> GetAllAsync();
        Task<(List<Contract> Items, int TotalCount)> GetAllPaginationAsync(int pageNumber, int pageSize);
        Task<int> CreateAsync(Contract contract);
        Task<int> UpdateAsync(Contract contract);
        Task<int> DeleteAsync(Guid id);
        Task<List<Contract>> GetByClientIdAsync(Guid clientId);
        Task<(List<Contract> Items, int TotalCount)> GetByClientIdPaginationAsync(Guid clientId, int pageNumber, int pageSize);
    }
}
