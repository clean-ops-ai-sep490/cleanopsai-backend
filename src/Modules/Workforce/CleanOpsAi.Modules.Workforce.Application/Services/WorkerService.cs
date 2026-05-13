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
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

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
        private readonly ISkillRepository _skillRepository;
        private readonly ICertificationRepository _certificationRepository;
        private readonly ILogger<WorkerService> _logger;

        public WorkerService(IWorkerRepository workerRepository, IFileStorageService fileStorageService, IUserContext userContext, IDateTimeProvider dateTimeProvider, IGoongMapService goongMapService, IRequestClient<GetBusyWorkerIdsRequest> busyWorkerClient, IGeminiService geminiService, ISkillRepository skillRepository, ICertificationRepository certificationRepository, ILogger<WorkerService> logger)
        {
            _workerRepository = workerRepository;
            _fileStorage = fileStorageService;
            _userContext = userContext;
            _dateTimeProvider = dateTimeProvider;
            _goongMapService = goongMapService;
            _busyWorkerClient = busyWorkerClient;
            _geminiService = geminiService;
            _skillRepository = skillRepository;
            _certificationRepository = certificationRepository;
            _logger = logger;
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
            // =========================
            // 1. NORMALIZE UTC
            // =========================
            if (request.StartAt.HasValue)
                request.StartAt = NormalizeToUtc(request.StartAt.Value);

            if (request.EndAt.HasValue)
                request.EndAt = NormalizeToUtc(request.EndAt.Value);

            // =========================
            // 2. VALIDATE DATE RANGE
            // =========================
            if (request.StartAt.HasValue && request.EndAt.HasValue)
            {
                if (request.StartAt >= request.EndAt)
                    throw new BadRequestException("startAt phải nhỏ hơn endAt.");
            }
            else if (request.StartAt.HasValue)
            {
                throw new BadRequestException("Cần truyền endAt khi có startAt.");
            }
            else if (request.EndAt.HasValue)
            {
                throw new BadRequestException("Cần truyền startAt khi có endAt.");
            }

            // =========================
            // 3. GEOCODE ADDRESS
            // =========================
            if (!string.IsNullOrWhiteSpace(request.Address)
                && !request.Latitude.HasValue
                && !request.Longitude.HasValue)
            {
                var coords = await _goongMapService
                    .GetCoordinatesAsync(request.Address);

                if (coords.HasValue)
                {
                    request.Latitude = coords.Value.lat;
                    request.Longitude = coords.Value.lng;
                }
            }

            // =========================
            // 4. GET BUSY WORKERS
            // =========================
            var busyWorkerIds = new HashSet<Guid>();

            if (request.StartAt.HasValue && request.EndAt.HasValue)
            {
                var busyResponse = await _busyWorkerClient
                    .GetResponse<GetBusyWorkerIdsResponse>(
                        new GetBusyWorkerIdsRequest
                        {
                            StartAt = request.StartAt.Value,
                            EndAt = request.EndAt.Value
                        });

                busyWorkerIds = busyResponse
                    .Message
                    .BusyWorkerIds
                    .ToHashSet();
            }

            // =========================
            // 5. FILTER WORKERS
            // =========================
            Console.WriteLine("========== FINAL FILTER REQUEST ==========");
            Console.WriteLine(JsonSerializer.Serialize(request,
                new JsonSerializerOptions
                {
                    WriteIndented = true
                }));
            var workers = await _workerRepository.FilterAsync(request);

            // =========================
            // 6. REMOVE BUSY WORKERS
            // =========================
            workers = workers
                .Where(x => !busyWorkerIds.Contains(x.Id))
                .ToList();

            // =========================
            // 7. MAP RESPONSE
            // =========================
            return workers
                .Select(x => new WorkerResponse
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
                })
                .ToList();
        }

        private static DateTime NormalizeToUtc(DateTime value)
        {
            return value.Kind switch
            {
                DateTimeKind.Utc => value,
                DateTimeKind.Local => value.ToUniversalTime(),
                _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
            };
        }

        // search nlp filter, example query: "Tìm thợ điện ở Hà Nội có chứng chỉ an toàn điện và kỹ năng hàn, không bận từ 1/10 đến 5/10"
        public async Task<List<WorkerResponse>> NlpFilterAsync(string? query, CancellationToken cancellationToken = default)
        {
            
            var correlationId = Guid.NewGuid().ToString("N");
            _logger.LogInformation("[NLP:{CorrelationId}] Search request received. Query='{Query}'", correlationId, query);

            //if (string.IsNullOrWhiteSpace(query))
            //{
            //    _logger.LogInformation("[NLP:{CorrelationId}] Empty query => returning all workers", correlationId);
            //    return await GetAllAsync();
            //}
            if (string.IsNullOrWhiteSpace(query))
            {
                _logger.LogWarning("[NLP:{CorrelationId}] Empty query", correlationId);

                return new List<WorkerResponse>();
            }

            var parsed = WorkerNlpLocalParser.Parse(query);
            _logger.LogInformation(
                "[NLP:{CorrelationId}] Local parser result: {Parsed}",
                correlationId,
                JsonSerializer.Serialize(parsed, new JsonSerializerOptions { WriteIndented = true }));

            if (HasParsedFilters(parsed))
            {
                _logger.LogInformation("[NLP:{CorrelationId}] Local parser found filters => skipping Gemini enrichment", correlationId);
            }
            else
            {
                try
                {
                    using var geminiCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                    geminiCts.CancelAfter(TimeSpan.FromSeconds(1.5));

                    _logger.LogInformation("[NLP:{CorrelationId}] Calling Gemini enrichment...", correlationId);
                    var geminiResult = await _geminiService.ParseWorkerFilterAsync(query, geminiCts.Token);
                    //_logger.LogInformation(
                    //    "[NLP:{CorrelationId}] Gemini result: {Gemini}",
                    //    correlationId,
                    //    JsonSerializer.Serialize(geminiResult, new JsonSerializerOptions { WriteIndented = true }));

                    parsed = WorkerNlpLocalParser.Merge(geminiResult, parsed);
                    //_logger.LogInformation(
                    //    "[NLP:{CorrelationId}] Merged parser result: {Merged}",
                    //    correlationId,
                    //    JsonSerializer.Serialize(parsed, new JsonSerializerOptions { WriteIndented = true }));
                }
                catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
                {
                    _logger.LogWarning("[NLP:{CorrelationId}] Gemini timeout => fallback to local parser", correlationId);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "[NLP:{CorrelationId}] Gemini enrichment failed => fallback to local parser", correlationId);
                }
            }

            if (parsed.StartAt.HasValue && parsed.EndAt.HasValue && parsed.StartAt > parsed.EndAt)
            {
                _logger.LogWarning("[NLP:{CorrelationId}] Invalid date range detected, resetting StartAt/EndAt", correlationId);
                parsed.StartAt = null;
                parsed.EndAt = null;
            }

            _logger.LogInformation(
                "[NLP:{CorrelationId}] Resolve IDs from categories. Skills={Skills}; Certs={Certs}",
                correlationId,
                string.Join(", ", parsed.SkillCategories ?? new List<string>()),
                string.Join(", ", parsed.CertificateCategories ?? new List<string>()));

            var skillIds = await ResolveSkillIdsAsync(parsed.SkillCategories, query);
            var certificationIds = await ResolveCertificationIdsAsync(parsed.CertificateCategories, query);

            _logger.LogInformation(
                "[NLP:{CorrelationId}] Resolved skillIds={SkillIds}; certIds={CertIds}",
                correlationId,
                string.Join(", ", skillIds),
                string.Join(", ", certificationIds));

            var request = new WorkerFilterRequest
            {
                Address = CleanAddress(parsed.Address),
                SkillIds = skillIds,
                CertificateIds = certificationIds,
                StartAt = parsed.StartAt,
                EndAt = parsed.EndAt
            };

            if (request.StartAt.HasValue)
                request.StartAt = NormalizeToUtc(request.StartAt.Value);

            if (request.EndAt.HasValue)
                request.EndAt = NormalizeToUtc(request.EndAt.Value);

            _logger.LogInformation(
                "[NLP:{CorrelationId}] Built filter request: {Request}",
                correlationId,
                JsonSerializer.Serialize(request, new JsonSerializerOptions { WriteIndented = true }));

            if (!string.IsNullOrWhiteSpace(request.Address))
            {
                try
                {
                    using var geocodeCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                    geocodeCts.CancelAfter(TimeSpan.FromSeconds(2));

                    _logger.LogInformation("[NLP:{CorrelationId}] Geocoding address='{Address}'", correlationId, request.Address);
                    var coords = await _goongMapService.GetCoordinatesAsync(request.Address, geocodeCts.Token);
                    _logger.LogInformation("[NLP:{CorrelationId}] Geocode result={Coords}", correlationId, coords.HasValue ? $"{coords.Value.lat},{coords.Value.lng}" : "null");
                    if (coords.HasValue)
                    {
                        request.Latitude = coords.Value.lat;
                        request.Longitude = coords.Value.lng;
                    }
                }
                catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
                {
                    _logger.LogWarning("[NLP:{CorrelationId}] Geocode timeout", correlationId);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "[NLP:{CorrelationId}] Geocoding failed", correlationId);
                }
            }

            var busyWorkerIds = new HashSet<Guid>();
            if (request.StartAt.HasValue && request.EndAt.HasValue)
            {
                try
                {
                    using var busyCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                    busyCts.CancelAfter(TimeSpan.FromSeconds(1));

                    _logger.LogInformation("[NLP:{CorrelationId}] Checking busy workers", correlationId);
                    var response = await _busyWorkerClient.GetResponse<GetBusyWorkerIdsResponse>(
                        new GetBusyWorkerIdsRequest
                        {
                            StartAt = request.StartAt.Value,
                            EndAt = request.EndAt.Value
                        },
                        busyCts.Token);

                    busyWorkerIds = response.Message.BusyWorkerIds.ToHashSet();
                    _logger.LogInformation("[NLP:{CorrelationId}] Busy workers count={Count}", correlationId, busyWorkerIds.Count);
                }
                catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
                {
                    _logger.LogWarning("[NLP:{CorrelationId}] Busy worker lookup timeout", correlationId);
                }
                catch (RequestTimeoutException ex)
                {
                    _logger.LogWarning(ex, "[NLP:{CorrelationId}] Busy worker lookup request timeout", correlationId);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "[NLP:{CorrelationId}] Busy worker lookup failed", correlationId);
                }
            }

            cancellationToken.ThrowIfCancellationRequested();

            _logger.LogInformation("[NLP:{CorrelationId}] Calling FilterStrictAsync...", correlationId);
            var workers = await _workerRepository.FilterStrictAsync(request);
            _logger.LogInformation("[NLP:{CorrelationId}] FilterStrictAsync returned {Count} workers", correlationId, workers.Count);

            if (busyWorkerIds.Count > 0)
            {
                workers = workers.Where(x => !busyWorkerIds.Contains(x.Id)).ToList();
                _logger.LogInformation("[NLP:{CorrelationId}] After busy filter => {Count} workers", correlationId, workers.Count);
            }

            var result = workers
                .Select(x => new WorkerResponse
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
                })
                .ToList();

            _logger.LogInformation("[NLP:{CorrelationId}] Final response count={Count}", correlationId, result.Count);
            return result;
        }

        private async Task<List<Guid>> ResolveSkillIdsAsync(List<string>? categories, string query)
        {
            categories ??= new List<string>();
            var normalizedQuery = WorkerNlpLocalParser.NormalizeSearchText(query);
            var hasSkillIntent = categories.Any() || ContainsSkillIntent(normalizedQuery);
            var hasOnlyCertificateIntent = ContainsCertificateIntent(normalizedQuery) && !hasSkillIntent;

            if (hasOnlyCertificateIntent)
                return new List<Guid>();

            var normalizedTerms = categories
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(WorkerNlpLocalParser.NormalizeSearchText)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
            var ids = new HashSet<Guid>();
            var allSkills = await _skillRepository.GetAllAsync();

            foreach (var term in GetTermsForCatalogMatching(normalizedTerms, normalizedQuery))
            {
                var scoredSkills = allSkills
                    .Select(skill =>
                    {
                        var normalizedName = WorkerNlpLocalParser.NormalizeSearchText(skill.Name);
                        var normalizedCategory = WorkerNlpLocalParser.NormalizeSearchText(skill.Category);
                        var normalizedDescription = WorkerNlpLocalParser.NormalizeSearchText(skill.Description);

                        var score = ScoreCatalogName(term, normalizedName);

                        if (hasSkillIntent)
                        {
                            score = Math.Max(score, ScoreCatalogText(term, normalizedCategory));
                            score = Math.Max(score, ScoreCatalogText(term, normalizedDescription));
                        }

                        //_logger.LogInformation(
                        //    "[NLP:ResolveSkill] Term='{Term}' skill='{SkillName}', category='{Category}' => score={Score}",
                        //    term,
                        //    skill.Name,
                        //    skill.Category,
                        //    score);

                        return new { Skill = skill, Score = score };
                    })
                    .Where(x => x.Score > 0)
                    .ToList();

                if (!scoredSkills.Any())
                    continue;

                var bestScore = scoredSkills.Max(x => x.Score);

                foreach (var match in scoredSkills.Where(x => x.Score == bestScore))
                    ids.Add(match.Skill.Id);
            }

            return ids.ToList();
        }

        private static bool HasParsedFilters(WorkerFilterNlpResult parsed)
        {
            return !string.IsNullOrWhiteSpace(parsed.Address)
                   || parsed.SkillCategories.Any()
                   || parsed.CertificateCategories.Any()
                   || parsed.StartAt.HasValue
                   || parsed.EndAt.HasValue
                   || parsed.IsAvailable.HasValue;
        }

        private async Task<List<Guid>> ResolveCertificationIdsAsync(List<string>? categories, string query)
        {
            categories ??= new List<string>();
            var normalizedQuery = WorkerNlpLocalParser.NormalizeSearchText(query);
            var hasCertificateIntent = categories.Any() || ContainsCertificateIntent(normalizedQuery);

            if (!hasCertificateIntent)
                return new List<Guid>();

            var normalizedTerms = categories
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(WorkerNlpLocalParser.NormalizeSearchText)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
            var ids = new HashSet<Guid>();
            var allCertifications = await _certificationRepository.GetAllAsync();

            foreach (var term in GetTermsForCatalogMatching(normalizedTerms, normalizedQuery))
            {
                var scoredCertifications = allCertifications
                    .Select(certification =>
                    {
                        var normalizedName = WorkerNlpLocalParser.NormalizeSearchText(certification.Name);
                        var normalizedCategory = WorkerNlpLocalParser.NormalizeSearchText(certification.Category);
                        var normalizedOrganization = WorkerNlpLocalParser.NormalizeSearchText(certification.IssuingOrganization);

                        var score = ScoreCatalogName(term, normalizedName);
                        score = Math.Max(score, ScoreCatalogText(term, normalizedCategory));
                        score = Math.Max(score, ScoreCatalogText(term, normalizedOrganization));

                        //_logger.LogInformation(
                        //    "[NLP:ResolveCert] Term='{Term}' cert='{CertName}', category='{Category}' => score={Score}",
                        //    term,
                        //    certification.Name,
                        //    certification.Category,
                        //    score);

                        return new { Certification = certification, Score = score };
                    })
                    .Where(x => x.Score > 0)
                    .ToList();

                if (!scoredCertifications.Any())
                    continue;

                var bestScore = scoredCertifications.Max(x => x.Score);

                foreach (var match in scoredCertifications.Where(x => x.Score == bestScore))
                    ids.Add(match.Certification.Id);
            }

            return ids.ToList();
        }

        private static List<string> GetTermsForCatalogMatching(List<string> parsedTerms, string normalizedQuery)
        {
            return parsedTerms.Any()
                ? parsedTerms
                : new List<string> { normalizedQuery };
        }

        private static bool MatchCatalogName(string input, string catalogName)
        {
            if (string.IsNullOrWhiteSpace(input) || string.IsNullOrWhiteSpace(catalogName))
                return false;

            var terms = catalogName
                .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Where(x => x.Length > 1)
                .ToList();

            if (terms.Count == 0)
                return false;

            if (input.Contains(catalogName, StringComparison.OrdinalIgnoreCase))
                return true;

            var looseInput = LooseSearchText(input);
            var looseCatalogName = LooseSearchText(catalogName);

            if (!string.IsNullOrWhiteSpace(looseInput)
                && !string.IsNullOrWhiteSpace(looseCatalogName)
                && looseInput.Contains(looseCatalogName, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            return terms.Count >= 2
                   && terms.All(term => input.Contains(term, StringComparison.OrdinalIgnoreCase));
        }

        private static int ScoreCatalogName(string input, string catalogName)
        {
            if (string.IsNullOrWhiteSpace(input) || string.IsNullOrWhiteSpace(catalogName))
                return 0;

            if (input.Equals(catalogName, StringComparison.OrdinalIgnoreCase))
                return 100;

            if (input.Contains(catalogName, StringComparison.OrdinalIgnoreCase))
                return 90;

            if (catalogName.Contains(input, StringComparison.OrdinalIgnoreCase))
                return 75;

            var looseInput = LooseSearchText(input);
            var looseCatalogName = LooseSearchText(catalogName);

            if (!string.IsNullOrWhiteSpace(looseInput) && looseInput.Equals(looseCatalogName, StringComparison.OrdinalIgnoreCase))
                return 85;

            if (!string.IsNullOrWhiteSpace(looseInput)
                && !string.IsNullOrWhiteSpace(looseCatalogName)
                && looseInput.Contains(looseCatalogName, StringComparison.OrdinalIgnoreCase))
                return 70;

            if (!string.IsNullOrWhiteSpace(looseInput)
                && !string.IsNullOrWhiteSpace(looseCatalogName)
                && looseCatalogName.Contains(looseInput, StringComparison.OrdinalIgnoreCase))
                return 60;

            var terms = catalogName
                .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Where(x => x.Length > 1)
                .ToList();

            return terms.Count >= 2 && terms.All(term => input.Contains(term, StringComparison.OrdinalIgnoreCase))
                ? 55
                : 0;
        }

        private static bool MatchCatalogText(string input, string catalogText)
        {
            if (string.IsNullOrWhiteSpace(input) || string.IsNullOrWhiteSpace(catalogText))
                return false;

            if (input.Contains(catalogText, StringComparison.OrdinalIgnoreCase)
                || catalogText.Contains(input, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            var looseInput = LooseSearchText(input);
            var looseCatalogText = LooseSearchText(catalogText);

            return !string.IsNullOrWhiteSpace(looseInput)
                   && !string.IsNullOrWhiteSpace(looseCatalogText)
                   && (looseInput.Contains(looseCatalogText, StringComparison.OrdinalIgnoreCase)
                       || looseCatalogText.Contains(looseInput, StringComparison.OrdinalIgnoreCase));
        }

        private static int ScoreCatalogText(string input, string catalogText)
        {
            return MatchCatalogText(input, catalogText) ? 40 : 0;
        }

        private static bool ContainsSkillIntent(string normalizedQuery)
        {
            return Regex.IsMatch(
                normalizedQuery,
                @"\b(skill|skills|ky nang|ki nang|biet|lam|lam duoc|lam tot|su dung)\b",
                RegexOptions.IgnoreCase);
        }

        private static bool ContainsCertificateIntent(string normalizedQuery)
        {
            return Regex.IsMatch(
                normalizedQuery,
                @"\b(certificate|cert|chung chi|chung nhan|bang)\b",
                RegexOptions.IgnoreCase);
        }

        private static string LooseSearchText(string value)
        {
            var normalized = WorkerNlpLocalParser.NormalizeSearchText(value);
            normalized = Regex.Replace(normalized, @"[^a-z0-9\s]", " ");
            normalized = Regex.Replace(normalized, @"[aeiouy]", "");
            return Regex.Replace(normalized, @"\s+", " ").Trim();
        }

        private static string? CleanAddress(string? address)
        {
            if (string.IsNullOrWhiteSpace(address))
                return null;

            return address.Trim();
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
