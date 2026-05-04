using CleanOpsAi.Modules.Workforce.Application.Interfaces;
using CleanOpsAi.Modules.Workforce.Domain.Entities;
using CleanOpsAi.Modules.Workforce.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CleanOpsAi.Modules.Workforce.Infrastructure.Repositories
{
    public class CertificationRepository : ICertificationRepository
    {
        private readonly WorkforceDbContext _dbContext;

        public CertificationRepository(WorkforceDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Certification?> GetByIdAsync(Guid id)
        {
            return await _dbContext.Set<Certification>()
                .Include(c => c.WorkerCertifications)
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
        }

        public async Task<List<Certification>> GetAllAsync()
        {
            var certifications = await _dbContext.Set<Certification>()
                .Include(c => c.WorkerCertifications)
                .Where(x => x.IsDeleted == false)
                .OrderByDescending(x => x.Created)
                .ToListAsync();

            return certifications;
        }

        public async Task<(List<Certification> Items, int TotalCount)> GetAllPaginationAsync(int pageNumber, int pageSize)
        {
            var query = _dbContext.Set<Certification>()
                .Include(c => c.WorkerCertifications)
                .Where(x => x.IsDeleted == false)
                .OrderByDescending(x => x.Created);

            var totalCount = await query.CountAsync();

            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        public async Task<int> CreateAsync(Certification certification)
        {
            _dbContext.Set<Certification>().Add(certification);
            return await _dbContext.SaveChangesAsync();
        }

        public async Task<int> UpdateAsync(Certification certification)
        {
            _dbContext.Set<Certification>().Update(certification);
            return await _dbContext.SaveChangesAsync();
        }

        public async Task<int> DeleteAsync(Guid id)
        {
            var certification = await _dbContext.Set<Certification>()
                .FirstOrDefaultAsync(x => x.Id == id);

            if (certification != null)
            {
                certification.IsDeleted = true;
                _dbContext.Set<Certification>().Update(certification);
                return await _dbContext.SaveChangesAsync();
            }

            return 0;
        }

        // get all distinct categories
        public async Task<List<string>> GetAllCategoriesAsync()
        {
            return await _dbContext.Set<Certification>()
                .Where(x => !x.IsDeleted)
                .Select(x => x.Category)
                .Distinct()
                .OrderBy(x => x)
                .ToListAsync();
        }

        // get certifications by category
        public async Task<List<Certification>> GetByCategoryAsync(string category)
        {
            return await _dbContext.Set<Certification>()
                .Include(c => c.WorkerCertifications)
                .Where(x => !x.IsDeleted && x.Category.ToLower() == category.ToLower())
                .OrderByDescending(x => x.Created)
                .ToListAsync();
        }

        public async Task<List<WorkerCertification>> GetCertificationsByWorkerIdAsync(Guid workerId)
        {
            return await _dbContext.Set<WorkerCertification>()
                .Include(x => x.Certification)
                .Where(x => x.WorkerId == workerId)
                .OrderByDescending(x => x.IssuedDate)
                .ToListAsync();
        }

        public async Task<List<Certification>> GetByIdsAsync(List<Guid> ids)
        {
            return await _dbContext.Set<Certification>()
                .AsNoTracking()
                .Where(x => ids.Contains(x.Id) && !x.IsDeleted)
                .ToListAsync();
        }

    }
}