using CleanOpsAi.Modules.Workforce.Application.Interfaces;
using CleanOpsAi.Modules.Workforce.Domain.Entities;
using CleanOpsAi.Modules.Workforce.Infrastructure.Data;
using Microsoft.EntityFrameworkCore; 

namespace CleanOpsAi.Modules.Workforce.Infrastructure.Repositories
{
    public class WorkerCertificationRepository : IWorkerCertificationRepository
    {
        private readonly WorkforceDbContext _dbContext;

        public WorkerCertificationRepository(WorkforceDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<WorkerCertification?> GetByIdAsync(Guid workerId, Guid certificationId)
        {
            return await _dbContext.Set<WorkerCertification>()
                .Include(x => x.Worker)
                .Include(x => x.Certification)
                .FirstOrDefaultAsync(x =>
                    x.WorkerId == workerId &&
                    x.CertificationId == certificationId);
        }

        public async Task<List<WorkerCertification>> GetAllAsync()
        {
            return await _dbContext.Set<WorkerCertification>()
                .Include(x => x.Worker)
                .Include(x => x.Certification)
                .OrderByDescending(x => x.IssuedDate)
                .ToListAsync();
        }

        public async Task<(List<WorkerCertification> Items, int TotalCount)> GetAllPaginationAsync(int pageNumber, int pageSize)
        {
            var query = _dbContext.Set<WorkerCertification>()
                .Include(x => x.Worker)
                .Include(x => x.Certification)
                .OrderByDescending(x => x.IssuedDate);

            var totalCount = await query.CountAsync();

            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        public async Task<int> CreateAsync(WorkerCertification entity)
        {
            _dbContext.Set<WorkerCertification>().Add(entity);
            return await _dbContext.SaveChangesAsync();
        }

        public async Task<int> UpdateAsync(WorkerCertification entity)
        {
            _dbContext.Set<WorkerCertification>().Update(entity);
            return await _dbContext.SaveChangesAsync();
        }

        public async Task<int> DeleteAsync(Guid workerId, Guid certificationId)
        {
            var entity = await _dbContext.Set<WorkerCertification>()
                .FirstOrDefaultAsync(x =>
                    x.WorkerId == workerId &&
                    x.CertificationId == certificationId);

            if (entity != null)
            {
                _dbContext.Set<WorkerCertification>().Remove(entity);
                return await _dbContext.SaveChangesAsync();
            }

            return 0;
        }
    }
}
