using CleanOpsAi.Modules.Workforce.Application.Dtos.Workers;
using CleanOpsAi.Modules.Workforce.Application.Interfaces;
using CleanOpsAi.Modules.Workforce.Domain.Entities;
using CleanOpsAi.Modules.Workforce.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Text;

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

        public async Task<Worker?> GetByUserIdAsync(Guid userId)
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
                .Include(x => x.WorkerSkills).ThenInclude(ws => ws.Skill)
                .Include(x => x.WorkerCertifications).ThenInclude(wc => wc.Certification)
                .Where(x => !x.IsDeleted);

            // =========================
            // SKILL FILTER
            // =========================
            if (request.SkillCategories?.Any() == true)
            {
                query = query.Where(x =>
                    x.WorkerSkills.Any(s =>
                        request.SkillCategories.Contains(s.Skill.Category)));
            }

            // =========================
            // CERT FILTER
            // =========================
            if (request.CertificateCategories?.Any() == true)
            {
                query = query.Where(x =>
                    x.WorkerCertifications.Any(c =>
                        request.CertificateCategories.Contains(c.Certification.Category)));
            }

            // =========================
            // ADDRESS FILTER (TEXT ONLY - KHÔNG DROP DATA)
            // =========================
            var workers = await query.ToListAsync();

            if (!string.IsNullOrWhiteSpace(request.Address))
            {
                var keyword = request.Address.Trim().ToLowerInvariant();

                workers = workers
                    .OrderByDescending(x =>
                        (!string.IsNullOrEmpty(x.DisplayAddress) &&
                         x.DisplayAddress.ToLowerInvariant().Contains(keyword)))
                    .ToList();
            }

            // =========================
            // DISTANCE SORT (KHÔNG FILTER NULL LAT/LNG)
            // =========================
            if (request.Latitude.HasValue && request.Longitude.HasValue)
            {
                double lat = request.Latitude.Value;
                double lon = request.Longitude.Value;

                workers = workers
                    .OrderBy(x =>
                    {
                        if (!x.Latitude.HasValue || !x.Longitude.HasValue)
                            return double.MaxValue; // đẩy xuống cuối

                        return DistanceKm(x.Latitude.Value, x.Longitude.Value, lat, lon);
                    })
                    .ToList();
            }

            return workers;
        }

        public async Task<List<Worker>> FilterStrictAsync(WorkerFilterRequest request)
        {
            var query = _dbContext.Set<Worker>()
                .Include(x => x.WorkerSkills).ThenInclude(s => s.Skill)
                .Include(x => x.WorkerCertifications).ThenInclude(c => c.Certification)
                .Where(x => !x.IsDeleted);

            // SKILL FILTER
            if (request.SkillCategories?.Any() == true)
            {
                query = query.Where(x =>
                    x.WorkerSkills.Any(s =>
                        request.SkillCategories.Contains(s.Skill.Category)));
            }

            // CERT FILTER
            if (request.CertificateCategories?.Any() == true)
            {
                query = query.Where(x =>
                    x.WorkerCertifications.Any(c =>
                        request.CertificateCategories.Contains(c.Certification.Category)));
            }

            // LOCATION FILTER
            if (request.Latitude.HasValue && request.Longitude.HasValue)
            {
                double lat = request.Latitude.Value;
                double lon = request.Longitude.Value;
                const double boundingBox = 0.3;

                query = query.Where(x =>
                    x.Latitude.HasValue &&
                    x.Longitude.HasValue &&
                    Math.Abs(x.Latitude.Value - lat) <= boundingBox &&
                    Math.Abs(x.Longitude.Value - lon) <= boundingBox);
            }

            var workers = await query.ToListAsync();

            // =========================
            // SEARCH FIX (QUAN TRỌNG)
            // =========================
            if (!string.IsNullOrWhiteSpace(request.Address))
            {
                var keyword = request.Address.Trim().ToLowerInvariant();

                workers = workers
                    .Where(x =>
                        !string.IsNullOrEmpty(x.DisplayAddress) &&
                        x.DisplayAddress.ToLowerInvariant().Contains(keyword))
                    .ToList();
            }

            // SORT
            if (request.Latitude.HasValue && request.Longitude.HasValue)
            {
                double lat = request.Latitude.Value;
                double lon = request.Longitude.Value;

                workers = workers
                    .OrderBy(x => DistanceKm(x.Latitude ?? 0, x.Longitude ?? 0, lat, lon))
                    .ToList();
            }

            return workers;
        }

        public static double DistanceKm(double lat1, double lon1, double lat2, double lon2)
        {
            const double R = 6371;

            var dLat = ToRad(lat2 - lat1);
            var dLon = ToRad(lon2 - lon1);

            var a =
                Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(ToRad(lat1)) * Math.Cos(ToRad(lat2)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            return R * c;
        }

        private static double ToRad(double angle) => Math.PI * angle / 180;

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
