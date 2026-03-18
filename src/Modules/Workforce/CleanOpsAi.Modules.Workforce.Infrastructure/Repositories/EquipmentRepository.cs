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
    public class EquipmentRepository : IEquipmentRepository
    {
        private readonly WorkforceDbContext _dbContext;

        public EquipmentRepository(WorkforceDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Equipment?> GetByIdAsync(Guid id)
        {
            return await _dbContext.Set<Equipment>()
                .FirstOrDefaultAsync(x => x.Id == id && x.IsDeleted == false);
        }

        public async Task<List<Equipment>> GetAllAsync()
        {
            return await _dbContext.Set<Equipment>()
                .Where(x => x.IsDeleted == false)
                .OrderByDescending(x => x.Id)
                .ToListAsync();
        }

        public async Task<(List<Equipment> Items, int TotalCount)> GetAllPaginationAsync(int pageNumber, int pageSize)
        {
            var query = _dbContext.Set<Equipment>()
                .Where(x => x.IsDeleted == false)
                .OrderByDescending(x => x.Id);

            var totalCount = await query.CountAsync();

            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        public async Task<int> CreateAsync(Equipment equipment)
        {
            _dbContext.Set<Equipment>().Add(equipment);
            return await _dbContext.SaveChangesAsync();
        }

        public async Task<int> UpdateAsync(Equipment equipment)
        {
            _dbContext.Set<Equipment>().Update(equipment);
            return await _dbContext.SaveChangesAsync();
        }

        public async Task<int> DeleteAsync(Guid id)
        {
            var equipment = await _dbContext.Set<Equipment>()
                .FirstOrDefaultAsync(x => x.Id == id);

            if (equipment != null)
            {
                equipment.IsDeleted = true;
                _dbContext.Set<Equipment>().Update(equipment);
                return await _dbContext.SaveChangesAsync();
            }

            return 0;
        }
    }
}
