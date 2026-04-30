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
    public class SlaShiftRepository : ISlaShiftRepository
    {
        private readonly ClientManagementDbContext _dbContext;

        public SlaShiftRepository(ClientManagementDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<SlaShift?> GetByIdAsync(Guid id)
        {
            return await _dbContext.Set<SlaShift>()
                .Include(x => x.Sla)
                .FirstOrDefaultAsync(x => x.Id == id && x.IsDeleted == false);
        }

        public async Task<List<SlaShift>> GetBySlaIdAsync(Guid slaId)
        {
            return await _dbContext.Set<SlaShift>()
                .Include(x => x.Sla)
                .Where(x => x.SlaId == slaId && x.IsDeleted == false)
                .OrderBy(x => x.StartTime)
                .ToListAsync();
        }

        public async Task<(List<SlaShift>, int)> GetAllPaginationAsync(int pageNumber, int pageSize)
        {
            var query = _dbContext.Set<SlaShift>()
                .Include(x => x.Sla)
                .Where(x => x.IsDeleted == false);

            var total = await query.CountAsync();

            var items = await query
                .OrderByDescending(x => x.Created)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, total);
        }

        public async Task<int> CreateAsync(SlaShift entity)
        {
            _dbContext.Set<SlaShift>().Add(entity);
            return await _dbContext.SaveChangesAsync();
        }

        public async Task<int> UpdateAsync(SlaShift entity)
        {
            _dbContext.Set<SlaShift>().Update(entity);
            return await _dbContext.SaveChangesAsync();
        }

        public async Task<int> DeleteAsync(Guid id)
        {
            var entity = await _dbContext.Set<SlaShift>().FirstOrDefaultAsync(x => x.Id == id);

            if (entity == null)
                return 0;

            entity.IsDeleted = true;

            return await _dbContext.SaveChangesAsync();
        }
    }
}
