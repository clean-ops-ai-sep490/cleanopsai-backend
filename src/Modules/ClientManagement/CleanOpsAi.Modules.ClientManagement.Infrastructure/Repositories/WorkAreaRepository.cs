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
    public class WorkAreaRepository : IWorkAreaRepository
    {
        private readonly ClientManagementDbContext _dbContext;

        public WorkAreaRepository(ClientManagementDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<WorkArea?> GetByIdAsync(Guid id)
        {
            return await _dbContext.Set<WorkArea>()
                .Include(w => w.Zone)
                .FirstOrDefaultAsync(w => w.Id == id && w.IsDeleted == false);
        }

        public async Task<List<WorkArea>> GetAllAsync()
        {
            return await _dbContext.Set<WorkArea>()
                .Include(w => w.Zone)
                .Where(w => w.IsDeleted == false)
                .OrderByDescending(w => w.Created)
                .ToListAsync();
        }

        public async Task<(List<WorkArea> Items, int TotalCount)> GetAllPaginationAsync(int pageNumber, int pageSize)
        {
            var query = _dbContext.Set<WorkArea>()
                .Include(w => w.Zone)
                .Where(w => w.IsDeleted == false)
                .OrderByDescending(w => w.Created)
                .AsQueryable();

            var totalCount = await query.CountAsync();

            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        public async Task<(List<WorkArea> Items, int TotalCount)> GetByZoneIdPaginationAsync(Guid zoneId, int pageNumber, int pageSize)
        {
            var query = _dbContext.Set<WorkArea>()
                .Include(w => w.Zone)
                .Where(w => w.ZoneId == zoneId && w.IsDeleted == false)
                .OrderByDescending(w => w.Created)
                .AsQueryable();

            var totalCount = await query.CountAsync();

            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        public async Task<int> CreateAsync(WorkArea workArea)
        {
            _dbContext.Set<WorkArea>().Add(workArea);
            return await _dbContext.SaveChangesAsync();
        }

        public async Task<int> UpdateAsync(WorkArea workArea)
        {
            _dbContext.Set<WorkArea>().Update(workArea);
            return await _dbContext.SaveChangesAsync();
        }

        public async Task<int> DeleteAsync(Guid id)
        {
            var workArea = await _dbContext.Set<WorkArea>()
                .FirstOrDefaultAsync(w => w.Id == id);

            if (workArea == null)
                return 0;

            workArea.IsDeleted = true;

            _dbContext.Set<WorkArea>().Update(workArea);

            return await _dbContext.SaveChangesAsync();
        }
    }
}
