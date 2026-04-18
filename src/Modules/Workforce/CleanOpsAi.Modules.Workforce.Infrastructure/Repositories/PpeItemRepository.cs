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
    public class PpeItemRepository : IPpeItemRepository
    {
        private readonly WorkforceDbContext _dbContext;

        public PpeItemRepository(WorkforceDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<PpeItem?> GetByIdAsync(Guid id)
        {
            return await _dbContext.Set<PpeItem>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
        }

        public async Task<List<PpeItem>> GetAllAsync()
        {
            var items = await _dbContext.Set<PpeItem>()
                .Where(x => !x.IsDeleted)
                .OrderByDescending(x => x.Created)
                .ToListAsync();

            return items;
        }

        public async Task<(List<PpeItem> Items, int TotalCount)> GetAllPaginationAsync(int pageNumber, int pageSize)
        {
            var query = _dbContext.Set<PpeItem>()
                .Where(x => !x.IsDeleted)
                .OrderByDescending(x => x.Created);

            var totalCount = await query.CountAsync();

            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        public async Task<int> CreateAsync(PpeItem entity)
        {
            _dbContext.Set<PpeItem>().Add(entity);
            return await _dbContext.SaveChangesAsync();
        }

        public async Task<int> UpdateAsync(PpeItem entity)
        {
            _dbContext.Set<PpeItem>().Update(entity);
            return await _dbContext.SaveChangesAsync();
        }

        public async Task<int> DeleteAsync(Guid id)
        {
            var entity = await _dbContext.Set<PpeItem>()
                .FirstOrDefaultAsync(x => x.Id == id);

            if (entity != null)
            {
                entity.IsDeleted = true;
                _dbContext.Set<PpeItem>().Update(entity);
                return await _dbContext.SaveChangesAsync();
            }

            return 0;
        }

        //  filter theo loại PPE
        public async Task<List<PpeItem>> GetByActionKeyAsync(string actionKey)
        {
            return await _dbContext.Set<PpeItem>()
                .Where(x => !x.IsDeleted && x.ActionKey.ToLower() == actionKey.ToLower())
                .OrderByDescending(x => x.Created)
                .ToListAsync();
        }

        //  lấy danh sách loại PPE (distinct)
        public async Task<List<string>> GetAllActionKeysAsync()
        {
            return await _dbContext.Set<PpeItem>()
                .Where(x => !x.IsDeleted)
                .Select(x => x.ActionKey)
                .Distinct()
                .OrderBy(x => x)
                .ToListAsync();
        }
    }
}
