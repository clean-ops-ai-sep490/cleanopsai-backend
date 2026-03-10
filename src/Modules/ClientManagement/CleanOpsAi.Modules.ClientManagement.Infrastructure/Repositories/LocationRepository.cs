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
    public class LocationRepository : ILocationRepository
    {
        private readonly ClientManagementDbContext _dbContext;
        public LocationRepository(ClientManagementDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        // get Location by id
        public async Task<Location> GetByIdAsync(Guid id)
        {
            var item = _dbContext.Set<Location>().Include(c => c.Client).FirstOrDefault(c => c.Id == id && c.IsDeleted == false);
            return item;

        }

        // get all Locations
        public async Task<List<Location>> GetAllAsync()
        {
            var items = _dbContext.Set<Location>().Include(c => c.Client).OrderByDescending(c => c.Id).ToList();
            return items;
        }

        // get all Locations with pagination
        public async Task<(List<Location> Items, int TotalCount)> GetAllPaginationAsync(int pageNumber, int pageSize)
        {
            var query = _dbContext.Set<Location>().Include(c => c.Client).AsQueryable().Where(c => c.IsDeleted == false).OrderByDescending(c => c.Id);

            var totalCount = await query.CountAsync();

            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        // add Location
        public async Task<int> CreateAsync(Location location)
        {
            _dbContext.Set<Location>().Add(location);
            return await _dbContext.SaveChangesAsync();
        }

        // update Location
        public async Task<int> UpdateAsync(Location location)
        {
            _dbContext.Set<Location>().Update(location);
            return await _dbContext.SaveChangesAsync();
        }

        // Soft delete Location
        public async Task<int> DeleteAsync(Guid id)
        {
            var item = await _dbContext.Set<Location>()
                .FirstOrDefaultAsync(c => c.Id == id);

            if (item == null)
            {
                return 0;
            }

            item.IsDeleted = true;

            _dbContext.Set<Location>().Update(item);

            return await _dbContext.SaveChangesAsync();
        }
    }
}
