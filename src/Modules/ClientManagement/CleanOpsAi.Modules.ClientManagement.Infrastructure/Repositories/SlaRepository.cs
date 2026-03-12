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
    public class SlaRepository : ISlaRepository
    {
        private readonly ClientManagementDbContext _dbContext;

        public SlaRepository(ClientManagementDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Sla> GetByIdAsync(Guid id)
        {
            var sla = _dbContext.Set<Sla>()
                .Include(s => s.WorkArea)
                .Include(s => s.Contract)
                .FirstOrDefault(x => x.Id == id && x.IsDeleted == false);

            return sla;
        }

        public async Task<List<Sla>> GetAllAsync()
        {
            var slas = await _dbContext.Set<Sla>()
                .Include(s => s.WorkArea)
                .Include(s => s.Contract)
                .Where(x => x.IsDeleted == false)
                .OrderByDescending(x => x.Id)
                .ToListAsync();

            return slas;
        }

        public async Task<(List<Sla> Items, int TotalCount)> GetAllPaginationAsync(int pageNumber, int pageSize)
        {
            var query = _dbContext.Set<Sla>()
                .Include(s => s.WorkArea)
                .Include(s => s.Contract)
                .Where(x => x.IsDeleted == false)
                .OrderByDescending(x => x.Id);

            var totalCount = await query.CountAsync();

            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        public async Task<(List<Sla> Items, int TotalCount)> FilterPaginationAsync(
            Guid? workAreaId,
            Guid? contractId,
            int pageNumber,
            int pageSize)
        {
            var query = _dbContext.Set<Sla>()
                .Include(s => s.WorkArea)
                .Include(s => s.Contract)
                .Where(x => x.IsDeleted == false)
                .AsQueryable();

            if (workAreaId.HasValue)
                query = query.Where(x => x.WorkAreaId == workAreaId.Value);

            if (contractId.HasValue)
                query = query.Where(x => x.ContractId == contractId.Value);

            query = query.OrderByDescending(x => x.Id);

            var totalCount = await query.CountAsync();

            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        public async Task<int> CreateAsync(Sla sla)
        {
            _dbContext.Set<Sla>().Add(sla);
            return await _dbContext.SaveChangesAsync();
        }

        public async Task<int> UpdateAsync(Sla sla)
        {
            _dbContext.Set<Sla>().Update(sla);
            return await _dbContext.SaveChangesAsync();
        }

        public async Task<int> DeleteAsync(Guid id)
        {
            var sla = _dbContext.Set<Sla>().FirstOrDefault(x => x.Id == id);

            if (sla != null)
            {
                sla.IsDeleted = true;
                _dbContext.Set<Sla>().Update(sla);
                return await _dbContext.SaveChangesAsync();
            }

            return 0;
        }
    }
}
