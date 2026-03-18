using CleanOpsAi.Modules.Workforce.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanOpsAi.Modules.Workforce.Application.Interfaces
{
    public interface IWorkerSkillRepository
    {
        Task<WorkerSkill?> GetByIdAsync(Guid workerId, Guid skillId);

        Task<List<WorkerSkill>> GetAllAsync();

        Task<(List<WorkerSkill> Items, int TotalCount)> GetAllPaginationAsync(int pageNumber, int pageSize);

        Task<int> CreateAsync(WorkerSkill entity);

        Task<int> UpdateAsync(WorkerSkill entity);

        Task<int> DeleteAsync(Guid workerId, Guid skillId);
    }
}
