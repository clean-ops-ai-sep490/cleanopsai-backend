using CleanOpsAi.Modules.Workforce.Application.Dtos;
using CleanOpsAi.Modules.Workforce.Application.Dtos.Skills;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanOpsAi.Modules.Workforce.Application.Interfaces
{
    public interface ISkillService
    {
        Task<List<SkillResponse>?> GetByIdAsync(Guid id);

        Task<List<SkillResponse>> GetAllAsync();

        Task<PagedResponse<SkillResponse>> GetAllPaginationAsync(int pageNumber, int pageSize);

        Task<SkillResponse?> CreateAsync(SkillCreateRequest request);

        Task<SkillResponse?> UpdateAsync(Guid id, SkillUpdateRequest request);

        Task<int> DeleteAsync(Guid id);
    }
}
