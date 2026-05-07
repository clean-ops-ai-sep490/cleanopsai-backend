using CleanOpsAi.BuildingBlocks.Application;
using CleanOpsAi.BuildingBlocks.Application.Exceptions;
using CleanOpsAi.BuildingBlocks.Application.Interfaces;
using CleanOpsAi.BuildingBlocks.Infrastructure.Events.Request;
using CleanOpsAi.BuildingBlocks.Infrastructure.Events.Response;
using CleanOpsAi.Modules.Workforce.Application.Dtos;
using CleanOpsAi.Modules.Workforce.Application.Dtos.Nlps;
using CleanOpsAi.Modules.Workforce.Application.Dtos.Workers;
using CleanOpsAi.Modules.Workforce.Application.Interfaces;
using CleanOpsAi.Modules.Workforce.Domain.Entities;
using MassTransit;
using Medo;
using System.Diagnostics;

namespace CleanOpsAi.Modules.Workforce.Application.Services
{
    public class WorkerService : IWorkerService
    {
        private readonly IWorkerRepository _workerRepository;
        private readonly IFileStorageService _fileStorage;
        private const string CONTAINER = "contracts";
        private const string AVATAR_FOLDER = "avatars";
        private readonly IUserContext _userContext;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly IGoongMapService _goongMapService;
        private readonly IRequestClient<GetBusyWorkerIdsRequest> _busyWorkerClient;
        private readonly IGeminiService _geminiService;

        public WorkerService(IWorkerRepository workerRepository, IFileStorageService fileStorageService, IUserContext userContext, IDateTimeProvider dateTimeProvider, IGoongMapService goongMapService, IRequestClient<GetBusyWorkerIdsRequest> busyWorkerClient, IGeminiService geminiService)
        {
            _workerRepository = workerRepository;
            _fileStorage = fileStorageService;
            _userContext = userContext;
            _dateTimeProvider = dateTimeProvider;
            _goongMapService = goongMapService;
            _busyWorkerClient = busyWorkerClient;
            _geminiService = geminiService;
        }

        // get by id
        public async Task<List<WorkerResponse>> GetByIdAsync(Guid id)
        {
            var worker = await _workerRepository.GetByIdAsync(id);

            if (worker == null)
                return null;

            return new List<WorkerResponse>
            {
                new WorkerResponse
                {
                    Id = worker.Id,
                    UserId = worker.UserId,
                    FullName = worker.FullName,
                    DisplayAddress = worker.DisplayAddress,
                    Latitude = worker.Latitude,
                    Longitude = worker.Longitude,
                    AvatarUrl = worker.AvatarUrl,
                    TotalSkills = worker.WorkerSkills.Count,
                    TotalCertifications = worker.WorkerCertifications.Count
                }
            };
        }

        // get by user id
        public async Task<WorkerResponse> GetByUserIdAsync(Guid userId)
        {
            var worker = await _workerRepository.GetByUserIdAsync(userId);

            if (worker == null)
                return null;

            return new WorkerResponse
            {
                Id = worker.Id,
                UserId = worker.UserId,
                FullName = worker.FullName,
                DisplayAddress = worker.DisplayAddress,
                Latitude = worker.Latitude,
                Longitude = worker.Longitude,
                AvatarUrl = worker.AvatarUrl,
                TotalSkills = worker.WorkerSkills.Count,
                TotalCertifications = worker.WorkerCertifications.Count
            };
        }

        // get all
        public async Task<List<WorkerResponse>> GetAllAsync()
        {
            var workers = await _workerRepository.GetAllAsync();

            return workers.Select(x => new WorkerResponse
            {
                Id = x.Id,
                UserId = x.UserId,
                FullName = x.FullName,
                DisplayAddress = x.DisplayAddress,
                Latitude = x.Latitude,
                Longitude = x.Longitude,
                AvatarUrl = x.AvatarUrl,
                TotalSkills = x.WorkerSkills.Count,
                TotalCertifications = x.WorkerCertifications.Count
            }).ToList();
        }

