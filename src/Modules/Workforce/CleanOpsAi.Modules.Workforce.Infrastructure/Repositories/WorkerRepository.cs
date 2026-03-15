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
    public class WorkerRepository : IWorkerRepository
    {
        private readonly WorkforceDbContext _dbContext;

        public WorkerRepository(WorkforceDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Worker?> GetByIdAsync(Guid id)
        {
            var worker = await _dbContext.Set<Worker>()
                .Include(x => x.WorkerSkills)
                .Include(x => x.WorkerCertifications)
                .Include(x => x.WorkerGps)
                .FirstOrDefaultAsync(x => x.Id == id && x.IsDeleted == false);

            return worker;
        }

        public async Task<Worker?> GetByUserIdAsync(string userId)
        {
            var worker = await _dbContext.Set<Worker>()
                .Include(x => x.WorkerSkills)
                .Include(x => x.WorkerCertifications)
                .Include(x => x.WorkerGps)
                .FirstOrDefaultAsync(x => x.UserId == userId && x.IsDeleted == false);

            return worker;
        }

        public async Task<List<Worker>> GetAllAsync()
        {
            var workers = await _dbContext.Set<Worker>()
                .Where(x => x.IsDeleted == false)
                .OrderByDescending(x => x.Id)
                .ToListAsync();

            return workers;
        }

        public async Task<(List<Worker> Items, int TotalCount)> GetAllPaginationAsync(
            int pageNumber,
            int pageSize)
        {
            var query = _dbContext.Set<Worker>()
                .Where(x => x.IsDeleted == false)
                .OrderByDescending(x => x.Id);

            var totalCount = await query.CountAsync();

            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        public async Task<int> CreateAsync(Worker worker)
        {
            _dbContext.Set<Worker>().Add(worker);
            return await _dbContext.SaveChangesAsync();
        }

        public async Task<int> UpdateAsync(Worker worker)
        {
            _dbContext.Set<Worker>().Update(worker);
            return await _dbContext.SaveChangesAsync();
        }

        public async Task<int> DeleteAsync(Guid id)
        {
            var worker = await _dbContext.Set<Worker>()
                .FirstOrDefaultAsync(x => x.Id == id);

            if (worker != null)
            {
                worker.IsDeleted = true;

                _dbContext.Set<Worker>().Update(worker);

                return await _dbContext.SaveChangesAsync();
            }

            return 0;
        }
    }
}
