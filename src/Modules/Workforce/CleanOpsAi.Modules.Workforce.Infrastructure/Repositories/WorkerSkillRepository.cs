using CleanOpsAi.Modules.Workforce.Application.Interfaces;
using CleanOpsAi.Modules.Workforce.Domain.Entities;
using CleanOpsAi.Modules.Workforce.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanOpsAi.Modules.Workforce.Infrastructure.Repositories
{
    public class WorkerSkillRepository : IWorkerSkillRepository
    {
        private readonly WorkforceDbContext _dbContext;

        public WorkerSkillRepository(WorkforceDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<WorkerSkill?> GetByIdAsync(Guid workerId, Guid skillId)
        {
            return await _dbContext.Set<WorkerSkill>()
                .Include(x => x.Worker)
                .Include(x => x.Skill)
                .FirstOrDefaultAsync(x =>
                    x.WorkerId == workerId &&
                    x.SkillId == skillId);
        }

        public async Task<List<WorkerSkill>> GetAllAsync()
        {
            return await _dbContext.Set<WorkerSkill>()
                .Include(x => x.Worker)
                .Include(x => x.Skill)
                .OrderByDescending(x => x.WorkerId)
                .ToListAsync();
        }

        public async Task<(List<WorkerSkill> Items, int TotalCount)> GetAllPaginationAsync(int pageNumber, int pageSize)
        {
            var query = _dbContext.Set<WorkerSkill>()
                .Include(x => x.Worker)
                .Include(x => x.Skill)
                .OrderByDescending(x => x.WorkerId);

            var totalCount = await query.CountAsync();

            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        public async Task<int> CreateAsync(WorkerSkill entity)
        {
            _dbContext.Set<WorkerSkill>().Add(entity);
            return await _dbContext.SaveChangesAsync();
        }

        public async Task<int> UpdateAsync(WorkerSkill entity)
        {
            _dbContext.Set<WorkerSkill>().Update(entity);
            return await _dbContext.SaveChangesAsync();
        }

        public async Task<int> DeleteAsync(Guid workerId, Guid skillId)
        {
            var entity = await _dbContext.Set<WorkerSkill>()
                .FirstOrDefaultAsync(x =>
                    x.WorkerId == workerId &&
                    x.SkillId == skillId);

            if (entity != null)
            {
                _dbContext.Set<WorkerSkill>().Remove(entity);
                return await _dbContext.SaveChangesAsync();
            }

            return 0;
        }
    }
}
