using CleanOpsAi.Modules.Workforce.Application.Interfaces;
using CleanOpsAi.Modules.Workforce.Domain.Entities;
using CleanOpsAi.Modules.Workforce.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CleanOpsAi.Modules.Workforce.Infrastructure.Repositories
{
    public class WorkAreaSupervisorRepository : IWorkAreaSupervisorRepository
    {
        private readonly WorkforceDbContext _dbContext;

        public WorkAreaSupervisorRepository(WorkforceDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<WorkAreaSupervisor?> GetByIdAsync(Guid id)
        {
            return await _dbContext.Set<WorkAreaSupervisor>()
                .Include(x => x.Worker)
                .FirstOrDefaultAsync(x => x.Id == id && x.IsDeleted == false);
        }

        public async Task<List<WorkAreaSupervisor>> GetByUserIdAsync(Guid userId)
        {
            return await _dbContext.Set<WorkAreaSupervisor>()
                .Include(x => x.Worker)
                .Where(x => x.UserId == userId && x.IsDeleted == false)
                .OrderByDescending(x => x.Created)
                .ToListAsync();
        }

        public async Task<(List<WorkAreaSupervisor> Items, int TotalCount)> GetByWorkerIdPaginationAsync(
            Guid workerId,
            int pageNumber,
            int pageSize)
        {
            var query = _dbContext.Set<WorkAreaSupervisor>()
                .Include(x => x.Worker)
                .Where(x => x.WorkerId == workerId && x.IsDeleted == false)
                .OrderByDescending(x => x.Created);

            var totalCount = await query.CountAsync();

            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        public async Task<List<WorkAreaSupervisor>> GetAllAsync()
        {
            return await _dbContext.Set<WorkAreaSupervisor>()
                .Include(x => x.Worker)
                .Where(x => x.IsDeleted == false)
                .OrderByDescending(x => x.Id)
                .ToListAsync();
        }

        public async Task<(List<WorkAreaSupervisor> Items, int TotalCount)> GetAllPaginationAsync(
            int pageNumber,
            int pageSize)
        {
            var query = _dbContext.Set<WorkAreaSupervisor>()
                .Include(x => x.Worker)
                .Where(x => x.IsDeleted == false)
                .OrderByDescending(x => x.Id);

            var totalCount = await query.CountAsync();

            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        public async Task<List<WorkAreaSupervisor>> GetByWorkAreaIdAsync(Guid workAreaId)
        {
            return await _dbContext.Set<WorkAreaSupervisor>()
                .Include(x => x.Worker)
                .Where(x => x.WorkAreaId == workAreaId && x.IsDeleted == false)
                .OrderByDescending(x => x.Id)
                .ToListAsync();
        }

        public async Task<int> DeleteAsync(Guid id)
        {
            var supervisorWorkarea = await _dbContext.Set<WorkAreaSupervisor>()
                .FirstOrDefaultAsync(x => x.Id == id);

            if (supervisorWorkarea != null)
            {
                supervisorWorkarea.IsDeleted = true;
                _dbContext.Set<WorkAreaSupervisor>().Update(supervisorWorkarea);
                return await _dbContext.SaveChangesAsync();
            }

            return 0;
        }

        // Lấy GPS mới nhất của tất cả worker trong một WorkArea
        public async Task<List<WorkerGps>> GetLatestGpsByWorkAreaAsync(Guid workAreaId)
        {
            var workerIds = await _dbContext.Set<WorkAreaSupervisor>()
                .Where(x => x.WorkAreaId == workAreaId && x.IsDeleted == false)
                .Select(x => x.WorkerId!.Value)
                .ToListAsync();

            if (!workerIds.Any())
                return new List<WorkerGps>();

            var gps = await _dbContext.Set<WorkerGps>()
                .Include(x => x.Worker)
                .Where(x =>
                    workerIds.Contains(x.WorkerId) &&
                    x.IsDeleted == false)
                .OrderByDescending(x => x.Created)
                .ToListAsync();

            return gps
                .GroupBy(x => x.WorkerId)
                .Select(g => g.First())
                .ToList();
        }

        // Kiểm tra đã tồn tại chưa (tránh duplicate)
        public async Task<bool> ExistsAsync(Guid workAreaId, Guid userId, Guid workerId)
        {
            return await _dbContext.Set<WorkAreaSupervisor>()
                .AnyAsync(x => x.WorkAreaId == workAreaId
                            && x.UserId == userId
                            && x.WorkerId == workerId
                            && x.IsDeleted == false);
        }

        // Bulk create
        public async Task<int> CreateRangeAsync(List<WorkAreaSupervisor> entities)
        {
            _dbContext.Set<WorkAreaSupervisor>().AddRange(entities);
            return await _dbContext.SaveChangesAsync();
        }

        public async Task<WorkAreaSupervisor?> GetByWorkAreaUserWorkerAsync(
            Guid workAreaId, Guid userId, Guid workerId)
        {
            return await _dbContext.Set<WorkAreaSupervisor>()
                .FirstOrDefaultAsync(x => x.WorkAreaId == workAreaId
                                       && x.UserId == userId
                                       && x.WorkerId == workerId
                                       && x.IsDeleted == false);
        }

        // Xoa khi update
        public async Task<int> DeleteByWorkAreaAndSupervisorAsync(Guid workAreaId, Guid supervisorId)
        {
            var entities = await _dbContext.Set<WorkAreaSupervisor>()
                .Where(x => x.WorkAreaId == workAreaId
                         && x.UserId == supervisorId
                         && x.IsDeleted == false)
                .ToListAsync();

            foreach (var entity in entities)
            {
                entity.IsDeleted = true;
                entity.LastModified = DateTime.UtcNow;
            }

            return await _dbContext.SaveChangesAsync();
        }
         
        // get WorkAreaSupervisor by WorkAreaId + WorkerId 
        public async Task<WorkAreaSupervisor?> GetByWorkAreaAndWorkerAsync(Guid workAreaId, Guid workerId)
        {
            return await _dbContext.Set<WorkAreaSupervisor>()
                .Include(x => x.Worker)
                .FirstOrDefaultAsync(x => x.WorkAreaId == workAreaId
                                      && x.WorkerId == workerId
                                      && x.IsDeleted == false);
        }

		public async Task<List<Guid>> GetSupervisorIdsAsync(Guid workAreaId, Guid workerId, CancellationToken ct = default)
		{
			return await _dbContext.Set<WorkAreaSupervisor>()
				.Where(x => x.WorkAreaId == workAreaId && x.WorkerId == workerId)
				.Select(x => x.UserId) 
				.ToListAsync(ct);
		}

        public async Task<List<WorkAreaSupervisor>> GetWorkersBySupervisorIdAsync(Guid supervisorId)
        {
            return await _dbContext.Set<WorkAreaSupervisor>()
                .Include(x => x.Worker)
                .Where(x => x.UserId == supervisorId && x.IsDeleted == false)
                .OrderByDescending(x => x.Created)
                .ToListAsync();
        }

        public async Task<(List<WorkAreaSupervisor> Items, int TotalCount)>GetWorkersByWorkAreaPagingAsync(
            Guid workAreaId,
            int pageNumber,
            int pageSize)
        {
            var query = _dbContext.Set<WorkAreaSupervisor>()
                .Include(x => x.Worker)
                .Where(x =>
                    x.WorkAreaId == workAreaId &&
                    x.IsDeleted == false)
                .OrderByDescending(x => x.Created);

            var totalCount = await query.CountAsync();

            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }

    } 
}
