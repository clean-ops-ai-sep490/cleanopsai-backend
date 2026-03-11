using CleanOpsAi.Modules.ClientManagement.Application.Dtos;
using CleanOpsAi.Modules.ClientManagement.Application.Dtos.Clients;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanOpsAi.Modules.ClientManagement.Application.Interfaces
{
    public interface IClientService
    {
        Task<List<ClientResponse>> GetByIdAsync(Guid id);
        Task<List<ClientResponse>> GetAllAsync();
        Task<PagedResponse<ClientResponse>> GetAllPaginationAsync(int pageNumber, int pageSize);
        Task<ClientResponse> CreateAsync(ClientCreateRequest request);
        Task<ClientResponse> UpdateAsync(Guid id, ClientUpdateRequest request);
        Task<int> DeleteAsync(Guid id);
    }
}
