using CleanOpsAi.Modules.Workforce.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanOpsAi.Modules.Workforce.Application.Interfaces
{
    public interface ISkillRepository
    {
        Task<Skill?> GetByIdAsync(Guid id);

        Task<List<Skill>> GetAllAsync();

        Task<(List<Skill> Items, int TotalCount)> GetAllPaginationAsync(int pageNumber, int pageSize);

        Task<int> CreateAsync(Skill skill);

        Task<int> UpdateAsync(Skill skill);

        Task<int> DeleteAsync(Guid id);
    }
}
