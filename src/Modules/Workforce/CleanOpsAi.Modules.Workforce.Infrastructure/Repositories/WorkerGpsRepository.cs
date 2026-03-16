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
    public class WorkerGpsRepository : IWorkerGpsRepository
    {
        private readonly WorkforceDbContext _dbContext;

        public WorkerGpsRepository(WorkforceDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<WorkerGps?> GetByIdAsync(Guid id)
        {
            return await _dbContext.Set<WorkerGps>()
                .Include(x => x.Worker)
                .FirstOrDefaultAsync(x => x.Id == id && x.IsDeleted == false);
        }

        public async Task<List<WorkerGps>> GetAllAsync()
        {
            return await _dbContext.Set<WorkerGps>()
                .Include(x => x.Worker)
                .Where(x => x.IsDeleted == false)
                .OrderByDescending(x => x.Created)
                .ToListAsync();
        }

        public async Task<(List<WorkerGps> Items, int TotalCount)> GetAllPaginationAsync(int pageNumber, int pageSize)
        {
            var query = _dbContext.Set<WorkerGps>()
                .Include(x => x.Worker)
                .Where(x => x.IsDeleted == false)
                .OrderByDescending(x => x.Created);

            var totalCount = await query.CountAsync();

            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        public async Task<int> CreateAsync(WorkerGps entity)
        {
            _dbContext.Set<WorkerGps>().Add(entity);
            return await _dbContext.SaveChangesAsync();
        }

        public async Task<int> UpdateAsync(WorkerGps entity)
        {
            _dbContext.Set<WorkerGps>().Update(entity);
            return await _dbContext.SaveChangesAsync();
        }

        public async Task<int> DeleteAsync(Guid id)
        {
            var entity = await _dbContext.Set<WorkerGps>()
                .FirstOrDefaultAsync(x => x.Id == id);

            if (entity != null)
            {
                entity.IsDeleted = true;
                _dbContext.Set<WorkerGps>().Update(entity);
                return await _dbContext.SaveChangesAsync();
            }

            return 0;
        }
    }
}
