using CleanOpsAi.BuildingBlocks.Application;
using CleanOpsAi.BuildingBlocks.Application.Exceptions;
using CleanOpsAi.BuildingBlocks.Application.Interfaces.Messaging;
using CleanOpsAi.BuildingBlocks.Infrastructure.Events.Request;
using CleanOpsAi.Modules.Scoring.Application.Common.Interfaces.Repositories;
using CleanOpsAi.Modules.Scoring.Application.Common.Interfaces.Services;
using CleanOpsAi.Modules.Scoring.Application.DTOs.Request;
using CleanOpsAi.Modules.Scoring.Application.DTOs.Response;
using CleanOpsAi.Modules.Scoring.Domain.Entities;
using CleanOpsAi.Modules.Scoring.Domain.Enums;
using CleanOpsAi.Modules.Scoring.IntegrationEvents;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace CleanOpsAi.Modules.Scoring.Application.Services
{
	public class ScoringJobService : IScoringJobService
	{
		private const int MaxBatchImages = 5;
		private const int MaxVisualizationConcurrency = 2;
		private const string SourceOfTruth = "visualize-link-only";
		private const string RuntimeServiceName = "scoring-job-service";
		private const string CodePathVersion = "visualize_single_source_v1";
		private static readonly SemaphoreSlim RetrainTriggerSemaphore = new(1, 1);
		private static readonly HashSet<string> AllowedReviewedVerdicts = new(StringComparer.OrdinalIgnoreCase)
		{
			"PASS",
			"FAIL",
		};
		private static readonly JsonSerializerOptions PayloadSerializerOptions = new()
		{
			DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.Never
		};
		private readonly IScoringJobRepository _repository;
		private readonly IScoringInferenceClient _inferenceClient;
		private readonly IEventBus _eventBus;
		private readonly IUserContext _userContext;
		private readonly ISupervisorManagedWorkerQueryService _supervisorManagedWorkerQueryService;
		private readonly IWorkerLookupQueryService _workerLookupQueryService;
		private readonly IScoringAnnotationArtifactService _scoringAnnotationArtifactService;
		private readonly IScoringRetrainRequestHandler _scoringRetrainRequestHandler;
		private readonly ILogger<ScoringJobService> _logger;

		public ScoringJobService(
			IScoringJobRepository repository,
			IScoringInferenceClient inferenceClient,
			IEventBus eventBus,
			IUserContext userContext,
			ISupervisorManagedWorkerQueryService supervisorManagedWorkerQueryService,
			IWorkerLookupQueryService workerLookupQueryService,
			IScoringAnnotationArtifactService scoringAnnotationArtifactService,
			IScoringRetrainRequestHandler scoringRetrainRequestHandler,
			ILogger<ScoringJobService> logger)
		{
			_repository = repository;
			_inferenceClient = inferenceClient;
			_eventBus = eventBus;
			_userContext = userContext;
			_supervisorManagedWorkerQueryService = supervisorManagedWorkerQueryService;
			_workerLookupQueryService = workerLookupQueryService;
			_scoringAnnotationArtifactService = scoringAnnotationArtifactService;
			_scoringRetrainRequestHandler = scoringRetrainRequestHandler;
			_logger = logger;
		}

		public async Task<SubmitScoringJobResponse> SubmitAsync(CreateScoringJobRequest request, CancellationToken ct = default)
		{
			request ??= new CreateScoringJobRequest();

			var imageUrls = request.ImageUrls
				.Where(x => !string.IsNullOrWhiteSpace(x))
				.Select(x => x.Trim())
				.Distinct(StringComparer.OrdinalIgnoreCase)
				.ToList();

			if (imageUrls.Count == 0)
			{
				throw new ArgumentException("At least one image URL is required.");
			}

			if (imageUrls.Count > MaxBatchImages)
			{
				throw new ArgumentException($"Maximum {MaxBatchImages} images per request.");
			}

			var requestId = string.IsNullOrWhiteSpace(request.RequestId)
				? Guid.NewGuid().ToString("N")
				: request.RequestId.Trim();

			var existingJob = await _repository.GetByRequestIdAsync(requestId, ct);
			if (existingJob is not null)
			{
				return new SubmitScoringJobResponse
				{
					JobId = existingJob.Id,
					RequestId = existingJob.RequestId,
					Status = existingJob.Status.ToString().ToUpperInvariant(),
					QueuedAt = existingJob.Created,
				};
			}

			var now = DateTime.UtcNow;
			var job = new ScoringJob
			{
				Id = Guid.NewGuid(),
				RequestId = requestId,
				EnvironmentKey = string.IsNullOrWhiteSpace(request.EnvironmentKey)
					? "LOBBY_CORRIDOR"
					: request.EnvironmentKey.Trim().ToUpperInvariant(),
				Status = ScoringJobStatus.Queued,
				SubmittedByUserId = ResolveSubmittedByUserId(request.SubmittedByUserId),
				Created = now,
				LastModified = now,
				CreatedBy = string.IsNullOrWhiteSpace(_userContext.Email) ? null : _userContext.Email,
				LastModifiedBy = string.IsNullOrWhiteSpace(_userContext.Email) ? null : _userContext.Email,
			};

			await _repository.InsertAsync(job, ct);
			await _repository.SaveChangesAsync(ct);

			await _eventBus.PublishAsync(new ScoringJobRequestedEvent
			{
				JobId = job.Id,
				RequestId = job.RequestId,
				EnvironmentKey = job.EnvironmentKey,
				ImageUrls = imageUrls,
				SubmittedByUserId = job.SubmittedByUserId,
			}, ct);

			return new SubmitScoringJobResponse
			{
				JobId = job.Id,
				RequestId = job.RequestId,
				Status = job.Status.ToString().ToUpperInvariant(),
				QueuedAt = job.Created,
			};
		}

		public async Task<ScoringJobDetailResponse?> GetByIdAsync(Guid jobId, CancellationToken ct = default)
		{
			var job = await _repository.GetByIdWithResultsAsync(jobId, ct);
			if (job is null)
			{
				return null;
			}

			return MapToDetail(job);
		}

		public async Task<IReadOnlyCollection<ScoringJobListItemResponse>> GetJobsAsync(string? status = null, int take = 50, CancellationToken ct = default)
		{
			ScoringJobStatus? parsedStatus = null;
			if (!string.IsNullOrWhiteSpace(status))
			{
				if (!Enum.TryParse<ScoringJobStatus>(status, true, out var parsed))
				{
					throw new ArgumentException($"Unsupported scoring status '{status}'.", nameof(status));
				}

				parsedStatus = parsed;
			}

			var jobs = await _repository.GetJobsAsync(parsedStatus, take, ct);
			return jobs.Select(MapToListItem).ToList();
		}

		public async Task<IReadOnlyCollection<PendingScoringReviewItemResponse>> GetPendingResultsAsync(int take = 100, CancellationToken ct = default)
		{
			var managedWorkerUserIds = await GetScopedManagedWorkerUserIdsAsync(ct);
			if (IsSupervisor() && (managedWorkerUserIds?.Count ?? 0) == 0)
			{
				return Array.Empty<PendingScoringReviewItemResponse>();
			}

			var pendingResults = await _repository.GetPendingResultsAsync(take, managedWorkerUserIds, ct);
			var workerLookup = await BuildWorkerLookupAsync(pendingResults, ct);

			return pendingResults
				.Select(x => new PendingScoringReviewItemResponse
				{
					ResultId = x.Id,
					JobId = x.ScoringJobId,
					RequestId = x.ScoringJob.RequestId,
					SubmittedByUserId = x.ScoringJob.SubmittedByUserId,
					WorkerId = TryGetWorkerId(workerLookup, x.ScoringJob.SubmittedByUserId),
					WorkerName = TryGetWorkerName(workerLookup, x.ScoringJob.SubmittedByUserId),
					EnvironmentKey = x.ScoringJob.EnvironmentKey,
					SourceType = x.SourceType,
					Source = x.Source,
					Verdict = x.Verdict,
					QualityScore = x.QualityScore,
					CreatedAt = x.Created,
				})
				.ToList();
		}

		public async Task<IReadOnlyCollection<ScoringAnnotationCandidateListItemResponse>> GetAnnotationCandidatesAsync(
			string? status = null,
			string? environmentKey = null,
			Guid? assignedToUserId = null,
			DateTime? createdFromUtc = null,
			int take = 50,
			CancellationToken ct = default)
		{
			EnsureAnnotationParticipant();

			ScoringAnnotationCandidateStatus? parsedStatus = null;
			if (!string.IsNullOrWhiteSpace(status))
			{
				if (!Enum.TryParse<ScoringAnnotationCandidateStatus>(status, true, out var parsed))
				{
					throw new ArgumentException($"Unsupported annotation candidate status '{status}'.", nameof(status));
				}

				parsedStatus = parsed;
			}

			var managedWorkerUserIds = await GetScopedManagedWorkerUserIdsAsync(ct);
			if (IsSupervisor() && (managedWorkerUserIds?.Count ?? 0) == 0)
			{
				return Array.Empty<ScoringAnnotationCandidateListItemResponse>();
			}

			var candidates = await _repository.GetAnnotationCandidatesAsync(
				parsedStatus,
				environmentKey,
				assignedToUserId,
				createdFromUtc,
				take,
				managedWorkerUserIds,
				ct);

			return candidates.Select(MapAnnotationCandidateListItem).ToList();
		}

		public async Task<ScoringAnnotationCandidateDetailResponse?> GetAnnotationCandidateByIdAsync(Guid candidateId, CancellationToken ct = default)
		{
			EnsureAnnotationParticipant();

			var candidate = await _repository.GetAnnotationCandidateByIdAsync(candidateId, ct);
			if (candidate is null)
			{
				return null;
			}

			await EnsureCanReadCandidateAsync(candidate, ct);
			return MapAnnotationCandidateDetail(candidate);
		}

		public async Task<ScoringAnnotationCandidateDetailResponse?> ClaimAnnotationCandidateAsync(Guid candidateId, CancellationToken ct = default)
		{
			EnsureAnnotationSupervisorOrAdmin();

			var candidate = await _repository.GetAnnotationCandidateByIdAsync(candidateId, ct);
			if (candidate is null)
			{
				return null;
			}

			await EnsureCanManageCandidateAsync(candidate, ct);
			if (candidate.CandidateStatus == ScoringAnnotationCandidateStatus.Approved)
			{
				throw new InvalidOperationException("Approved annotation candidates cannot be claimed.");
			}

			var now = DateTime.UtcNow;
			candidate.AssignedToUserId = _userContext.UserId == Guid.Empty ? null : _userContext.UserId;
			if (candidate.CandidateStatus == ScoringAnnotationCandidateStatus.Queued)
			{
				candidate.CandidateStatus = ScoringAnnotationCandidateStatus.InProgress;
			}
			candidate.LastModified = now;
			candidate.LastModifiedBy = GetActorIdentifier();
			await _repository.SaveChangesAsync(ct);

			return MapAnnotationCandidateDetail(candidate);
		}

		public async Task<ScoringAnnotationCandidateDetailResponse?> UpsertAnnotationCandidateAsync(
			Guid candidateId,
			UpsertScoringAnnotationRequest request,
			CancellationToken ct = default)
		{
			EnsureAnnotationSupervisorOrAdmin();
			if (request is null)
			{
				throw new ArgumentException("Annotation payload cannot be null.", nameof(request));
			}

			if (request.Labels.ValueKind != JsonValueKind.Array)
			{
				throw new ArgumentException("Annotation labels must be a JSON array.", nameof(request));
			}

			var candidate = await _repository.GetAnnotationCandidateByIdAsync(candidateId, ct);
			if (candidate is null)
			{
				return null;
			}

			await EnsureCanManageCandidateAsync(candidate, ct);
			if (candidate.CandidateStatus == ScoringAnnotationCandidateStatus.Approved)
			{
				throw new InvalidOperationException("Approved annotation candidates cannot be edited.");
			}

			var annotationFormat = ParseAnnotationFormat(request.AnnotationFormat);
			var now = DateTime.UtcNow;
			var normalizedNote = string.IsNullOrWhiteSpace(request.ReviewerNote) ? null : request.ReviewerNote.Trim();

			var annotation = candidate.Annotation;
			if (annotation is null)
			{
				annotation = new ScoringAnnotation
				{
					Id = Guid.NewGuid(),
					CandidateId = candidate.Id,
					AnnotationFormat = annotationFormat,
					LabelsJson = request.Labels.GetRawText(),
					ReviewerNote = normalizedNote,
					Version = 1,
					CreatedByUserId = _userContext.UserId == Guid.Empty ? null : _userContext.UserId,
					Created = now,
					LastModified = now,
					CreatedBy = GetActorIdentifier(),
					LastModifiedBy = GetActorIdentifier(),
				};
				await _repository.InsertAnnotationAsync(annotation, ct);
				candidate.Annotation = annotation;
			}
			else
			{
				annotation.AnnotationFormat = annotationFormat;
				annotation.LabelsJson = request.Labels.GetRawText();
				annotation.ReviewerNote = normalizedNote;
				annotation.Version += 1;
				annotation.LastModified = now;
				annotation.LastModifiedBy = GetActorIdentifier();
				if (!annotation.CreatedByUserId.HasValue && _userContext.UserId != Guid.Empty)
				{
					annotation.CreatedByUserId = _userContext.UserId;
				}
			}

			candidate.AssignedToUserId = _userContext.UserId == Guid.Empty ? candidate.AssignedToUserId : _userContext.UserId;
			candidate.CandidateStatus = request.Submit
				? ScoringAnnotationCandidateStatus.Submitted
				: ScoringAnnotationCandidateStatus.InProgress;
			candidate.SubmittedAtUtc = request.Submit ? now : null;
			candidate.LastModified = now;
			candidate.LastModifiedBy = GetActorIdentifier();
			await _repository.SaveChangesAsync(ct);

			return MapAnnotationCandidateDetail(candidate);
		}

		public async Task<ScoringAnnotationCandidateDetailResponse?> ApproveAnnotationCandidateAsync(
			Guid candidateId,
			ApproveScoringAnnotationCandidateRequest? request,
			CancellationToken ct = default)
		{
			EnsureAnnotationSupervisorOrAdmin();

			var candidate = await _repository.GetAnnotationCandidateByIdAsync(candidateId, ct);
			if (candidate is null)
			{
				return null;
			}

			await EnsureCanManageCandidateAsync(candidate, ct);
			if (candidate.Annotation is null)
			{
				throw new InvalidOperationException("Annotation candidate must have annotation data before approval.");
			}

			if (candidate.CandidateStatus == ScoringAnnotationCandidateStatus.Approved)
			{
				return MapAnnotationCandidateDetail(candidate);
			}

			var now = DateTime.UtcNow;
			Guid? actorId = _userContext.UserId == Guid.Empty ? null : _userContext.UserId;
			if (!string.IsNullOrWhiteSpace(request?.Note))
			{
				candidate.Annotation.ReviewerNote = request.Note.Trim();
			}

			candidate.CandidateStatus = ScoringAnnotationCandidateStatus.Approved;
			candidate.ApprovedAtUtc = now;
			candidate.SubmittedAtUtc ??= now;
			candidate.LastModified = now;
			candidate.LastModifiedBy = GetActorIdentifier();
			candidate.Annotation.ApprovedByUserId = actorId;
			candidate.Annotation.LastModified = now;
			candidate.Annotation.LastModifiedBy = GetActorIdentifier();

			if ((string.IsNullOrWhiteSpace(candidate.SnapshotBlobKey) || string.IsNullOrWhiteSpace(candidate.MetadataBlobKey)) &&
				candidate.Result is not null)
			{
				await _scoringAnnotationArtifactService.EnsureReviewedSnapshotAsync(candidate, candidate.Result, ct);
			}

			await _repository.SaveChangesAsync(ct);

			try
			{
				await _scoringAnnotationArtifactService.PublishApprovedAnnotationAsync(candidate, candidate.Annotation, ct);
			}
			catch
			{
				candidate.CandidateStatus = ScoringAnnotationCandidateStatus.Submitted;
				candidate.ApprovedAtUtc = null;
				candidate.LastModified = DateTime.UtcNow;
				candidate.LastModifiedBy = GetActorIdentifier();
				candidate.Annotation.ApprovedByUserId = null;
				candidate.Annotation.LastModified = candidate.LastModified;
				candidate.Annotation.LastModifiedBy = candidate.LastModifiedBy;
				await _repository.SaveChangesAsync(ct);
				throw;
			}

			await _eventBus.PublishAsync(new ScoringAnnotationApprovedEvent
			{
				CandidateId = candidate.Id,
				AnnotationId = candidate.Annotation.Id,
				ResultId = candidate.ResultId,
				JobId = candidate.JobId,
				RequestId = candidate.RequestId,
				EnvironmentKey = candidate.EnvironmentKey,
				ApprovedAtUtc = now,
				ApprovedByUserId = actorId,
				ApprovedByEmail = string.IsNullOrWhiteSpace(_userContext.Email) ? null : _userContext.Email,
			}, ct);

			return MapAnnotationCandidateDetail(candidate);
		}

		public async Task<ScoringAnnotationCandidateDetailResponse?> RejectAnnotationCandidateAsync(
			Guid candidateId,
			RejectScoringAnnotationCandidateRequest? request,
			CancellationToken ct = default)
		{
			EnsureAnnotationSupervisorOrAdmin();

			var candidate = await _repository.GetAnnotationCandidateByIdAsync(candidateId, ct);
			if (candidate is null)
			{
				return null;
			}

			await EnsureCanManageCandidateAsync(candidate, ct);
			if (candidate.CandidateStatus == ScoringAnnotationCandidateStatus.Approved)
			{
				throw new InvalidOperationException("Approved annotation candidates cannot be rejected.");
			}

			var now = DateTime.UtcNow;
			candidate.CandidateStatus = ScoringAnnotationCandidateStatus.Rejected;
			candidate.LastModified = now;
			candidate.LastModifiedBy = GetActorIdentifier();
			if (candidate.Annotation is not null && !string.IsNullOrWhiteSpace(request?.Reason))
			{
				candidate.Annotation.ReviewerNote = request.Reason.Trim();
				candidate.Annotation.LastModified = now;
				candidate.Annotation.LastModifiedBy = GetActorIdentifier();
			}

			await _repository.SaveChangesAsync(ct);
			return MapAnnotationCandidateDetail(candidate);
		}

		public async Task<IReadOnlyCollection<ScoringRetrainBatchListItemResponse>> GetRetrainBatchesAsync(string? status = null, int take = 50, CancellationToken ct = default)
		{
			ScoringRetrainBatchStatus? parsedStatus = null;
			if (!string.IsNullOrWhiteSpace(status))
			{
				if (!Enum.TryParse<ScoringRetrainBatchStatus>(status, true, out var parsed))
				{
					throw new ArgumentException($"Unsupported retrain batch status '{status}'.", nameof(status));
				}

				parsedStatus = parsed;
			}

			var batches = await _repository.GetRetrainBatchesAsync(parsedStatus, take, ct);
			return batches.Select(MapRetrainBatchListItem).ToList();
		}

		public async Task<ScoringResultReviewResponse?> ReviewPendingResultAsync(Guid resultId, ReviewScoringResultRequest request, CancellationToken ct = default)
		{
			if (resultId == Guid.Empty)
			{
				throw new ArgumentException("Result id cannot be empty.", nameof(resultId));
			}

			if (request is null)
			{
				throw new ArgumentException("Review payload cannot be null.", nameof(request));
			}

			var reviewedVerdict = string.IsNullOrWhiteSpace(request.Verdict)
				? string.Empty
				: request.Verdict.Trim().ToUpperInvariant();

			if (!AllowedReviewedVerdicts.Contains(reviewedVerdict))
			{
				throw new ArgumentException("Reviewed verdict must be PASS or FAIL.", nameof(request));
			}

			var result = await _repository.GetResultByIdWithJobAsync(resultId, ct);
			if (result is null)
			{
				return null;
			}

			await EnsureCanReviewResultAsync(result, ct);

			var originalVerdict = string.IsNullOrWhiteSpace(result.Verdict)
				? "UNKNOWN"
				: result.Verdict.Trim().ToUpperInvariant();

			if (!string.Equals(originalVerdict, "PENDING", StringComparison.OrdinalIgnoreCase))
			{
				throw new InvalidOperationException($"Only PENDING results can be reviewed. Current verdict: {originalVerdict}.");
			}

			var now = DateTime.UtcNow;
			var reviewReason = string.IsNullOrWhiteSpace(request.Reason)
				? null
				: request.Reason.Trim();

			var payloadNode = BuildPayloadWithHumanReview(
				result.PayloadJson,
				originalVerdict,
				reviewedVerdict,
				reviewReason,
				now,
				_userContext.IsAuthenticated ? _userContext.UserId : null,
				string.IsNullOrWhiteSpace(_userContext.Email) ? null : _userContext.Email);

			result.Verdict = reviewedVerdict;
			result.PayloadJson = payloadNode.ToJsonString(PayloadSerializerOptions);
			result.LastModified = now;
			result.LastModifiedBy = string.IsNullOrWhiteSpace(_userContext.Email) ? null : _userContext.Email;

			if (result.ScoringJob is not null)
			{
				result.ScoringJob.LastModified = now;
				result.ScoringJob.LastModifiedBy = result.LastModifiedBy;
			}

			await _repository.SaveChangesAsync(ct);
			var annotationCandidate = await EnsureAnnotationCandidateForReviewedFailAsync(result, originalVerdict, reviewedVerdict, now, ct);
			if (annotationCandidate is not null)
			{
				await _scoringAnnotationArtifactService.EnsureReviewedSnapshotAsync(annotationCandidate, result, ct);
				await _repository.SaveChangesAsync(ct);
			}

			if (result.ScoringJob is not null)
			{
				await _eventBus.PublishAsync(new ScoringResultReviewedEvent
				{
					JobId = result.ScoringJob.Id,
					ResultId = result.Id,
					RequestId = result.ScoringJob.RequestId,
					EnvironmentKey = result.ScoringJob.EnvironmentKey,
					SourceType = result.SourceType,
					Source = result.Source,
					OriginalVerdict = originalVerdict,
					ReviewedVerdict = reviewedVerdict,
					ReviewReason = reviewReason,
					ReviewedAtUtc = now,
					ReviewedByUserId = _userContext.IsAuthenticated ? _userContext.UserId : null,
					ReviewedByEmail = string.IsNullOrWhiteSpace(_userContext.Email) ? null : _userContext.Email,
				}, ct);
			}

			return new ScoringResultReviewResponse
			{
				ResultId = result.Id,
				JobId = result.ScoringJobId,
				OriginalVerdict = originalVerdict,
				ReviewedVerdict = reviewedVerdict,
				ReviewReason = reviewReason,
				ReviewedAtUtc = now,
				ReviewedByUserId = _userContext.IsAuthenticated ? _userContext.UserId : null,
				ReviewedByEmail = string.IsNullOrWhiteSpace(_userContext.Email) ? null : _userContext.Email,
			};
		}

		private async Task<ScoringAnnotationCandidate?> EnsureAnnotationCandidateForReviewedFailAsync(
			ScoringJobResult result,
			string originalVerdict,
			string reviewedVerdict,
			DateTime reviewedAtUtc,
			CancellationToken ct)
		{
			if (!string.Equals(originalVerdict, "PENDING", StringComparison.OrdinalIgnoreCase) ||
				!string.Equals(reviewedVerdict, "FAIL", StringComparison.OrdinalIgnoreCase))
			{
				return null;
			}

			if (result.ScoringJob is null)
			{
				return null;
			}

			var existing = await _repository.GetAnnotationCandidateByResultIdAsync(result.Id, ct);
			if (existing is not null)
			{
				return existing;
			}

			var candidate = new ScoringAnnotationCandidate
			{
				Id = Guid.NewGuid(),
				ResultId = result.Id,
				JobId = result.ScoringJobId,
				RequestId = result.ScoringJob.RequestId,
				EnvironmentKey = result.ScoringJob.EnvironmentKey,
				ImageUrl = result.Source,
				VisualizationBlobUrl = ExtractVisualizationBlobUrl(result.PayloadJson),
				OriginalVerdict = originalVerdict,
				ReviewedVerdict = reviewedVerdict,
				SourceType = "reviewed-fail-from-pending",
				CandidateStatus = ScoringAnnotationCandidateStatus.Queued,
				CreatedAtUtc = reviewedAtUtc,
				Created = reviewedAtUtc,
				LastModified = reviewedAtUtc,
				CreatedBy = string.IsNullOrWhiteSpace(_userContext.Email) ? "system" : _userContext.Email,
				LastModifiedBy = string.IsNullOrWhiteSpace(_userContext.Email) ? "system" : _userContext.Email,
			};

			await _repository.InsertAnnotationCandidateAsync(candidate, ct);
			await _repository.SaveChangesAsync(ct);
			return candidate;
		}

		private async Task<IReadOnlyCollection<Guid>?> GetScopedManagedWorkerUserIdsAsync(CancellationToken ct)
		{
			if (!IsSupervisor())
			{
				return null;
			}

			if (_userContext.UserId == Guid.Empty)
			{
				return Array.Empty<Guid>();
			}

			return await _supervisorManagedWorkerQueryService.GetManagedWorkerUserIdsAsync(_userContext.UserId, ct);
		}

		private async Task EnsureCanReviewResultAsync(ScoringJobResult result, CancellationToken ct)
		{
			if (!IsSupervisor())
			{
				return;
			}

			var managedWorkerUserIds = await GetScopedManagedWorkerUserIdsAsync(ct) ?? Array.Empty<Guid>();
			var submittedByUserId = result.ScoringJob?.SubmittedByUserId;

			if (!submittedByUserId.HasValue || !managedWorkerUserIds.Contains(submittedByUserId.Value))
			{
				throw new ForbiddenException("You are not allowed to review scoring results outside your managed workers.");
			}
		}

		private async Task<IReadOnlyDictionary<Guid, WorkerLookupItem>> BuildWorkerLookupAsync(
			IReadOnlyCollection<ScoringJobResult> pendingResults,
			CancellationToken ct)
		{
			var submittedByUserIds = pendingResults
				.Select(x => x.ScoringJob.SubmittedByUserId)
				.Where(x => x.HasValue)
				.Select(x => x!.Value)
				.Distinct()
				.ToList();

			if (submittedByUserIds.Count == 0)
			{
				return new Dictionary<Guid, WorkerLookupItem>();
			}

			var workers = await _workerLookupQueryService.GetWorkersByUserIdsAsync(submittedByUserIds, ct);
			return workers.ToDictionary(x => x.UserId);
		}

		private static Guid? TryGetWorkerId(
			IReadOnlyDictionary<Guid, WorkerLookupItem> workerLookup,
			Guid? submittedByUserId)
		{
			if (!submittedByUserId.HasValue)
			{
				return null;
			}

			return workerLookup.TryGetValue(submittedByUserId.Value, out var worker)
				? worker.WorkerId
				: null;
		}

		private static string? TryGetWorkerName(
			IReadOnlyDictionary<Guid, WorkerLookupItem> workerLookup,
			Guid? submittedByUserId)
		{
			if (!submittedByUserId.HasValue)
			{
				return null;
			}

			return workerLookup.TryGetValue(submittedByUserId.Value, out var worker)
				? worker.FullName
				: null;
		}

		private bool IsSupervisor()
		{
			return RoleEquals("Supervisor", "4");
		}

		public async Task ProcessQueuedJobAsync(Guid jobId, string environmentKey, IReadOnlyCollection<string> imageUrls, CancellationToken ct = default)
		{
			var job = await _repository.GetByIdWithResultsAsync(jobId, ct);
			if (job is null)
			{
				return;
			}

			if (job.Status == ScoringJobStatus.Succeeded)
			{
				return;
			}

			job.Status = ScoringJobStatus.Processing;
			job.RetryCount += 1;
			job.LastModified = DateTime.UtcNow;
			await _repository.SaveChangesAsync(ct);
			_logger.LogInformation(
				"Processing scoring job {JobId} requestId={RequestId} env={EnvironmentKey} imageCount={ImageCount} scoring_job_source_of_truth={SourceOfTruth} code_path_version={CodePathVersion}",
				job.Id,
				job.RequestId,
				environmentKey,
				imageUrls.Count,
				SourceOfTruth,
				CodePathVersion);

			var visualizedResults = await EvaluateVisualizationResultsAsync(environmentKey, imageUrls, ct);
			var mappedResults = new List<ScoringJobResult>(visualizedResults.Count);
			foreach (var result in visualizedResults)
			{
				var sourceType = string.IsNullOrWhiteSpace(result.SourceType) ? "unknown" : result.SourceType!;
				var source = string.IsNullOrWhiteSpace(result.Source) ? "unknown-source" : result.Source!;
				var verdict = string.IsNullOrWhiteSpace(result.Scoring?.Verdict) ? "UNKNOWN" : result.Scoring!.Verdict!;

				mappedResults.Add(new ScoringJobResult
				{
					Id = Guid.NewGuid(),
					ScoringJobId = job.Id,
					SourceType = sourceType,
					Source = source,
					Verdict = verdict,
					QualityScore = result.Scoring?.QualityScore ?? 0,
					PayloadJson = BuildResultPayloadJson(result, DateTime.UtcNow),
					Created = DateTime.UtcNow,
					LastModified = DateTime.UtcNow,
				});
				_logger.LogInformation(
					"Mapped scoring result source={Source} verdict={Verdict} qualityScore={QualityScore} visualizationUrl={VisualizationUrl} scoring_job_source_of_truth={SourceOfTruth} code_path_version={CodePathVersion}",
					source,
					verdict,
					result.Scoring?.QualityScore ?? 0,
					result.Visualization?.Url,
					SourceOfTruth,
					CodePathVersion);
			}

			await _repository.ReplaceResultsAsync(job.Id, mappedResults, ct);
			job.Results = mappedResults;

			job.Status = ScoringJobStatus.Succeeded;
			job.CompletedAt = DateTime.UtcNow;
			job.FailureReason = null;
			job.LastModified = DateTime.UtcNow;

			await _repository.SaveChangesAsync(ct);

			await _eventBus.PublishAsync(new ScoringJobCompletedEvent
			{
				JobId = job.Id,
				RequestId = job.RequestId,
				TotalRequested = imageUrls.Count,
				ProcessedCount = mappedResults.Count,
				SkippedCount = Math.Max(0, imageUrls.Count - mappedResults.Count),
				PassCount = mappedResults.Count(x => string.Equals(x.Verdict, "PASS", StringComparison.OrdinalIgnoreCase)),
				PendingCount = mappedResults.Count(x => string.Equals(x.Verdict, "PENDING", StringComparison.OrdinalIgnoreCase)),
				FailCount = mappedResults.Count(x => string.Equals(x.Verdict, "FAIL", StringComparison.OrdinalIgnoreCase)),
			}, ct);

			await _eventBus.PublishAsync(new ScoringCompletedEvent
			{
				RequestId = job.RequestId,
				Results = mappedResults.Select(r => new ScoringResultItem
				{
					ImageUrl = r.Source,
					QualityScore = r.QualityScore,
					Verdict = r.Verdict,
					VisualizationBlobUrl = ExtractVisualizationBlobUrl(r.PayloadJson)
				}).ToList()
			});
		}

		public async Task<ScoringRetrainBatchDetailResponse> TriggerRetrainAsync(TriggerScoringRetrainRequest request, CancellationToken ct = default)
		{
			request ??= new TriggerScoringRetrainRequest();

			var now = DateTime.UtcNow;
			var lookbackDays = Math.Max(1, request.LookbackDays);
			var minApprovedAnnotations = Math.Max(1, request.MinApprovedAnnotations);
			var maxSamplesPerBatch = Math.Clamp(Math.Max(request.MaxSamplesPerBatch, minApprovedAnnotations), 1, 5000);
			var sinceUtc = await ResolveRetrainSourceWindowFromUtcAsync(request, lookbackDays, now, ct);

			await RetrainTriggerSemaphore.WaitAsync(ct);
			ScoringRetrainBatch batch;
			IReadOnlyCollection<ScoringJobResult> reviewedResults;
			IReadOnlyCollection<ScoringAnnotationCandidate> annotatedCandidates;
			IReadOnlyCollection<ScoringAnnotationCandidate> approvedAnnotationCandidates;

			try
			{
				if (await _repository.HasActiveRetrainBatchAsync(ct))
				{
					throw new InvalidOperationException("A scoring retrain batch is already queued or running.");
				}

				reviewedResults = await _repository.GetReviewedResultsForRetrainAsync(sinceUtc, maxSamplesPerBatch, ct);
				annotatedCandidates = await _repository.GetAnnotatedCandidatesForRetrainAsync(sinceUtc, maxSamplesPerBatch, ct);
				approvedAnnotationCandidates = await _repository.GetApprovedAnnotationCandidatesForRetrainAsync(sinceUtc, maxSamplesPerBatch, ct);
				if (approvedAnnotationCandidates.Count < minApprovedAnnotations)
				{
					throw new InvalidOperationException(
						$"Not enough approved annotation samples to trigger retrain. Found {approvedAnnotationCandidates.Count}, required {minApprovedAnnotations}.");
				}

				batch = new ScoringRetrainBatch
				{
					Id = Guid.NewGuid(),
					RequestedAtUtc = now,
					SourceWindowFromUtc = sinceUtc,
					ReviewedSampleCount = reviewedResults.Count,
					AnnotatedSampleCount = annotatedCandidates.Count,
					ApprovedAnnotationCount = approvedAnnotationCandidates.Count,
					CalibrationSampleCount = approvedAnnotationCandidates.Count,
					Status = ScoringRetrainBatchStatus.Queued,
					Created = now,
					LastModified = now,
					CreatedBy = string.IsNullOrWhiteSpace(_userContext.Email) ? "system" : _userContext.Email,
					LastModifiedBy = string.IsNullOrWhiteSpace(_userContext.Email) ? "system" : _userContext.Email,
				};

				await _repository.InsertRetrainBatchAsync(batch, ct);
				await _repository.SaveChangesAsync(ct);
			}
			finally
			{
				RetrainTriggerSemaphore.Release();
			}

			var retrainRequestedEvent = new ScoringRetrainRequestedEvent
			{
				BatchId = batch.Id,
				RequestedAtUtc = batch.RequestedAtUtc,
				SourceWindowFromUtc = batch.SourceWindowFromUtc,
				ReviewedSampleCount = batch.ReviewedSampleCount,
				ApprovedAnnotationCount = batch.ApprovedAnnotationCount,
				MinApprovedAnnotations = minApprovedAnnotations,
				MaxSamplesPerBatch = maxSamplesPerBatch,
				Samples = reviewedResults.Select(MapRetrainSample).ToList(),
			};

			if (_scoringRetrainRequestHandler.InlineExecutionEnabled)
			{
				_logger.LogInformation(
					"Executing scoring retrain inline for batch {BatchId} because inline local mode is enabled.",
					batch.Id);
				// The retrain job can outlive the HTTP request. Do not let a client timeout
				// cancel the batch and leave it stuck in RUNNING after the trainer completes.
				await _scoringRetrainRequestHandler.HandleAsync(retrainRequestedEvent, CancellationToken.None);
			}
			else
			{
				await _eventBus.PublishAsync(retrainRequestedEvent, ct);
			}

			return MapRetrainBatch(batch);
		}

		private async Task<DateTime> ResolveRetrainSourceWindowFromUtcAsync(
			TriggerScoringRetrainRequest request,
			int lookbackDays,
			DateTime now,
			CancellationToken ct)
		{
			if (!request.UseLastBatchTime)
			{
				return now.AddDays(-lookbackDays);
			}

			var latestBatch = await _repository.GetLatestRetrainBatchAsync(ct);
			return latestBatch?.RequestedAtUtc ?? now.AddDays(-lookbackDays);
		}

		private async Task<List<ScoringVisualizationLinkResponse>> EvaluateVisualizationResultsAsync(
			string environmentKey,
			IReadOnlyCollection<string> imageUrls,
			CancellationToken ct)
		{
			var sourceUrls = imageUrls
				.Where(x => !string.IsNullOrWhiteSpace(x))
				.Select(x => x.Trim())
				.Distinct(StringComparer.OrdinalIgnoreCase)
				.ToList();
			var results = new ScoringVisualizationLinkResponse[sourceUrls.Count];
			using var gate = new SemaphoreSlim(MaxVisualizationConcurrency);

			var tasks = sourceUrls.Select(async (sourceUrl, index) =>
			{
				await gate.WaitAsync(ct);
				try
				{
					results[index] = await _inferenceClient.EvaluateUrlVisualizeLinkAsync(environmentKey, sourceUrl, ct);
				}
				catch (Exception ex)
				{
					_logger.LogWarning(
						ex,
						"Failed to evaluate visualization link for source {SourceUrl}. Persisting failed placeholder payload.",
						sourceUrl);
					results[index] = BuildFailedVisualizationResponse(sourceUrl, environmentKey, ex);
				}
				finally
				{
					gate.Release();
				}
			});

			await Task.WhenAll(tasks);
			return results.ToList();
		}

		private static string BuildResultPayloadJson(ScoringVisualizationLinkResponse result, DateTime generatedAtUtc)
		{
			var payloadNode = JsonSerializer.SerializeToNode(result, PayloadSerializerOptions) as JsonObject
				?? new JsonObject();

			var blobUrl = result.Visualization?.Url;
			if (!string.IsNullOrWhiteSpace(blobUrl))
			{
				payloadNode["visualization_blob_url"] = blobUrl;
			}
			payloadNode["backend_runtime"] = new JsonObject
			{
				["source_of_truth"] = SourceOfTruth,
				["service"] = RuntimeServiceName,
				["generated_at_utc"] = generatedAtUtc,
				["code_path_version"] = CodePathVersion,
			};

			return payloadNode.ToJsonString(PayloadSerializerOptions);
		}

		private static ScoringVisualizationLinkResponse BuildFailedVisualizationResponse(
			string sourceUrl,
			string environmentKey,
			Exception ex)
		{
			var message = ex.Message.Length > 500 ? ex.Message[..500] : ex.Message;
			return new ScoringVisualizationLinkResponse
			{
				SourceType = "url",
				Source = sourceUrl,
				EnvironmentKey = environmentKey,
				Scoring = new ScoringInferenceScore
				{
					Verdict = "FAIL",
					QualityScore = 0,
					BaseCleanScore = 0,
					ObjectPenalty = 0,
					PassThreshold = null,
					Reasons = new List<string> { "visualization evaluation failed" },
				},
				AdditionalData = new Dictionary<string, JsonElement>
				{
					["error"] = JsonSerializer.SerializeToElement(new
					{
						message,
						type = ex.GetType().Name,
					})
				}
			};
		}

		private static string? ExtractVisualizationBlobUrl(string payloadJson)
		{
			if (string.IsNullOrWhiteSpace(payloadJson))
			{
				return null;
			}

			try
			{
				var node = JsonNode.Parse(payloadJson) as JsonObject;
				if (node is null)
				{
					return null;
				}

				var direct = node["visualization_blob_url"]?.GetValue<string>();
				if (!string.IsNullOrWhiteSpace(direct))
				{
					return direct;
				}

				var legacy = node["visualization"]?["url"]?.GetValue<string>();
				if (!string.IsNullOrWhiteSpace(legacy))
				{
					return legacy;
				}
			}
			catch
			{
				return null;
			}

			return null;
		}

		private static JsonObject BuildPayloadWithHumanReview(
			string payloadJson,
			string originalVerdict,
			string reviewedVerdict,
			string? reviewReason,
			DateTime reviewedAtUtc,
			Guid? reviewedByUserId,
			string? reviewedByEmail)
		{
			var payloadNode = JsonNode.Parse(string.IsNullOrWhiteSpace(payloadJson) ? "{}" : payloadJson) as JsonObject
				?? new JsonObject();

			JsonObject CreateReviewEntry()
			{
				return new JsonObject
				{
					["original_verdict"] = originalVerdict,
					["reviewed_verdict"] = reviewedVerdict,
					["review_reason"] = reviewReason,
					["reviewed_at_utc"] = reviewedAtUtc,
					["reviewed_by_user_id"] = reviewedByUserId?.ToString(),
					["reviewed_by_email"] = reviewedByEmail,
				};
			}

			var reviewHistory = payloadNode["human_review_history"] as JsonArray ?? new JsonArray();
			reviewHistory.Add(CreateReviewEntry());
			payloadNode["human_review_history"] = reviewHistory;
			payloadNode["human_review"] = CreateReviewEntry();

			return payloadNode;
		}

		public async Task MarkFailedAsync(Guid jobId, string reason, CancellationToken ct = default)
		{
			var job = await _repository.GetByIdWithResultsAsync(jobId, ct);
			if (job is null)
			{
				return;
			}

			job.Status = ScoringJobStatus.Failed;
			job.CompletedAt = DateTime.UtcNow;
			job.FailureReason = reason.Length > 2000 ? reason[..2000] : reason;
			job.LastModified = DateTime.UtcNow;
			await _repository.SaveChangesAsync(ct);

			await _eventBus.PublishAsync(new ScoringJobFailedEvent
			{
				JobId = job.Id,
				RequestId = job.RequestId,
				Reason = job.FailureReason,
			}, ct);
		}

		public async Task<ScoringRetrainBatchDetailResponse?> GetRetrainBatchByIdAsync(Guid batchId, CancellationToken ct = default)
		{
			var batch = await _repository.GetRetrainBatchByIdWithRunsAsync(batchId, ct);
			return batch is null ? null : MapRetrainBatch(batch);
		}

		private void EnsureAnnotationParticipant()
		{
			if (IsAdmin() || IsManager() || IsSupervisor())
			{
				return;
			}

			throw new ForbiddenException("Only Supervisor, Manager or Admin can access scoring annotations.");
		}

		private void EnsureAnnotationSupervisorOrAdmin()
		{
			if (IsAdmin() || IsSupervisor())
			{
				return;
			}

			throw new ForbiddenException("Only Supervisor or Admin can manage scoring annotations.");
		}

		private async Task EnsureCanReadCandidateAsync(ScoringAnnotationCandidate candidate, CancellationToken ct)
		{
			if (!IsSupervisor())
			{
				return;
			}

			await EnsureSupervisorCandidateScopeAsync(candidate, ct);
		}

		private async Task EnsureCanManageCandidateAsync(ScoringAnnotationCandidate candidate, CancellationToken ct)
		{
			if (IsAdmin())
			{
				return;
			}

			await EnsureSupervisorCandidateScopeAsync(candidate, ct);

			if (_userContext.UserId == Guid.Empty)
			{
				throw new ForbiddenException("Authenticated user is required to manage annotation candidates.");
			}

			if (candidate.AssignedToUserId.HasValue && candidate.AssignedToUserId.Value != _userContext.UserId)
			{
				throw new ForbiddenException("This annotation candidate is already assigned to another reviewer.");
			}
		}

		private async Task EnsureSupervisorCandidateScopeAsync(ScoringAnnotationCandidate candidate, CancellationToken ct)
		{
			var managedWorkerUserIds = await GetScopedManagedWorkerUserIdsAsync(ct) ?? Array.Empty<Guid>();
			var submittedByUserId = candidate.Result?.ScoringJob?.SubmittedByUserId;

			if (!submittedByUserId.HasValue || !managedWorkerUserIds.Contains(submittedByUserId.Value))
			{
				throw new ForbiddenException("You are not allowed to access annotation candidates outside your managed workers.");
			}
		}

		private static ScoringRetrainSampleItem MapRetrainSample(ScoringJobResult result)
		{
			var reviewedAtUtc = result.LastModified;
			var reviewedByEmail = default(string);

			if (!string.IsNullOrWhiteSpace(result.PayloadJson))
			{
				try
				{
					var root = JsonNode.Parse(result.PayloadJson) as JsonObject;
					var review = root?["human_review"] as JsonObject;
					var reviewedAtRaw = review?["reviewed_at_utc"]?.GetValue<string>();
					if (DateTime.TryParse(reviewedAtRaw, out var parsedReviewedAt))
					{
						reviewedAtUtc = DateTime.SpecifyKind(parsedReviewedAt, DateTimeKind.Utc);
					}

					reviewedByEmail = review?["reviewed_by_email"]?.GetValue<string>();
				}
				catch
				{
					// Keep fallback metadata when payload cannot be parsed.
				}
			}

			return new ScoringRetrainSampleItem
			{
				ResultId = result.Id,
				JobId = result.ScoringJobId,
				RequestId = result.ScoringJob.RequestId,
				EnvironmentKey = result.ScoringJob.EnvironmentKey,
				SourceType = result.SourceType,
				Source = result.Source,
				ReviewedVerdict = result.Verdict,
				ReviewedAtUtc = reviewedAtUtc,
				ReviewedByEmail = reviewedByEmail,
			};
		}

		private static ScoringJobDetailResponse MapToDetail(ScoringJob job)
		{
			var orderedResults = job.Results
				.OrderBy(x => x.Created)
				.ToList();

			var passCount = orderedResults.Count(x => string.Equals(x.Verdict, "PASS", StringComparison.OrdinalIgnoreCase));
			var pendingCount = orderedResults.Count(x => string.Equals(x.Verdict, "PENDING", StringComparison.OrdinalIgnoreCase));
			var failCount = orderedResults.Count(x => string.Equals(x.Verdict, "FAIL", StringComparison.OrdinalIgnoreCase));
			var unknownCount = orderedResults.Count - passCount - pendingCount - failCount;

			return new ScoringJobDetailResponse
			{
				JobId = job.Id,
				RequestId = job.RequestId,
				EnvironmentKey = job.EnvironmentKey,
				Status = job.Status.ToString().ToUpperInvariant(),
				RetryCount = job.RetryCount,
				FailureReason = job.FailureReason,
				CreatedAt = job.Created,
				CompletedAt = job.CompletedAt,
				Summary = new ScoringJobResultSummary
				{
					Processed = orderedResults.Count,
					Pass = passCount,
					Pending = pendingCount,
					Fail = failCount,
					Unknown = unknownCount,
				},
				Results = orderedResults
					.Select(x => new ScoringJobResultResponse
					{
						SourceType = x.SourceType,
						Source = x.Source,
						Verdict = x.Verdict,
						QualityScore = x.QualityScore,
						VisualizationBlobUrl = ExtractVisualizationBlobUrl(x.PayloadJson),
						PayloadJson = x.PayloadJson,
					})
					.ToList(),
			};
		}

		private static ScoringJobListItemResponse MapToListItem(ScoringJob job)
		{
			var detail = MapToDetail(job);
			return new ScoringJobListItemResponse
			{
				JobId = detail.JobId,
				RequestId = detail.RequestId,
				EnvironmentKey = detail.EnvironmentKey,
				Status = detail.Status,
				RetryCount = detail.RetryCount,
				FailureReason = detail.FailureReason,
				CreatedAt = detail.CreatedAt,
				CompletedAt = detail.CompletedAt,
				Summary = detail.Summary,
			};
		}

		private static ScoringRetrainBatchDetailResponse MapRetrainBatch(ScoringRetrainBatch batch)
		{
			return new ScoringRetrainBatchDetailResponse
			{
				BatchId = batch.Id,
				Status = batch.Status.ToString().ToUpperInvariant(),
				RequestedAtUtc = batch.RequestedAtUtc,
				SourceWindowFromUtc = batch.SourceWindowFromUtc,
				ReviewedSampleCount = batch.ReviewedSampleCount,
				AnnotatedSampleCount = batch.AnnotatedSampleCount,
				ApprovedAnnotationCount = batch.ApprovedAnnotationCount,
				CalibrationSampleCount = batch.CalibrationSampleCount,
				CompletedAtUtc = batch.CompletedAtUtc,
				FailureReason = batch.FailureReason,
				Promoted = batch.Promoted,
				MetricKey = batch.MetricKey,
				CandidateMetric = batch.CandidateMetric,
				BaselineMetric = batch.BaselineMetric,
				MinimumImprovement = batch.MinimumImprovement,
				PromotionReason = batch.PromotionReason,
				Runs = batch.Runs
					.OrderByDescending(x => x.StartedAtUtc)
					.Select(x => new ScoringRetrainRunResponse
					{
						RunId = x.Id,
						Status = x.Status.ToString().ToUpperInvariant(),
						Mode = x.Mode,
						StartedAtUtc = x.StartedAtUtc,
						CompletedAtUtc = x.CompletedAtUtc,
						ExitCode = x.ExitCode,
						Message = x.Message,
						Logs = x.Logs
					})
					.ToList(),
			};
		}

		private static ScoringRetrainBatchListItemResponse MapRetrainBatchListItem(ScoringRetrainBatch batch)
		{
			var latestRun = batch.Runs
				.OrderByDescending(x => x.StartedAtUtc)
				.FirstOrDefault();

			return new ScoringRetrainBatchListItemResponse
			{
				BatchId = batch.Id,
				Status = batch.Status.ToString().ToUpperInvariant(),
				RequestedAtUtc = batch.RequestedAtUtc,
				SourceWindowFromUtc = batch.SourceWindowFromUtc,
				ReviewedSampleCount = batch.ReviewedSampleCount,
				AnnotatedSampleCount = batch.AnnotatedSampleCount,
				ApprovedAnnotationCount = batch.ApprovedAnnotationCount,
				CalibrationSampleCount = batch.CalibrationSampleCount,
				CompletedAtUtc = batch.CompletedAtUtc,
				Promoted = batch.Promoted,
				FailureReason = batch.FailureReason,
				PromotionReason = batch.PromotionReason,
				MetricKey = batch.MetricKey,
				CandidateMetric = batch.CandidateMetric,
				BaselineMetric = batch.BaselineMetric,
				MinimumImprovement = batch.MinimumImprovement,
				RunCount = batch.Runs.Count,
				LatestRunStartedAtUtc = latestRun?.StartedAtUtc,
				Runs = batch.Runs
					.OrderByDescending(x => x.StartedAtUtc)
					.Select(x => new ScoringRetrainRunResponse
					{
						RunId = x.Id,
						Status = x.Status.ToString().ToUpperInvariant(),
						Mode = x.Mode,
						StartedAtUtc = x.StartedAtUtc,
						CompletedAtUtc = x.CompletedAtUtc,
						ExitCode = x.ExitCode,
						Message = x.Message,
						Logs = x.Logs
					})
					.ToList()
			};
		}

		private static ScoringAnnotationCandidateListItemResponse MapAnnotationCandidateListItem(ScoringAnnotationCandidate candidate)
		{
			return new ScoringAnnotationCandidateListItemResponse
			{
				CandidateId = candidate.Id,
				ResultId = candidate.ResultId,
				JobId = candidate.JobId,
				RequestId = candidate.RequestId,
				EnvironmentKey = candidate.EnvironmentKey,
				CandidateStatus = candidate.CandidateStatus.ToString().ToUpperInvariant(),
				ImageUrl = candidate.ImageUrl,
				VisualizationBlobUrl = candidate.VisualizationBlobUrl,
				OriginalVerdict = candidate.OriginalVerdict,
				ReviewedVerdict = candidate.ReviewedVerdict,
				SourceType = candidate.SourceType,
				AssignedToUserId = candidate.AssignedToUserId,
				CreatedAtUtc = candidate.CreatedAtUtc,
				SubmittedAtUtc = candidate.SubmittedAtUtc,
				ApprovedAtUtc = candidate.ApprovedAtUtc,
				HasAnnotation = candidate.Annotation is not null,
				AnnotationVersion = candidate.Annotation?.Version,
			};
		}

		private static ScoringAnnotationCandidateDetailResponse MapAnnotationCandidateDetail(ScoringAnnotationCandidate candidate)
		{
			var payloadJson = candidate.Result?.PayloadJson ?? "{}";
			return new ScoringAnnotationCandidateDetailResponse
			{
				CandidateId = candidate.Id,
				ResultId = candidate.ResultId,
				JobId = candidate.JobId,
				RequestId = candidate.RequestId,
				EnvironmentKey = candidate.EnvironmentKey,
				CandidateStatus = candidate.CandidateStatus.ToString().ToUpperInvariant(),
				ImageUrl = candidate.ImageUrl,
				VisualizationBlobUrl = !string.IsNullOrWhiteSpace(candidate.VisualizationBlobUrl)
					? candidate.VisualizationBlobUrl
					: ExtractVisualizationBlobUrl(payloadJson),
				OriginalVerdict = candidate.OriginalVerdict,
				ReviewedVerdict = candidate.ReviewedVerdict,
				SourceType = candidate.SourceType,
				AssignedToUserId = candidate.AssignedToUserId,
				CreatedAtUtc = candidate.CreatedAtUtc,
				SubmittedAtUtc = candidate.SubmittedAtUtc,
				ApprovedAtUtc = candidate.ApprovedAtUtc,
				PayloadJson = payloadJson,
				PreAnnotationJson = BuildPreAnnotationJson(payloadJson),
				SnapshotBlobKey = candidate.SnapshotBlobKey,
				MetadataBlobKey = candidate.MetadataBlobKey,
				Annotation = candidate.Annotation is null ? null : MapAnnotation(candidate.Annotation),
			};
		}

		private static ScoringAnnotationResponse MapAnnotation(ScoringAnnotation annotation)
		{
			return new ScoringAnnotationResponse
			{
				AnnotationId = annotation.Id,
				AnnotationFormat = MapAnnotationFormat(annotation.AnnotationFormat),
				LabelsJson = string.IsNullOrWhiteSpace(annotation.LabelsJson) ? "[]" : annotation.LabelsJson,
				ReviewerNote = annotation.ReviewerNote,
				Version = annotation.Version,
				CreatedByUserId = annotation.CreatedByUserId,
				ApprovedByUserId = annotation.ApprovedByUserId,
				LastModifiedUtc = annotation.LastModified,
			};
		}

		private static string BuildPreAnnotationJson(string payloadJson)
		{
			if (string.IsNullOrWhiteSpace(payloadJson))
			{
				return "{\"schemaVersion\":1,\"format\":\"bbox-region-v1\",\"labels\":[]}";
			}

			try
			{
				var root = JsonNode.Parse(payloadJson) as JsonObject;
				var yoloResults = root?["yolo"]?["results"] as JsonArray;
				var labels = new JsonArray();
				if (yoloResults is not null)
				{
					foreach (var item in yoloResults.OfType<JsonObject>())
					{
						var bbox = item["bbox"] as JsonArray;
						if (bbox is null || bbox.Count < 4)
						{
							continue;
						}

						var x1 = bbox[0]?.GetValue<double>() ?? 0;
						var y1 = bbox[1]?.GetValue<double>() ?? 0;
						var x2 = bbox[2]?.GetValue<double>() ?? 0;
						var y2 = bbox[3]?.GetValue<double>() ?? 0;
						var classLabel = item["class_label"]?.GetValue<string>();
						if (string.IsNullOrWhiteSpace(classLabel))
						{
							classLabel = MapYoloClassLabel(item["class_id"]?.GetValue<int>() ?? 0);
						}

						labels.Add(new JsonObject
						{
							["label"] = classLabel,
							["shapeType"] = "rectangle",
							["source"] = "ai-preannotation",
							["points"] = new JsonArray
							{
								new JsonArray(x1, y1),
								new JsonArray(x2, y2),
							}
						});
					}
				}

				return new JsonObject
				{
					["schemaVersion"] = 1,
					["format"] = "bbox-region-v1",
					["labels"] = labels,
				}.ToJsonString(PayloadSerializerOptions);
			}
			catch
			{
				return "{\"schemaVersion\":1,\"format\":\"bbox-region-v1\",\"labels\":[]}";
			}
		}

		private static string MapYoloClassLabel(int classId)
		{
			return classId == 1 ? "wet_surface" : "stain_or_water";
		}

		private static ScoringAnnotationFormat ParseAnnotationFormat(string? raw)
		{
			var normalized = string.IsNullOrWhiteSpace(raw) ? "bbox-region-v1" : raw.Trim();
			return normalized.ToLowerInvariant() switch
			{
				"bbox-region-v1" => ScoringAnnotationFormat.BboxRegionV1,
				_ => throw new ArgumentException($"Unsupported annotation format '{raw}'.", nameof(raw)),
			};
		}

		private static string MapAnnotationFormat(ScoringAnnotationFormat format)
		{
			return format switch
			{
				ScoringAnnotationFormat.BboxRegionV1 => "bbox-region-v1",
				_ => format.ToString(),
			};
		}

		private string GetActorIdentifier()
		{
			if (_userContext.UserId != Guid.Empty)
			{
				return _userContext.UserId.ToString();
			}

			return string.IsNullOrWhiteSpace(_userContext.Email) ? "system" : _userContext.Email;
		}

		private Guid? ResolveSubmittedByUserId(string? submittedByUserId)
		{
			if (!string.IsNullOrWhiteSpace(submittedByUserId))
			{
				if (Guid.TryParse(submittedByUserId.Trim(), out var parsed) && parsed != Guid.Empty)
				{
					return parsed;
				}

				throw new ArgumentException("SubmittedByUserId must be a valid user id.", nameof(submittedByUserId));
			}

			return _userContext.IsAuthenticated && _userContext.UserId != Guid.Empty
				? _userContext.UserId
				: null;
		}

		private bool IsAdmin()
		{
			return RoleEquals("Admin", "2");
		}

		private bool IsManager()
		{
			return RoleEquals("Manager", "3");
		}

		private bool RoleEquals(string roleName, string roleValue)
		{
			return string.Equals(_userContext.Role, roleName, StringComparison.OrdinalIgnoreCase) ||
				string.Equals(_userContext.Role, roleValue, StringComparison.OrdinalIgnoreCase);
		}
	}
}
