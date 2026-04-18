using CleanOpsAi.Modules.ClientManagement.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanOpsAi.Modules.ClientManagement.Application.Interfaces
{
    public interface IClientRepository
    {
        Task<Client> GetByIdAsync(Guid id);
        Task<List<Client>> GetAllAsync();
        Task<(List<Client> Items, int TotalCount)> GetAllPaginationAsync(int pageNumber, int pageSize);
        Task<int> CreateAsync(Client client);
        Task<int> UpdateAsync(Client client);
        Task<int> DeleteAsync(Guid id);
    }
}
