using CleanOpsAi.Modules.ClientManagement.Application.Dtos;
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
        Task<ContractResponse?> GetByIdAsync(Guid id);
        Task<List<ContractResponse>> GetAllAsync();
        Task<PagedResponse<ContractResponse>> GetAllPaginationAsync(int pageNumber, int pageSize);
        Task<ContractResponse> CreateAsync(ContractCreateRequest request);
        Task<ContractResponse?> UpdateAsync(Guid id, ContractUpdateRequest request);
        Task<int> DeleteAsync(Guid id);
        Task<List<ContractResponse>> GetByClientIdAsync(Guid clientId);
        Task<PagedResponse<ContractResponse>> GetByClientIdPaginationAsync(Guid clientId, int pageNumber, int pageSize);
    }
}
