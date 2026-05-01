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
    public class WorkAreaDetailRepository : IWorkAreaDetailRepository
    {
        private readonly ClientManagementDbContext _dbContext;

        public WorkAreaDetailRepository(ClientManagementDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<WorkAreaDetail?> GetByIdAsync(Guid id)
        {
            return await _dbContext.Set<WorkAreaDetail>()
                .Include(x => x.WorkArea)
                .FirstOrDefaultAsync(x => x.Id == id && x.IsDeleted == false);
        }

        public async Task<(List<WorkAreaDetail>, int)> GetAllPaginationAsync(int pageNumber, int pageSize)
        {
            var query = _dbContext.Set<WorkAreaDetail>()
                .Include(x => x.WorkArea)
                .Where(x => !x.IsDeleted);

            var total = await query.CountAsync();

            var data = await query
                .OrderByDescending(x => x.Created)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (data, total);
        }

        public async Task<(List<WorkAreaDetail>, int)> GetByWorkAreaIdPaginationAsync(Guid workAreaId, int pageNumber, int pageSize)
        {
            var query = _dbContext.Set<WorkAreaDetail>()
                .Include(x => x.WorkArea)
                .Where(x => x.WorkAreaId == workAreaId && !x.IsDeleted)
                .OrderByDescending(x => x.Created);

            var total = await query.CountAsync();

            var data = await query
                .OrderByDescending(x => x.Created)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (data, total);
        }

        public async Task<WorkAreaDetail> CreateAsync(WorkAreaDetail entity)
        {
            _dbContext.Add(entity);
            await _dbContext.SaveChangesAsync();
            return entity;
        }

        public async Task<int> UpdateAsync(WorkAreaDetail entity)
        {
            _dbContext.Update(entity);
            return await _dbContext.SaveChangesAsync();
        }

        public async Task<int> DeleteAsync(WorkAreaDetail entity)
        {
            entity.IsDeleted = true;
            _dbContext.Update(entity);
            return await _dbContext.SaveChangesAsync();
        }
    }
}
