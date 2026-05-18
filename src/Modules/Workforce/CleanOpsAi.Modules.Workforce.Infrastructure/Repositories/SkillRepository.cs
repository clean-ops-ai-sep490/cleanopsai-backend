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
    public class SkillRepository : ISkillRepository
    {
        private readonly WorkforceDbContext _dbContext;

        public SkillRepository(WorkforceDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Skill?> GetByIdAsync(Guid id)
        {
            return await _dbContext.Set<Skill>()
                .FirstOrDefaultAsync(x => x.Id == id && x.IsDeleted == false);
        }

        public async Task<List<Skill>> GetAllAsync()
        {
            return await _dbContext.Set<Skill>()
                .Where(x => x.IsDeleted == false)
                .OrderByDescending(x => x.Created)
                .ToListAsync();
        }

        public async Task<(List<Skill> Items, int TotalCount)> GetAllPaginationAsync(int pageNumber, int pageSize)
        {
            var query = _dbContext.Set<Skill>()
                .Where(x => x.IsDeleted == false)
                .OrderByDescending(x => x.Created);

            var totalCount = await query.CountAsync();

            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        public async Task<int> CreateAsync(Skill skill)
        {
            _dbContext.Set<Skill>().Add(skill);
            return await _dbContext.SaveChangesAsync();
        }

        public async Task<int> UpdateAsync(Skill skill)
        {
            _dbContext.Set<Skill>().Update(skill);
            return await _dbContext.SaveChangesAsync();
        }

        public async Task<int> DeleteAsync(Guid id)
        {
            var skill = await _dbContext.Set<Skill>()
                .FirstOrDefaultAsync(x => x.Id == id);

            if (skill != null)
            {
                skill.IsDeleted = true;
                _dbContext.Set<Skill>().Update(skill);
                return await _dbContext.SaveChangesAsync();
            }

            return 0;
        }

        public async Task<List<string>> GetAllCategoriesAsync()
        {
            return await _dbContext.Set<Skill>()
                .Where(x => x.IsDeleted == false)
                .Select(x => x.Category)
                .Distinct()
                .OrderBy(x => x)
                .ToListAsync();
        }

        public async Task<List<Skill>> GetByCategoryAsync(string category)
        {
            return await _dbContext.Set<Skill>()
                .Where(x => x.IsDeleted == false && x.Category == category)
                .OrderByDescending(x => x.Created)
                .ToListAsync();
        }
        public async Task<List<Skill>> GetByNameAsync(string name)
        {
            return await _dbContext.Set<Skill>()
                .Where(x => x.IsDeleted == false && x.Name.Contains(name))
                .OrderByDescending(x => x.Created)
                .ToListAsync();
        }

        public async Task<List<WorkerSkill>> GetSkillsByWorkerIdAsync(Guid workerId)
        {
            return await _dbContext.Set<WorkerSkill>()
                .Where(ws => ws.WorkerId == workerId)
                .Include(ws => ws.Skill)
                .Where(ws => ws.Skill.IsDeleted == false)
                .OrderByDescending(ws => ws.Skill.Created)
                .ToListAsync();
        }

        public async Task<List<Skill>> GetByIdsAsync(List<Guid> ids)
        {
            return await _dbContext.Set<Skill>()
                .AsNoTracking()
                .Where(x => ids.Contains(x.Id) && !x.IsDeleted)
                .ToListAsync();
        }

    }
}