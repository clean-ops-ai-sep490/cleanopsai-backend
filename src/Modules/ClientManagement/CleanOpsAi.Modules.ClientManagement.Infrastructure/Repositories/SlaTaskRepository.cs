using CleanOpsAi.Modules.ClientManagement.Application.Interfaces;
using CleanOpsAi.Modules.ClientManagement.Domain.Entities;
using CleanOpsAi.Modules.ClientManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanOpsAi.Modules.ClientManagement.Infrastructure.Repositories
{
    public class SlaTaskRepository : ISlaTaskRepository
    {
        private readonly ClientManagementDbContext _dbContext;

        public SlaTaskRepository(ClientManagementDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<SlaTask?> GetByIdAsync(Guid id)
        {
            return await _dbContext.Set<SlaTask>()
                .Include(x => x.Sla)
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
        }

        public async Task<List<SlaTask>> GetAllAsync()
        {
            return await _dbContext.Set<SlaTask>()
                .Include(x => x.Sla)
                .Where(x => x.IsDeleted == false)
                .OrderByDescending(x => x.Id)
                .ToListAsync();
        }

        public async Task<(List<SlaTask> Items, int TotalCount)> GetAllPaginationAsync(int pageNumber, int pageSize)
        {
            var query = _dbContext.Set<SlaTask>()
                .Include(x => x.Sla)
                .Where(x => x.IsDeleted == false)
                .OrderByDescending(x => x.Id);

            var totalCount = await query.CountAsync();

            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        public async Task<List<SlaTask>> GetBySlaIdAsync(Guid slaId)
        {
            return await _dbContext.Set<SlaTask>()
                .Include(x => x.Sla)
                .Where(x => x.SlaId == slaId && x.IsDeleted == false)
                .ToListAsync();
        }

        public async Task<int> CreateAsync(SlaTask slaTask)
        {
            _dbContext.Set<SlaTask>().Add(slaTask);
            return await _dbContext.SaveChangesAsync();
        }

        public async Task<int> UpdateAsync(SlaTask slaTask)
        {
            _dbContext.Set<SlaTask>().Update(slaTask);
            return await _dbContext.SaveChangesAsync();
        }

        public async Task<int> DeleteAsync(Guid id)
        {
            var task = await _dbContext.Set<SlaTask>()
                .FirstOrDefaultAsync(x => x.Id == id);

            if (task != null)
            {
                task.IsDeleted = true;
                _dbContext.Set<SlaTask>().Update(task);
                return await _dbContext.SaveChangesAsync();
            }

            return 0;
        }
    }
}
