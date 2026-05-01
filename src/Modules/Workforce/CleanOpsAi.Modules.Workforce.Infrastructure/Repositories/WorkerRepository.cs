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
            // 1. BASE QUERY
            // =========================
            var workers = await _dbContext.Set<Worker>()
                .Include(x => x.WorkerSkills)
                    .ThenInclude(ws => ws.Skill)
                .Include(x => x.WorkerCertifications)
                    .ThenInclude(wc => wc.Certification)
                .Where(x => !x.IsDeleted)
                .ToListAsync();

            // =========================
            // 2. PREPARE DATA
            // =========================
            var skillCategories = request.SkillCategories ?? new List<string>();
            var certCategories = request.CertificateCategories ?? new List<string>();
            var keyword = request.Address?.Trim().ToLowerInvariant();

            bool hasLocation = request.Latitude.HasValue && request.Longitude.HasValue;
            double reqLat = request.Latitude ?? 0;
            double reqLon = request.Longitude ?? 0;

            // =========================
            // 3. RANKING + SORT
            // =========================
            var result = workers
                .Select(w => new
                {
                    Worker = w,

                    // 🔥 SCORE (skill + cert)
                    Score =
                        // skill score
                        (skillCategories.Count > 0
                            ? w.WorkerSkills?.Count(s =>
                                s.Skill != null &&
                                s.Skill.Category != null &&
                                skillCategories.Contains(s.Skill.Category))
                            : 0)
                        +
                        // cert score
                        (certCategories.Count > 0
                            ? w.WorkerCertifications?.Count(c =>
                                c.Certification != null &&
                                c.Certification.Category != null &&
                                certCategories.Contains(c.Certification.Category))
                            : 0),

                    // 📍 ADDRESS MATCH
                    AddressMatch =
                        !string.IsNullOrEmpty(keyword) &&
                        !string.IsNullOrEmpty(w.DisplayAddress) &&
                        w.DisplayAddress.ToLowerInvariant().Contains(keyword),

                    // 📏 DISTANCE
                    Distance =
                        (hasLocation && w.Latitude.HasValue && w.Longitude.HasValue)
                            ? DistanceKm(w.Latitude.Value, w.Longitude.Value, reqLat, reqLon)
                            : double.MaxValue
                })
                // =========================
                // SORT PRIORITY
                // =========================
                .OrderByDescending(x => x.Score)          // ưu tiên skill + cert
                .ThenByDescending(x => x.AddressMatch)    // ưu tiên address match
                .ThenBy(x => x.Distance)                  // gần hơn lên trước
                .Select(x => x.Worker)
                .ToList();

            return result;
        }

        public async Task<List<Worker>> FilterStrictAsync(WorkerFilterRequest request)
        {
            var skillCategories = request.SkillCategories ?? new List<string>();
            var certCategories = request.CertificateCategories ?? new List<string>();
            var keyword = Normalize(request.Address);

            bool hasLocation = request.Latitude.HasValue && request.Longitude.HasValue;
            double reqLat = request.Latitude ?? 0;
            double reqLon = request.Longitude ?? 0;
            const double RadiusKm = 30;

            // =========================
            // 1. QUERY BASE (NO FULL LOAD)
            // =========================
            var query = _dbContext.Set<Worker>()
                .AsNoTracking()
                .Where(x => !x.IsDeleted);

            // =========================
            // 2. LOCATION PRE-FILTER (SQL LEVEL)
            // =========================
            if (hasLocation)
            {
                double latDelta = 0.27;
                double lonDelta = 0.27 / Math.Cos(reqLat * Math.PI / 180.0);

                query = query.Where(w =>
                    w.Latitude.HasValue &&
                    w.Longitude.HasValue &&
                    w.Latitude >= reqLat - latDelta &&
                    w.Latitude <= reqLat + latDelta &&
                    w.Longitude >= reqLon - lonDelta &&
                    w.Longitude <= reqLon + lonDelta
                );
            }

            // =========================
            // 3. INCLUDE (SAU FILTER THÔ)
            // =========================
            query = query
                .Include(x => x.WorkerSkills)
                    .ThenInclude(s => s.Skill)
                .Include(x => x.WorkerCertifications)
                    .ThenInclude(c => c.Certification);

            var candidates = await query.ToListAsync();

            // =========================
            // 4. FILTER STRICT (RAM - FINAL CHECK)
            // =========================
            var filtered = candidates.Where(w =>
            {
                // ================= SKILL (AND)
                if (skillCategories.Any())
                {
                    bool okSkill = w.WorkerSkills != null &&
                        skillCategories.All(cat =>
                            w.WorkerSkills.Any(s =>
                                s.Skill != null &&
                                MatchText(s.Skill.Name, cat) ||
                                MatchText(s.Skill.Category, cat) ||
                                MatchText(s.Skill.Description, cat)
                            )
                        );

                    if (!okSkill) return false;
                }

                // ================= CERT (AND - FIX QUAN TRỌNG)
                if (certCategories.Any())
                {
                    bool okCert = w.WorkerCertifications != null &&
                        certCategories.All(cat =>
                            w.WorkerCertifications.Any(c =>
                                c.Certification != null &&
                                (
                                    MatchText(c.Certification.Name, cat) ||
                                    MatchText(c.Certification.Category, cat) ||
                                    MatchText(c.Certification.IssuingOrganization, cat)
                                )
                            )
                        );

                    if (!okCert) return false;
                }

                // ================= LOCATION (STRICT)
                if (hasLocation)
                {
                    if (!w.Latitude.HasValue || !w.Longitude.HasValue)
                        return false;

                    var dist = DistanceKm(
                        w.Latitude.Value,
                        w.Longitude.Value,
                        reqLat,
                        reqLon
                    );

                    if (dist > RadiusKm)
                        return false;
                }

                // ================= KEYWORD ADDRESS (OPTIONAL BUT STRICT IF EXISTS)
                if (!string.IsNullOrEmpty(keyword))
                {
                    if (string.IsNullOrEmpty(w.DisplayAddress) ||
                        !Normalize(w.DisplayAddress).Contains(keyword))
                        return false;
                }

                return true;
            });

            // =========================
            // 5. RANKING
            // =========================
            return filtered
                .OrderByDescending(w =>
                    (skillCategories.Count > 0 ? w.WorkerSkills.Count : 0) +
                    (certCategories.Count > 0 ? w.WorkerCertifications.Count : 0)
                )
                .ThenBy(w =>
                    hasLocation && w.Latitude.HasValue && w.Longitude.HasValue
                        ? DistanceKm(w.Latitude.Value, w.Longitude.Value, reqLat, reqLon)
                        : double.MaxValue
                )
                .ToList();
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
