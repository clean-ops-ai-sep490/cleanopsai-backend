using CleanOpsAi.Modules.Workforce.Application.Dtos.Workers;
using CleanOpsAi.Modules.Workforce.Application.Interfaces;
using CleanOpsAi.Modules.Workforce.Application.Services;
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
                .Include(x => x.WorkerSkills)
                .Include(x => x.WorkerCertifications)
                .Where(x => x.IsDeleted == false)
                .OrderByDescending(x => x.Created)
                .ToListAsync();

            return workers;
        }

        public async Task<int> CountAllActiveAsync()
        {
            return await _dbContext.Set<Worker>()
                .CountAsync(x => x.IsDeleted == false);
        }

        public async Task<(List<Worker> Items, int TotalCount)> GetAllPaginationAsync(
            int pageNumber,
            int pageSize)
        {
            var query = _dbContext.Set<Worker>()
                .Include(x => x.WorkerSkills)
                .Include(x => x.WorkerCertifications)
                .Where(x => x.IsDeleted == false)
                .OrderByDescending(x => x.Created);

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
            // =========================
            // 1. PREPARE DATA
            // =========================
            var skillIds = request.SkillIds ?? new List<Guid>();
            var certIds = request.CertificateIds ?? new List<Guid>();

            var keyword = WorkerNlpLocalParser.NormalizeSearchText(request.Address);

            bool hasLocation =
                request.Latitude.HasValue &&
                request.Longitude.HasValue;

            double reqLat = request.Latitude ?? 0;
            double reqLon = request.Longitude ?? 0;
            var now = DateTime.UtcNow;

            // =========================
            // 2. SQL FILTER FIRST
            // =========================
            var query = _dbContext.Set<Worker>()
                .AsNoTracking()
                .Where(x => !x.IsDeleted);

            foreach (var skillId in skillIds.Distinct())
            {
                query = query.Where(w =>
                    w.WorkerSkills.Any(ws => ws.SkillId == skillId));
            }

            foreach (var certId in certIds.Distinct())
            {
                query = query.Where(w =>
                    w.WorkerCertifications.Any(wc =>
                        wc.CertificationId == certId &&
                        (!wc.ExpiredAt.HasValue || wc.ExpiredAt.Value > now)));
            }

            var workers = await query
                .Include(x => x.WorkerSkills)
                    .ThenInclude(ws => ws.Skill)
                .Include(x => x.WorkerCertifications)
                    .ThenInclude(wc => wc.Certification)
                .ToListAsync();

            // =========================
            // 3. FILTER + RANKING + SORT
            // =========================
            var result = workers
                .Where(w =>
                    string.IsNullOrWhiteSpace(keyword) ||
                    hasLocation ||
                    WorkerNlpLocalParser.NormalizeSearchText(w.DisplayAddress).Contains(keyword))
                .Select(w => new
                {
                    Worker = w,

                    // SCORE
                    Score =
                        w.WorkerSkills.Count(ws =>
                            skillIds.Contains(ws.SkillId))
                        +
                        w.WorkerCertifications.Count(wc =>
                            certIds.Contains(wc.CertificationId) &&
                            (!wc.ExpiredAt.HasValue || wc.ExpiredAt.Value > now)),

                    // ADDRESS MATCH
                    AddressMatch =
                        !string.IsNullOrEmpty(keyword) &&
                        !string.IsNullOrEmpty(w.DisplayAddress) &&
                        WorkerNlpLocalParser.NormalizeSearchText(w.DisplayAddress).Contains(keyword),

                    // DISTANCE
                    Distance =
                        (hasLocation &&
                         w.Latitude.HasValue &&
                         w.Longitude.HasValue)
                            ? DistanceKm(
                                w.Latitude.Value,
                                w.Longitude.Value,
                                reqLat,
                                reqLon)
                            : double.MaxValue
                })
                // =========================
                // SORT
                // =========================
                .OrderByDescending(x => x.Score)
                .ThenByDescending(x => x.AddressMatch)
                .ThenBy(x => x.Distance)
                .Select(x => x.Worker)
                .ToList();

            return result;
        }

        public async Task<List<Worker>> FilterStrictAsync(WorkerFilterRequest request)
        {
            var skillIds = request.SkillIds ?? new List<Guid>();
            var certIds = request.CertificateIds ?? new List<Guid>();
            var keyword = WorkerNlpLocalParser.NormalizeSearchText(request.Address);

            bool hasLocation = request.Latitude.HasValue && request.Longitude.HasValue;
            double reqLat = request.Latitude ?? 0;
            double reqLon = request.Longitude ?? 0;
            const double RadiusKm = 15;
            var now = DateTime.UtcNow;

            var query = _dbContext.Set<Worker>()
                .AsNoTracking()
                .Where(x => !x.IsDeleted);

            foreach (var skillId in skillIds.Distinct())
            {
                query = query.Where(w => w.WorkerSkills.Any(ws => ws.SkillId == skillId));
            }

            foreach (var certId in certIds.Distinct())
            {
                query = query.Where(w =>
                    w.WorkerCertifications.Any(wc =>
                        wc.CertificationId == certId &&
                        (!wc.ExpiredAt.HasValue || wc.ExpiredAt.Value > now)));
            }

            if (hasLocation)
            {
                var latDelta = RadiusKm / 111.0;
                var lonDelta = RadiusKm / (111.0 * Math.Cos(reqLat * Math.PI / 180.0));

                query = query.Where(w =>
                    w.Latitude.HasValue &&
                    w.Longitude.HasValue &&
                    w.Latitude >= reqLat - latDelta &&
                    w.Latitude <= reqLat + latDelta &&
                    w.Longitude >= reqLon - lonDelta &&
                    w.Longitude <= reqLon + lonDelta);
            }

            //var workers = await query
            //    .Include(x => x.WorkerSkills)
            //        .ThenInclude(s => s.Skill)
            //    .Include(x => x.WorkerCertifications)
            //        .ThenInclude(c => c.Certification)
            //    .ToListAsync();
            var workers = await query
            .AsSplitQuery()
            .Select(x => new Worker
            {
                Id = x.Id,
                UserId = x.UserId,
                FullName = x.FullName,
                DisplayAddress = x.DisplayAddress,
                Latitude = x.Latitude,
                Longitude = x.Longitude,
                AvatarUrl = x.AvatarUrl,

                WorkerSkills = x.WorkerSkills,
                WorkerCertifications = x.WorkerCertifications
            })
            .Take(100)
            .ToListAsync();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                workers = workers
                    .Where(w => !string.IsNullOrWhiteSpace(w.DisplayAddress) && MatchAddressKeyword(w.DisplayAddress, keyword))
                    .ToList();
            }

            var rankedWorkers = workers
                .Select(w => new
                {
                    Worker = w,
                    Distance = hasLocation && w.Latitude.HasValue && w.Longitude.HasValue
                        ? DistanceKm(w.Latitude.Value, w.Longitude.Value, reqLat, reqLon)
                        : double.MaxValue
                })
                .Where(x => !hasLocation || x.Distance <= RadiusKm)
                .OrderBy(x => x.Distance)
                .Select(x => x.Worker)
                .ToList();

            return rankedWorkers;
        }

        private static string Normalize(string input)
        {
            return input?.Trim().ToLowerInvariant() ?? "";
        }

        private static bool MatchText(string source, string keyword)
        {
            var s = Normalize(source);
            var k = Normalize(keyword);

            return s.Contains(k) || k.Contains(s);
        }

        private static bool MatchAddressKeyword(string source, string keyword)
        {
            var s = WorkerNlpLocalParser.NormalizeSearchText(source);
            var k = WorkerNlpLocalParser.NormalizeSearchText(keyword);

            if (string.IsNullOrWhiteSpace(s) || string.IsNullOrWhiteSpace(k))
                return false;

            if (s.Contains(k))
                return true;

            var parts = k.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            return parts.Length > 0 && parts.All(part => s.Contains(part));
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

		public Task<List<Worker>> GetWorkersByUserIds(List<Guid> userIds)
		{
			return _dbContext.Set<Worker>()
				.Where(x => userIds.Contains(x.UserId) && x.IsDeleted == false)
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
