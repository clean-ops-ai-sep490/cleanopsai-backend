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
                .OrderByDescending(x => x.Created)
                .ToListAsync();
        }

        public async Task<(List<Equipment> Items, int TotalCount)> GetAllPaginationAsync(int pageNumber, int pageSize)
        {
            var query = _dbContext.Set<Equipment>()
                .Where(x => x.IsDeleted == false)
                .OrderByDescending(x => x.Created);

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

        public async Task<(List<Equipment> Items, int TotalCount)> SearchPaginationAsync(string? keyword, int pageNumber, int pageSize)
        {
            var query = _dbContext.Set<Equipment>()
                .Where(x => !x.IsDeleted);

            var list = await query.ToListAsync();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                keyword = keyword.Trim().ToLower();
                var keywordNormalized = RemoveDiacritics(keyword);

                list = list.Where(x =>
                    RemoveDiacritics(x.Name.ToLower())
                        .Contains(keywordNormalized)
                ).ToList();
            }

            var totalCount = list.Count;

            var items = list
                .OrderByDescending(x => x.Created)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return (items, totalCount);
        }

        public async Task<List<Equipment>> GetByIdsAsync(List<Guid> ids)
        {
            return await _dbContext.Set<Equipment>()
                .Where(x => ids.Contains(x.Id) && !x.IsDeleted)
                .ToListAsync();
        }

        private static string RemoveDiacritics(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return text;

            var normalized = text.Normalize(System.Text.NormalizationForm.FormD);

            var chars = normalized
                .Where(c => System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c)
                            != System.Globalization.UnicodeCategory.NonSpacingMark)
                .ToArray();

            return new string(chars).Normalize(System.Text.NormalizationForm.FormC);
        }
    }
}