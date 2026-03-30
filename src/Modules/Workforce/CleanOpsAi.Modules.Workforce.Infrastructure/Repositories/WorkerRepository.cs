using CleanOpsAi.Modules.Workforce.Application.Dtos.Workers;
using CleanOpsAi.Modules.Workforce.Application.Interfaces;
using CleanOpsAi.Modules.Workforce.Domain.Entities;
using CleanOpsAi.Modules.Workforce.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CleanOpsAi.Modules.Workforce.Infrastructure.Repositories
{
    public class WorkerRepository : IWorkerRepository
    {
        private readonly WorkforceDbContext _dbContext;

        public WorkerRepository(WorkforceDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Worker?> GetByIdAsync(Guid id)
        {
            var worker = await _dbContext.Set<Worker>()
                .Include(x => x.WorkerSkills)
                .Include(x => x.WorkerCertifications)
                .Include(x => x.WorkerGps)
                .FirstOrDefaultAsync(x => x.Id == id && x.IsDeleted == false);

            return worker;
        }

        public async Task<Worker?> GetByUserIdAsync(string userId)
        {
            var worker = await _dbContext.Set<Worker>()
                .Include(x => x.WorkerSkills)
                .Include(x => x.WorkerCertifications)
                .Include(x => x.WorkerGps)
                .FirstOrDefaultAsync(x => x.UserId == userId && x.IsDeleted == false);

            return worker;
        }

        public async Task<List<Worker>> GetAllAsync()
        {
            var workers = await _dbContext.Set<Worker>()
                .Where(x => x.IsDeleted == false)
                .OrderByDescending(x => x.Id)
                .ToListAsync();

            return workers;
        }

        public async Task<(List<Worker> Items, int TotalCount)> GetAllPaginationAsync(
            int pageNumber,
            int pageSize)
        {
            var query = _dbContext.Set<Worker>()
                .Where(x => x.IsDeleted == false)
                .OrderByDescending(x => x.Id);

            var totalCount = await query.CountAsync();

            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        public async Task<int> CreateAsync(Worker worker)
        {
            _dbContext.Set<Worker>().Add(worker);
            return await _dbContext.SaveChangesAsync();
        }

        public async Task<int> UpdateAsync(Worker worker)
        {
            _dbContext.Set<Worker>().Update(worker);
            return await _dbContext.SaveChangesAsync();
        }

        public async Task<int> DeleteAsync(Guid id)
        {
            var worker = await _dbContext.Set<Worker>()
                .FirstOrDefaultAsync(x => x.Id == id);

            if (worker != null)
            {
                worker.IsDeleted = true;

                _dbContext.Set<Worker>().Update(worker);

                return await _dbContext.SaveChangesAsync();
            }

            return 0;
        }

        public async Task<List<Worker>> FilterAsync(WorkerFilterRequest request)
        {
            var query = _dbContext.Set<Worker>()
                .Include(x => x.WorkerSkills)
                    .ThenInclude(ws => ws.Skill)
                .Include(x => x.WorkerCertifications)
                    .ThenInclude(wc => wc.Certification)
                .Where(x => !x.IsDeleted);

            double lat = request.Latitude ?? 0;
            double lon = request.Longitude ?? 0;

            var result = await query
                .Select(x => new
                {
                    Worker = x,

                    MatchCertificate =
                        (request.CertificateCategories != null && request.CertificateCategories.Any())
                        ? x.WorkerCertifications.Any(c =>
                            request.CertificateCategories.Contains(c.Certification.Category)) ? 1 : 0
                        : 0,

                    MatchSkill =
                        (request.SkillCategories != null && request.SkillCategories.Any())
                        ? x.WorkerSkills.Any(s =>
                            request.SkillCategories.Contains(s.Skill.Category)) ? 1 : 0
                        : 0,

                    Distance =
                        (request.Latitude.HasValue && request.Longitude.HasValue)
                        ? Math.Pow((x.Latitude ?? 0) - lat, 2) +
                          Math.Pow((x.Longitude ?? 0) - lon, 2)
                        : 0
                })
                .OrderByDescending(x => x.MatchCertificate) // ưu tiên cert
                .ThenByDescending(x => x.MatchSkill)        // rồi skill
                .ThenBy(x => x.Distance)                    // rồi gần nhất
                .Select(x => x.Worker)
                .ToListAsync();

            return result;
        }

		public Task<List<Worker>> GetWorkersByIds(List<Guid> ids)
		{
			return _dbContext.Set<Worker>()
				.Where(x => ids.Contains(x.Id))
				.ToListAsync();
		}

		public async Task<List<Guid>> GetWorkersWithAllSkillsAndCertsAsync(List<Guid> workerIds, List<Guid> requiredSkillIds, List<Guid> requiredCertIds, CancellationToken ct)
		{
			var query = _dbContext.Set<Worker>().AsQueryable();

			if (workerIds.Any())
				query = query.Where(w => workerIds.Contains(w.Id));

			if (requiredSkillIds.Any())
				query = query.Where(w =>
					requiredSkillIds.All(skillId =>
						w.WorkerSkills.Any(ws => ws.SkillId == skillId)));

			if (requiredCertIds.Any())
				query = query.Where(w =>
					requiredCertIds.All(certId =>
						w.WorkerCertifications.Any(wc =>
							wc.CertificationId == certId &&
							wc.ExpiredAt > DateTime.UtcNow)));

			return await query.Select(w => w.Id).ToListAsync(ct);
		}

		public async Task<List<Guid>> GetQualifiedWorkersAsync(List<Guid> requiredSkillIds, List<Guid> requiredCertificationIds, CancellationToken ct = default)
		{
			var query = _dbContext.Set<Worker>().AsNoTracking().AsQueryable();

			// Filter theo skills (All skills required)
			if (requiredSkillIds.Any())
			{
				query = query.Where(w => requiredSkillIds.All(skillId =>
					w.WorkerSkills.Any(ws => ws.SkillId == skillId)));
			}

			// Filter theo certifications (All certs required + not expired)
			if (requiredCertificationIds.Any())
			{
				query = query.Where(w => requiredCertificationIds.All(certId =>
					w.WorkerCertifications.Any(wc =>
						wc.CertificationId == certId &&
						wc.ExpiredAt > DateTime.UtcNow)));
			}

			return await query
				.Select(w => w.Id)
				.ToListAsync(ct);
		}

		public async Task<bool> IsWorkerQualifiedAsync(Guid workerId, List<Guid> requiredSkillIds, List<Guid> requiredCertificationIds, CancellationToken ct = default)
		{
			var query = _dbContext.Set<Worker>()
		.AsNoTracking()
		.Where(w => w.Id == workerId);

			if (requiredSkillIds.Any())
			{
				query = query.Where(w => requiredSkillIds.All(skillId =>
					w.WorkerSkills.Any(ws => ws.SkillId == skillId)));
			}

			if (requiredCertificationIds.Any())
			{
				query = query.Where(w => requiredCertificationIds.All(certId =>
					w.WorkerCertifications.Any(wc =>
						wc.CertificationId == certId &&
						wc.ExpiredAt > DateTime.UtcNow)));
			}

			// Chỉ cần kiểm tra tồn tại
			return await query.AnyAsync(ct);
		}
	}
}
