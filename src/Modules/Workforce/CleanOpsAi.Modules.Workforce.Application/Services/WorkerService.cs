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
            var sw = Stopwatch.StartNew();

            Console.WriteLine($"\n========== NLP TRACE START [{traceId}] ==========");

            // =========================
            // 0. HARD GUARD (NULL INPUT)
            // =========================
            if (string.IsNullOrWhiteSpace(query))
            {
                var all = await _workerRepository.GetAllAsync();
                Console.WriteLine($"[{traceId}] EMPTY QUERY → return all | count={all.Count}");
                return BuildNlpResponse(all, null);
            }

            // =========================
            // 1. GEMINI (FAIL FAST + TIMEOUT)
            // =========================
            WorkerFilterNlpResult parsed;

            try
            {
                Console.WriteLine($"[{traceId}] [1] GEMINI START");

                var geminiTask = _geminiService.ParseWorkerFilterAsync(query);

                var completed = await Task.WhenAny(
                    geminiTask,
                    Task.Delay(2000) // ⛔ hard timeout 2s
                );

                if (completed == geminiTask)
                {
                    parsed = await geminiTask;
                    Console.WriteLine($"[{traceId}] [1] GEMINI DONE");
                }
                else
                {
                    Console.WriteLine($"[{traceId}] [1] GEMINI TIMEOUT → fallback local");
                    parsed = new WorkerFilterNlpResult();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[{traceId}] [1] GEMINI FAIL: {ex.Message}");
                parsed = new WorkerFilterNlpResult();
            }

            // =========================
            // 2. LOG PARSED
            // =========================
            Console.WriteLine("========== PARSED RESULT ==========");
            Console.WriteLine($"Address: {parsed.Address}");
            Console.WriteLine($"Skills: {string.Join(",", parsed.SkillCategories ?? new())}");
            Console.WriteLine($"Certs: {string.Join(",", parsed.CertificateCategories ?? new())}");
            Console.WriteLine($"StartAt: {parsed.StartAt}");
            Console.WriteLine($"EndAt: {parsed.EndAt}");
            Console.WriteLine("===================================");

            // fallback
            bool parsedNothing =
                string.IsNullOrWhiteSpace(parsed.Address) &&
                (parsed.SkillCategories == null || !parsed.SkillCategories.Any()) &&
                (parsed.CertificateCategories == null || !parsed.CertificateCategories.Any());

            if (parsedNothing)
                parsed.Address = query;

            // =========================
            // 3. DATE VALIDATION
            // =========================
            if (parsed.StartAt.HasValue && parsed.EndAt.HasValue &&
                parsed.StartAt > parsed.EndAt)
            {
                Console.WriteLine($"[{traceId}] INVALID DATE RANGE → ignored");
                parsed.StartAt = null;
                parsed.EndAt = null;
            }

            // =========================
            // 4. BUILD REQUEST
            // =========================
            var request = new WorkerFilterRequest
            {
                Address = parsed.Address,
                SkillCategories = parsed.SkillCategories,
                CertificateCategories = parsed.CertificateCategories,
                StartAt = parsed.StartAt,
                EndAt = parsed.EndAt
            };

            // =========================
            // 5. GEOCODE (SAFE)
            // =========================
            if (!string.IsNullOrWhiteSpace(request.Address))
            {
                try
                {
                    Console.WriteLine($"[{traceId}] [2] GEOCODE START");

                    var geoTask = _goongMapService.GetCoordinatesAsync(request.Address);

                    var completed = await Task.WhenAny(
                        geoTask,
                        Task.Delay(2000)
                    );

                    if (completed == geoTask)
                    {
                        var coords = await geoTask;

                        if (coords.HasValue)
                        {
                            request.Latitude = coords.Value.lat;
                            request.Longitude = coords.Value.lng;
                        }

                        Console.WriteLine($"[{traceId}] [2] GEOCODE DONE");
                    }
                    else
                    {
                        Console.WriteLine($"[{traceId}] GEOCODE TIMEOUT");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[{traceId}] GEOCODE FAIL: {ex.Message}");
                }
            }

            // =========================
            // 6. BUSY WORKER (SAFE MASS TRANSIT CALL)
            // =========================
            HashSet<Guid> busyIds = new();

            if (request.StartAt.HasValue && request.EndAt.HasValue)
            {
                try
                {
                    Console.WriteLine($"[{traceId}] [3] BUSY WORKER START");

                    var busyTask = _busyWorkerClient.GetResponse<GetBusyWorkerIdsResponse>(
                        new GetBusyWorkerIdsRequest
                        {
                            StartAt = request.StartAt.Value,
                            EndAt = request.EndAt.Value
                        });

                    var completed = await Task.WhenAny(
                        busyTask,
                        Task.Delay(2000)
                    );

                    if (completed == busyTask)
                    {
                        var busyResponse = await busyTask;
                        busyIds = busyResponse.Message.BusyWorkerIds.ToHashSet();

                        Console.WriteLine($"[{traceId}] [3] BUSY DONE | count={busyIds.Count}");
                    }
                    else
                    {
                        Console.WriteLine($"[{traceId}] BUSY TIMEOUT → skip busy filter");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[{traceId}] BUSY FAIL: {ex.Message}");
                }
            }

            // =========================
            // 7. DB QUERY (FASTEST PART)
            // =========================
            Console.WriteLine($"[{traceId}] [4] DB FILTER START");

            var dbSw = Stopwatch.StartNew();
            var workers = await _workerRepository.FilterStrictAsync(request);
            dbSw.Stop();

            Console.WriteLine($"[{traceId}] [4] DB DONE in {dbSw.ElapsedMilliseconds}ms | count={workers.Count}");

            // =========================
            // 8. REMOVE BUSY WORKERS
            // =========================
            if (busyIds.Count > 0)
                workers = workers.Where(x => !busyIds.Contains(x.Id)).ToList();

            // =========================
            // 9. FINAL RESPONSE
            // =========================
            sw.Stop();

            Console.WriteLine($"[{traceId}] FINAL COUNT = {workers.Count}");
            Console.WriteLine($"========== NLP TRACE END [{traceId}] TOTAL {sw.ElapsedMilliseconds}ms ==========\n");

            string? warning = workers.Any() ? null : "Không tìm thấy worker phù hợp.";

            return BuildNlpResponse(workers, warning);
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
