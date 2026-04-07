using CleanOpsAi.Modules.Workforce.Application.Dtos;
using CleanOpsAi.Modules.Workforce.Application.Dtos.PpeItems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanOpsAi.Modules.Workforce.Application.Interfaces
{
    public interface IPpeItemService
    {
        Task<List<PpeItemResponse>?> GetByIdAsync(Guid id);
        Task<List<PpeItemResponse>> GetAllAsync();
        Task<PagedResponse<PpeItemResponse>> GetAllPaginationAsync(int pageNumber, int pageSize);

        Task<PpeItemResponse> CreateAsync(PpeItemCreateRequest request);
        Task<PpeItemResponse> UpdateAsync(Guid id, PpeItemUpdateRequest request);
        Task<int> DeleteAsync(Guid id);

        Task<List<PpeItemResponse>> GetByActionKeyAsync(string actionKey);
        Task<List<string>> GetAllActionKeysAsync();
    }
}
