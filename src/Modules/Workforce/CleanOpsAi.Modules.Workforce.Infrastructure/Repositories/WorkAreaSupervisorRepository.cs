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
    public class WorkAreaSupervisorRepository : IWorkAreaSupervisorRepository
    {
        private readonly WorkforceDbContext _dbContext;

        public WorkAreaSupervisorRepository(WorkforceDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<WorkAreaSupervisor?> GetByIdAsync(Guid id)
        {
            return await _dbContext.Set<WorkAreaSupervisor>()
                .Include(x => x.Worker)
                .FirstOrDefaultAsync(x => x.Id == id && x.IsDeleted == false);
        }

        public async Task<WorkAreaSupervisor?> GetByUserIdAsync(string userId)
        {
            return await _dbContext.Set<WorkAreaSupervisor>()
                .Include(x => x.Worker)
                .FirstOrDefaultAsync(x => x.UserId == userId && x.IsDeleted == false);
        }

        public async Task<WorkAreaSupervisor?> GetByWorkerIdAsync(Guid workerId)
        {
            return await _dbContext.Set<WorkAreaSupervisor>()
                .Include(x => x.Worker)
                .FirstOrDefaultAsync(x => x.WorkerId == workerId && x.IsDeleted == false);
        }

        public async Task<List<WorkAreaSupervisor>> GetAllAsync()
        {
            return await _dbContext.Set<WorkAreaSupervisor>()
                .Include(x => x.Worker)
                .Where(x => x.IsDeleted == false)
                .OrderByDescending(x => x.Id)
                .ToListAsync();
        }

        public async Task<(List<WorkAreaSupervisor> Items, int TotalCount)> GetAllPaginationAsync(
            int pageNumber,
            int pageSize)
        {
            var query = _dbContext.Set<WorkAreaSupervisor>()
                .Include(x => x.Worker)
                .Where(x => x.IsDeleted == false)
                .OrderByDescending(x => x.Id);

            var totalCount = await query.CountAsync();

            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        public async Task<List<WorkAreaSupervisor>> GetByWorkAreaIdAsync(Guid workAreaId)
        {
            return await _dbContext.Set<WorkAreaSupervisor>()
                .Include(x => x.Worker)
                .Where(x => x.WorkAreaId == workAreaId && x.IsDeleted == false)
                .OrderByDescending(x => x.Id)
                .ToListAsync();
        }

        public async Task<int> CreateAsync(WorkAreaSupervisor workAreaSupervisor)
        {
            _dbContext.Set<WorkAreaSupervisor>().Add(workAreaSupervisor);
            return await _dbContext.SaveChangesAsync();
        }

        public async Task<int> UpdateAsync(WorkAreaSupervisor workAreaSupervisor)
        {
            _dbContext.Set<WorkAreaSupervisor>().Update(workAreaSupervisor);
            return await _dbContext.SaveChangesAsync();
        }

        public async Task<int> DeleteAsync(Guid id)
        {
            var supervisor = await _dbContext.Set<WorkAreaSupervisor>()
                .FirstOrDefaultAsync(x => x.Id == id);

            if (supervisor != null)
            {
                supervisor.IsDeleted = true;
                _dbContext.Set<WorkAreaSupervisor>().Update(supervisor);
                return await _dbContext.SaveChangesAsync();
            }

            return 0;
        }

        // Lấy GPS mới nhất của tất cả worker trong một WorkArea
        public async Task<List<WorkerGps>> GetWorkersLatestGpsByWorkAreaIdAsync(Guid workAreaId)
        {
            // B1: Lấy danh sách WorkerId trong WorkArea
            var workerIds = await _dbContext.Set<WorkAreaSupervisor>()
                .Where(x => x.WorkAreaId == workAreaId
                         && x.WorkerId != null
                         && x.IsDeleted == false)
                .Select(x => x.WorkerId!.Value)
                .ToListAsync();

            if (!workerIds.Any())
                return new List<WorkerGps>();

            // B2: Lấy GPS mới nhất của từng worker
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
    }
}