        // get all pagination
        public async Task<PagedResponse<WorkerResponse>> GetAllPaginationAsync(int pageNumber, int pageSize)
        {
            var (items, totalCount) = await _workerRepository.GetAllPaginationAsync(pageNumber, pageSize);

            var responses = items.Select(x => new WorkerResponse
            {
                Id = x.Id,
                UserId = x.UserId,
                FullName = x.FullName,
                DisplayAddress = x.DisplayAddress,
                Latitude = x.Latitude,
                Longitude = x.Longitude,
                AvatarUrl = x.AvatarUrl,
                TotalSkills = x.WorkerSkills.Count,
                TotalCertifications = x.WorkerCertifications.Count
            }).ToList();

            return new PagedResponse<WorkerResponse>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalElements = totalCount,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize),
                Content = responses
            };
        }

        // create
        public async Task<WorkerResponse> CreateAsync(WorkerCreateRequest request)
        {
            var worker = new Worker
            {
                Id = Uuid7.NewGuid(),
                UserId = request.UserId,
                FullName = request.FullName,
                DisplayAddress = request.DisplayAddress,
                Latitude = request.Latitude,
                Longitude = request.Longitude,
                AvatarUrl = request.AvatarUrl,
                Created = _dateTimeProvider.UtcNow,
                CreatedBy = _userContext.UserId.ToString(),
                IsDeleted = false
            };

            await _workerRepository.CreateAsync(worker);

            return new WorkerResponse
            {
                Id = worker.Id,
                UserId = worker.UserId,
                FullName = worker.FullName,
                DisplayAddress = worker.DisplayAddress,
                Latitude = worker.Latitude,
                Longitude = worker.Longitude,
                AvatarUrl = worker.AvatarUrl,
                TotalSkills = 0,
                TotalCertifications = 0
            };
        }

        // update
        public async Task<WorkerResponse?> UpdateAsync(Guid id, WorkerUpdateRequest request)
        {
            var worker = await _workerRepository.GetByIdAsync(id);

            if (worker == null)
                throw new KeyNotFoundException($"Worker with id {id} not found.");

            if (!string.IsNullOrWhiteSpace(request.FullName))
                worker.FullName = request.FullName;

            // Geocode address → lat/lng + DisplayAddress
            if (!string.IsNullOrWhiteSpace(request.DisplayAddress))
            {
                worker.DisplayAddress = request.DisplayAddress;

                var coords = await _goongMapService.GetCoordinatesAsync(request.DisplayAddress);
                if (coords.HasValue)
                {
                    worker.Latitude = coords.Value.lat;
                    worker.Longitude = coords.Value.lng;
                }
            }

            // Upload avatar
            if (request.AvatarStream != null)
            {
                var fileUrl = await _fileStorage.UploadFileAsync(
                    request.AvatarStream,
                    $"{AVATAR_FOLDER}/{request.AvatarFileName}",
                    CONTAINER
                );
                worker.AvatarUrl = fileUrl;
            }

            worker.LastModified = _dateTimeProvider.UtcNow;
            worker.LastModifiedBy = _userContext.UserId.ToString();

            await _workerRepository.UpdateAsync(worker);

            return new WorkerResponse
            {
                Id = worker.Id,
                UserId = worker.UserId,
                FullName = worker.FullName,
                DisplayAddress = worker.DisplayAddress,
                Latitude = worker.Latitude,
                Longitude = worker.Longitude,
                AvatarUrl = worker.AvatarUrl,
                TotalSkills = worker.WorkerSkills.Count,
                TotalCertifications = worker.WorkerCertifications.Count
            };
        }

        // delete
        public async Task<int> DeleteAsync(Guid id)
        {
            var worker = await _workerRepository.GetByIdAsync(id);

            if (worker == null)
                throw new KeyNotFoundException($"Worker with id {id} not found.");

            return await _workerRepository.DeleteAsync(id);
        }

        // get information of current worker by user id from user context
        public async Task<List<WorkerResponse>> GetInforAsync()
        {
            var worker = await _workerRepository.GetByUserIdAsync(_userContext.UserId);

            if (worker == null)
                return null;

            return new List<WorkerResponse>
            {
                new WorkerResponse
                {
                    Id = worker.Id,
                    UserId = worker.UserId,
                    FullName = worker.FullName,
                    DisplayAddress = worker.DisplayAddress,
                    Latitude = worker.Latitude,
                    Longitude = worker.Longitude,
                    AvatarUrl = worker.AvatarUrl,
                    TotalSkills = worker.WorkerSkills.Count,
                    TotalCertifications = worker.WorkerCertifications.Count
                }
            };
        }

        // filter workers based on skills, certifications, location, and availability (busy or not) within a time range
        public async Task<List<WorkerResponse>> FilterAsync(WorkerFilterRequest request)
        {
            // Validate startAt/endAt
            if (request.StartAt.HasValue && request.EndAt.HasValue)
            {
                if (request.StartAt > request.EndAt)
                    throw new BadRequestException("startAt phải nhỏ hơn endAt.");
            }
            else if (request.StartAt.HasValue && !request.EndAt.HasValue)
            {
                throw new BadRequestException("Cần truyền endAt khi có startAt.");
            }
            else if (!request.StartAt.HasValue && request.EndAt.HasValue)
            {
                throw new BadRequestException("Cần truyền startAt khi có endAt.");
            }

            // Nếu FE truyền address thì geocode sang lat/lng
            if (!string.IsNullOrWhiteSpace(request.Address)
                && !request.Latitude.HasValue
                && !request.Longitude.HasValue)
            {
                var coords = await _goongMapService.GetCoordinatesAsync(request.Address);
                if (coords.HasValue)
                {
                    request.Latitude = coords.Value.lat;
                    request.Longitude = coords.Value.lng;
                }
            }

            //// Lấy danh sách worker đang bận
            var busyWorkerIds = new HashSet<Guid>();
            if (request.StartAt.HasValue && request.EndAt.HasValue)
            {
                var busyResponse = await _busyWorkerClient
                    .GetResponse<GetBusyWorkerIdsResponse>(new GetBusyWorkerIdsRequest
                    {
                        StartAt = request.StartAt.Value,
                        EndAt = request.EndAt.Value
                    });

                busyWorkerIds = busyResponse.Message.BusyWorkerIds.ToHashSet();
            }

            var workers = await _workerRepository.FilterAsync(request);

            return workers
            .Where(x => !busyWorkerIds.Contains(x.Id)) // loại worker bận
            .Select(x => new WorkerResponse
            {
                Id = x.Id,
                UserId = x.UserId,
                FullName = x.FullName,
                DisplayAddress = x.DisplayAddress,
                Latitude = x.Latitude,
                Longitude = x.Longitude,
                AvatarUrl = x.AvatarUrl,
                TotalSkills = x.WorkerSkills.Count,
                TotalCertifications = x.WorkerCertifications.Count
            }).ToList();
        }

        // search nlp filter, example query: "Tìm thợ điện ở Hà Nội có chứng chỉ an toàn điện và kỹ năng hàn, không bận từ 1/10 đến 5/10"
        public async Task<WorkerNlpFilterResponse> NlpFilterAsync(string? query)
        {
            var traceId = Guid.NewGuid().ToString("N");

            Console.WriteLine($"\n===== NLP START {traceId} =====");

            if (string.IsNullOrWhiteSpace(query))
            {
                var all = await _workerRepository.GetAllAsync();
                return BuildNlpResponse(all, null);
            }

            WorkerFilterNlpResult parsed;

            // =========================
            // 1. GEMINI SAFE CALL (NO BLOCK)
            // =========================
            try
            {
                var task = _geminiService.ParseWorkerFilterAsync(query);

                var completed = await Task.WhenAny(task, Task.Delay(1500));

                if (completed == task)
                    parsed = await task;
                else
                {
                    Console.WriteLine("GEMINI TIMEOUT → LOCAL ONLY");
                    parsed = new WorkerFilterNlpResult();
                }
            }
            catch
            {
                parsed = new WorkerFilterNlpResult();
            }

            // =========================
            // 2. SAFE FALLBACK (KHÔNG DÙNG QUERY LÀ ADDRESS)
            // =========================
            if (IsEmpty(parsed))
            {
                parsed = LocalParse(query); // 🔥 FIX QUAN TRỌNG
            }

            // =========================
            // 3. VALIDATE DATE
            // =========================
            if (parsed.StartAt > parsed.EndAt)
            {
                parsed.StartAt = null;
                parsed.EndAt = null;
            }

            var request = new WorkerFilterRequest
            {
                Address = CleanAddress(parsed.Address),
                SkillCategories = parsed.SkillCategories,
                CertificateCategories = parsed.CertificateCategories,
                StartAt = parsed.StartAt,
                EndAt = parsed.EndAt
            };

            // =========================
            // 4. GEOCODE SAFE
            // =========================
            if (!string.IsNullOrWhiteSpace(request.Address))
            {
                try
                {
                    var coords = await _goongMapService.GetCoordinatesAsync(request.Address);

                    if (coords.HasValue)
                    {
                        request.Latitude = coords.Value.lat;
                        request.Longitude = coords.Value.lng;
                    }
                }
                catch
                {
                    Console.WriteLine("GEOCODE FAIL → SKIP");
                }
            }

            // =========================
            // 5. BUSY WORKER SAFE
            // =========================
            HashSet<Guid> busyIds = new();

            if (request.StartAt.HasValue && request.EndAt.HasValue)
            {
                try
                {
                    var busyTask = _busyWorkerClient.GetResponse<GetBusyWorkerIdsResponse>(
                        new GetBusyWorkerIdsRequest
                        {
                            StartAt = request.StartAt.Value,
                            EndAt = request.EndAt.Value
                        });

                    var completed = await Task.WhenAny(busyTask, Task.Delay(1500));

                    if (completed == busyTask)
                    {
                        var res = await busyTask;
                        busyIds = res.Message.BusyWorkerIds.ToHashSet();
                    }
                }
                catch
                {
                    Console.WriteLine("BUSY WORKER FAIL → SKIP");
                }
            }

            // =========================
            // 6. DB FAST QUERY (IMPORTANT FIX)
            // =========================
            var workers = await _workerRepository.FilterStrictAsync(request);

            // =========================
            // 7. REMOVE BUSY
            // =========================
            if (busyIds.Count > 0)
                workers = workers.Where(x => !busyIds.Contains(x.Id)).ToList();

            Console.WriteLine($"FINAL COUNT = {workers.Count}");

            return BuildNlpResponse(workers, workers.Any() ? null : "Không tìm thấy worker phù hợp.");
        }

        private bool IsEmpty(WorkerFilterNlpResult r)
        {
            return r == null ||
                   (string.IsNullOrWhiteSpace(r.Address)
                    && (r.SkillCategories == null || !r.SkillCategories.Any())
                    && (r.CertificateCategories == null || !r.CertificateCategories.Any()));
        }

        private WorkerFilterNlpResult LocalParse(string query)
        {
            var result = new WorkerFilterNlpResult
            {
                Address = null,
                SkillCategories = new List<string>(),
                CertificateCategories = new List<string>()
            };

            if (string.IsNullOrWhiteSpace(query))
                return result;

            var lower = query.ToLower();

            // skill
            var skill = System.Text.RegularExpressions.Regex.Match(lower, @"skill\s+(.+)");
            if (skill.Success)
                result.SkillCategories.Add(skill.Groups[1].Value.Trim());

            // cert
            var cert = System.Text.RegularExpressions.Regex.Match(lower, @"(certificate|cert|chứng chỉ)\s+(.+)");
            if (cert.Success)
                result.CertificateCategories.Add(cert.Groups[2].Value.Trim());

            // address
            var addr = System.Text.RegularExpressions.Regex.Match(lower, @"(ở|tại|in)\s+(.+)");
            if (addr.Success)
                result.Address = addr.Groups[2].Value.Trim();

            return result;
        }

        private string CleanAddress(string? address)
        {
            if (string.IsNullOrWhiteSpace(address))
                return null;

            address = address.ToLower().Trim();

            if (address.Contains("skill") ||
                address.Contains("certificate"))
                return null;

            return address;
        }


        // -------------------------------------------------------
        // HELPER — map + wrap response
        // -------------------------------------------------------
        private WorkerNlpFilterResponse BuildNlpResponse(List<Worker> workers, string? warning)
        {
            return new WorkerNlpFilterResponse
            {
                Warning = warning,
                Workers = workers.Select(x => new WorkerResponse
                {
                    Id = x.Id,
                    UserId = x.UserId,
                    FullName = x.FullName,
                    DisplayAddress = x.DisplayAddress,
                    Latitude = x.Latitude,
                    Longitude = x.Longitude,
                    AvatarUrl = x.AvatarUrl,
                    TotalSkills = x.WorkerSkills?.Count ?? 0,
                    TotalCertifications = x.WorkerCertifications?.Count ?? 0
                }).ToList()
            };
        }


        // Lấy thông tin cơ bản (id, full name) của danh sách worker theo ids, dùng cho hiển thị trong dropdown khi chọn worker cho task assignment
        public async Task<List<WorkerDto>> GetWorkersByIds(List<Guid> ids)
		{
			var workers = await _workerRepository.GetWorkersByIds(ids);
			return workers.Select(x => new WorkerDto
			{
				Id = x.Id,
				UserId = x.UserId,
				FullName = x.FullName
			}).ToList();
		}

		public async Task<List<WorkerByUserIdResponse>> GetWorkersByUserIds(List<Guid> userIds)
		{
			var workers = await _workerRepository.GetWorkersByUserIds(userIds);

			return workers.Select(x => new WorkerByUserIdResponse
			{
				UserId = x.UserId,
				WorkerId = x.Id,
				FullName = x.FullName
			}).ToList();
		}

		public async Task<List<Guid>> GetWorkersWithAllSkillsAndCertsAsync(List<Guid> workerIds, List<Guid> requiredSkillIds, List<Guid> requiredCertIds, CancellationToken ct)
		{
			return await _workerRepository.GetWorkersWithAllSkillsAndCertsAsync(workerIds, requiredSkillIds, requiredCertIds, ct);
		}

		public async Task<List<Guid>> GetQualifiedWorkersAsync(List<Guid> requiredSkillIds, List<Guid> requiredCertificationIds, CancellationToken ct = default)
		{
			return await _workerRepository.GetQualifiedWorkersAsync(requiredSkillIds, requiredCertificationIds, ct);
		}

		public async Task<bool> IsWorkerQualifiedAsync(Guid workerId, List<Guid> requiredSkillIds, List<Guid> requiredCertificationIds, CancellationToken ct = default)
		{
			return await _workerRepository.IsWorkerQualifiedAsync(workerId, requiredSkillIds, requiredCertificationIds, ct);
		}

	}
}
