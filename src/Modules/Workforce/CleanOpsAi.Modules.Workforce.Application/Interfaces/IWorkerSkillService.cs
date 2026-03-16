using CleanOpsAi.Modules.Workforce.Application.Dtos;
using CleanOpsAi.Modules.Workforce.Application.Dtos.WorkerSkills;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanOpsAi.Modules.Workforce.Application.Interfaces
{
    public interface IWorkerSkillService
    {
        Task<WorkerSkillResponse?> GetByIdAsync(Guid workerId, Guid skillId);

        Task<List<WorkerSkillResponse>> GetAllAsync();

        Task<PagedResponse<WorkerSkillResponse>> GetAllPaginationAsync(int pageNumber, int pageSize);

        Task<WorkerSkillResponse?> CreateAsync(WorkerSkillCreateRequest request);

        Task<WorkerSkillResponse?> UpdateAsync(Guid workerId, Guid skillId, WorkerSkillUpdateRequest request);

        Task<int> DeleteAsync(Guid workerId, Guid skillId);
    }
}
