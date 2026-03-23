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

        public async Task<(List<WorkerGps> Items, int TotalCount)> GetAllPaginationAsync(
            int pageNumber, int pageSize)
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

        public async Task<WorkerGps?> GetLatestByWorkerIdAsync(Guid workerId)
        {
            return await _dbContext.Set<WorkerGps>()
                .Include(x => x.Worker)
                .Where(x => x.WorkerId == workerId && x.IsDeleted == false)
                .OrderByDescending(x => x.Created)
                .FirstOrDefaultAsync();
        }

        public async Task<List<WorkerGps>> GetLatestByWorkerIdsAsync(List<Guid> workerIds)
        {
            var allGps = await _dbContext.Set<WorkerGps>()
                .Include(x => x.Worker)
                .Where(x => workerIds.Contains(x.WorkerId) && x.IsDeleted == false)
                .OrderByDescending(x => x.Created)
                .ToListAsync();

            return allGps
                .GroupBy(x => x.WorkerId)
                .Select(g => g.First())
                .ToList();
        }

        public async Task<(List<WorkerGps> Items, int TotalCount)> GetByWorkerIdPaginationAsync(
            Guid workerId, int pageNumber, int pageSize)
        {
            var query = _dbContext.Set<WorkerGps>()
                .Include(x => x.Worker)
                .Where(x => x.WorkerId == workerId && x.IsDeleted == false)
                .OrderByDescending(x => x.Created);

            var totalCount = await query.CountAsync();
            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }
    }
}
