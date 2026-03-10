using CleanOpsAi.Modules.ClientManagement.Application.Dtos.Contracts;
using CleanOpsAi.Modules.ClientManagement.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanOpsAi.Modules.ClientManagement.Application.Interfaces
{
    public interface IContractService
    {
        Task<int> CreateAsync(ContractCreateRequest request);
        Task<int> UpdateAsync(Guid id, ContractUpdateRequest request);
        Task<int> DeleteAsync(Guid id);
        Task<Contract?> GetByIdAsync(Guid id);
        Task<List<Contract>> GetAllAsync();
    }
}
