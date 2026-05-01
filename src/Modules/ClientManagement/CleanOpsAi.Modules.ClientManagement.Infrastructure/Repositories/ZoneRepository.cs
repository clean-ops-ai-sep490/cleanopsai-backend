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
    public class ZoneRepository : IZoneRepository
    {
        private readonly ClientManagementDbContext _dbContext;

        public ZoneRepository(ClientManagementDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        // Get by Id
        public async Task<Zone?> GetByIdAsync(Guid id)
        {
            return await _dbContext.Set<Zone>()
                .Include(z => z.Location)
                .FirstOrDefaultAsync(z => z.Id == id && z.IsDeleted == false);
        }

        // Get all
        public async Task<List<Zone>> GetAllAsync()
        {
            return await _dbContext.Set<Zone>()
                .Include(z => z.Location)
                .Where(z => z.IsDeleted == false)
                .OrderByDescending(z => z.Created)
                .ToListAsync();
        }

        // Pagination
        public async Task<(List<Zone> Items, int TotalCount)> GetAllPaginationAsync(int pageNumber, int pageSize)
        {
            var query = _dbContext.Set<Zone>()
                .Include(z => z.Location)
                .Where(z => z.IsDeleted == false)
                .OrderByDescending(z => z.Created)
                .AsQueryable();

            var totalCount = await query.CountAsync();

            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        // Get by LocationId
        public async Task<(List<Zone> Items, int TotalCount)> GetByLocationIdPaginationAsync(Guid locationId, int pageNumber, int pageSize)
        {
            var query = _dbContext.Set<Zone>()
                .Include(z => z.Location)
                .Where(z => z.LocationId == locationId && z.IsDeleted == false)
                .OrderByDescending(z => z.Created)
                .AsQueryable();

            var totalCount = await query.CountAsync();

            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        // Create
        public async Task<int> CreateAsync(Zone zone)
        {
            _dbContext.Set<Zone>().Add(zone);
            return await _dbContext.SaveChangesAsync();
        }

        // Update
        public async Task<int> UpdateAsync(Zone zone)
        {
            _dbContext.Set<Zone>().Update(zone);
            return await _dbContext.SaveChangesAsync();
        }

        // Soft delete
        public async Task<int> DeleteAsync(Guid id)
        {
            var zone = await _dbContext.Set<Zone>()
                .FirstOrDefaultAsync(z => z.Id == id);

            if (zone == null)
                return 0;

            zone.IsDeleted = true;

            _dbContext.Set<Zone>().Update(zone);

            return await _dbContext.SaveChangesAsync();
        }
    }
}
