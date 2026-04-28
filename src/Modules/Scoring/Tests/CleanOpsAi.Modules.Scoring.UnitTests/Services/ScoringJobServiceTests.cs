using CleanOpsAi.BuildingBlocks.Application;
using CleanOpsAi.BuildingBlocks.Application.Exceptions;
using CleanOpsAi.BuildingBlocks.Application.Interfaces.Messaging;
using CleanOpsAi.Modules.Scoring.Application.Common.Interfaces.Repositories;
using CleanOpsAi.Modules.Scoring.Application.Common.Interfaces.Services;
using CleanOpsAi.Modules.Scoring.Application.DTOs.Request;
using CleanOpsAi.Modules.Scoring.Application.DTOs.Response;
using CleanOpsAi.Modules.Scoring.Application.Services;
using CleanOpsAi.Modules.Scoring.Domain.Entities;
using CleanOpsAi.Modules.Scoring.Domain.Enums;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System.Text.Json;

namespace CleanOpsAi.Modules.Scoring.UnitTests.Services
{
	public class ScoringJobServiceTests
	{
		private readonly IScoringJobRepository _repository;
		private readonly IScoringInferenceClient _inferenceClient;
		private readonly IEventBus _eventBus;
		private readonly IUserContext _userContext;
		private readonly ISupervisorManagedWorkerQueryService _managedWorkerQueryService;
		private readonly IWorkerLookupQueryService _workerLookupQueryService;
		private readonly IScoringAnnotationArtifactService _annotationArtifactService;
		private readonly IScoringRetrainRequestHandler _retrainRequestHandler;
		private readonly ILogger<ScoringJobService> _logger;
		private readonly ScoringJobService _service;

		public ScoringJobServiceTests()
		{
			_repository = Substitute.For<IScoringJobRepository>();
			_inferenceClient = Substitute.For<IScoringInferenceClient>();
			_eventBus = Substitute.For<IEventBus>();
			_userContext = Substitute.For<IUserContext>();
			_managedWorkerQueryService = Substitute.For<ISupervisorManagedWorkerQueryService>();
			_workerLookupQueryService = Substitute.For<IWorkerLookupQueryService>();
			_annotationArtifactService = Substitute.For<IScoringAnnotationArtifactService>();
			_retrainRequestHandler = Substitute.For<IScoringRetrainRequestHandler>();
			_logger = Substitute.For<ILogger<ScoringJobService>>();

			_service = new ScoringJobService(
				_repository,
				_inferenceClient,
				_eventBus,
				_userContext,
				_managedWorkerQueryService,
				_workerLookupQueryService,
				_annotationArtifactService,
				_retrainRequestHandler,
				_logger);
		}

		[Fact]
		public async Task GetPendingResultsAsync_ShouldFilterByManagedWorkers_WhenUserIsSupervisor()
		{
			var supervisorId = Guid.NewGuid();
			var managedWorkerUserId = Guid.NewGuid();
			var pendingResult = BuildPendingResult(managedWorkerUserId);

			_userContext.Role.Returns("Supervisor");
			_userContext.UserId.Returns(supervisorId);
			_managedWorkerQueryService
				.GetManagedWorkerUserIdsAsync(supervisorId, Arg.Any<CancellationToken>())
				.Returns(new[] { managedWorkerUserId });
			_repository
				.GetPendingResultsAsync(25, Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<CancellationToken>())
				.Returns(new[] { pendingResult });
			_workerLookupQueryService
				.GetWorkersByUserIdsAsync(Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<CancellationToken>())
				.Returns(new[]
				{
					new WorkerLookupItem
					{
						UserId = managedWorkerUserId,
						WorkerId = Guid.NewGuid(),
						FullName = "Worker A"
					}
				});

			var result = await _service.GetPendingResultsAsync(25);
			var item = result.First();

			Assert.Single(result);
			Assert.Equal(managedWorkerUserId, item.SubmittedByUserId);
			Assert.Equal("Worker A", item.WorkerName);
			Assert.NotNull(item.WorkerId);
			await _repository.Received(1).GetPendingResultsAsync(
				25,
				Arg.Is<IReadOnlyCollection<Guid>>(x => x.Count == 1 && x.Contains(managedWorkerUserId)),
				Arg.Any<CancellationToken>());
		}

		[Fact]
		public async Task GetPendingResultsAsync_ShouldReturnEmpty_WhenSupervisorManagesNoWorkers()
		{
			var supervisorId = Guid.NewGuid();

			_userContext.Role.Returns("Supervisor");
			_userContext.UserId.Returns(supervisorId);
			_managedWorkerQueryService
				.GetManagedWorkerUserIdsAsync(supervisorId, Arg.Any<CancellationToken>())
				.Returns(Array.Empty<Guid>());

			var result = await _service.GetPendingResultsAsync(10);

			Assert.Empty(result);
			await _repository.DidNotReceive().GetPendingResultsAsync(Arg.Any<int>(), Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<CancellationToken>());
		}

		[Fact]
		public async Task GetPendingResultsAsync_ShouldNotFilter_WhenUserIsAdmin()
		{
			var pendingResult = BuildPendingResult(Guid.NewGuid());

			_userContext.Role.Returns("Admin");
			_repository
				.GetPendingResultsAsync(50, null, Arg.Any<CancellationToken>())
				.Returns(new[] { pendingResult });
			_workerLookupQueryService
				.GetWorkersByUserIdsAsync(Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<CancellationToken>())
				.Returns(Array.Empty<WorkerLookupItem>());

			var result = await _service.GetPendingResultsAsync(50);
			var item = result.First();

			Assert.Single(result);
			Assert.Null(item.WorkerId);
			Assert.Null(item.WorkerName);
			await _repository.Received(1).GetPendingResultsAsync(50, null, Arg.Any<CancellationToken>());
			await _managedWorkerQueryService.DidNotReceive().GetManagedWorkerUserIdsAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
		}

		[Fact]
		public async Task GetPendingResultsAsync_ShouldExposeWorkerMetadata_WhenLookupExists()
		{
			var submittedByUserId = Guid.NewGuid();
			var workerId = Guid.NewGuid();
			var pendingResult = BuildPendingResult(submittedByUserId);

			_userContext.Role.Returns("Admin");
			_repository
				.GetPendingResultsAsync(5, null, Arg.Any<CancellationToken>())
				.Returns(new[] { pendingResult });
			_workerLookupQueryService
				.GetWorkersByUserIdsAsync(
					Arg.Is<IReadOnlyCollection<Guid>>(x => x.Count == 1 && x.Contains(submittedByUserId)),
					Arg.Any<CancellationToken>())
				.Returns(new[]
				{
					new WorkerLookupItem
					{
						UserId = submittedByUserId,
						WorkerId = workerId,
						FullName = "Assigned Worker"
					}
				});

			var result = await _service.GetPendingResultsAsync(5);
			var item = result.First();

			Assert.Single(result);
			Assert.Equal(submittedByUserId, item.SubmittedByUserId);
			Assert.Equal(workerId, item.WorkerId);
			Assert.Equal("Assigned Worker", item.WorkerName);
		}

		[Fact]
		public async Task ReviewPendingResultAsync_ShouldAllowManagedWorker_WhenUserIsSupervisor()
		{
			var supervisorId = Guid.NewGuid();
			var managedWorkerUserId = Guid.NewGuid();
			var resultId = Guid.NewGuid();
			var pendingResult = BuildPendingResult(managedWorkerUserId, resultId);

			_userContext.Role.Returns("Supervisor");
			_userContext.UserId.Returns(supervisorId);
			_userContext.IsAuthenticated.Returns(true);
			_userContext.Email.Returns("supervisor@example.com");
			_managedWorkerQueryService
				.GetManagedWorkerUserIdsAsync(supervisorId, Arg.Any<CancellationToken>())
				.Returns(new[] { managedWorkerUserId });
			_repository.GetResultByIdWithJobAsync(resultId, Arg.Any<CancellationToken>()).Returns(pendingResult);

			var response = await _service.ReviewPendingResultAsync(resultId, new ReviewScoringResultRequest
			{
				Verdict = "PASS",
				Reason = "Looks good"
			});

			Assert.NotNull(response);
			Assert.Equal("PASS", pendingResult.Verdict);
			await _repository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
		}

		[Fact]
		public async Task ReviewPendingResultAsync_ShouldThrowForbidden_WhenWorkerIsNotManaged()
		{
			var supervisorId = Guid.NewGuid();
			var managedWorkerUserId = Guid.NewGuid();
			var unmanagedWorkerUserId = Guid.NewGuid();
			var resultId = Guid.NewGuid();
			var pendingResult = BuildPendingResult(unmanagedWorkerUserId, resultId);

			_userContext.Role.Returns("Supervisor");
			_userContext.UserId.Returns(supervisorId);
			_managedWorkerQueryService
				.GetManagedWorkerUserIdsAsync(supervisorId, Arg.Any<CancellationToken>())
				.Returns(new[] { managedWorkerUserId });
			_repository.GetResultByIdWithJobAsync(resultId, Arg.Any<CancellationToken>()).Returns(pendingResult);

			await Assert.ThrowsAsync<ForbiddenException>(() => _service.ReviewPendingResultAsync(
				resultId,
				new ReviewScoringResultRequest { Verdict = "FAIL" }));

			await _repository.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
		}

		[Fact]
		public async Task ReviewPendingResultAsync_ShouldThrowForbidden_WhenSubmittedByUserIdIsMissing()
		{
			var supervisorId = Guid.NewGuid();
			var resultId = Guid.NewGuid();
			var pendingResult = BuildPendingResult(null, resultId);

			_userContext.Role.Returns("Supervisor");
			_userContext.UserId.Returns(supervisorId);
			_managedWorkerQueryService
				.GetManagedWorkerUserIdsAsync(supervisorId, Arg.Any<CancellationToken>())
				.Returns(new[] { Guid.NewGuid() });
			_repository.GetResultByIdWithJobAsync(resultId, Arg.Any<CancellationToken>()).Returns(pendingResult);

			await Assert.ThrowsAsync<ForbiddenException>(() => _service.ReviewPendingResultAsync(
				resultId,
				new ReviewScoringResultRequest { Verdict = "PASS" }));
		}

		[Fact]
		public async Task ProcessQueuedJobAsync_ShouldPersistVisualizationResponseAsSingleSourceOfTruth()
		{
			var jobId = Guid.NewGuid();
			var now = DateTime.UtcNow;
			var job = new ScoringJob
			{
				Id = jobId,
				RequestId = "req-visual-1",
				EnvironmentKey = "LOBBY_CORRIDOR",
				Status = ScoringJobStatus.Queued,
				RetryCount = 0,
				Created = now,
				LastModified = now,
				Results = new List<ScoringJobResult>()
			};
			IReadOnlyCollection<ScoringJobResult>? replacedResults = null;

			_repository.GetByIdWithResultsAsync(jobId, Arg.Any<CancellationToken>()).Returns(job);
			_repository
				.When(x => x.ReplaceResultsAsync(jobId, Arg.Any<IReadOnlyCollection<ScoringJobResult>>(), Arg.Any<CancellationToken>()))
				.Do(callInfo => replacedResults = callInfo.ArgAt<IReadOnlyCollection<ScoringJobResult>>(1));

			_inferenceClient.EvaluateUrlVisualizeLinkAsync(
				"LOBBY_CORRIDOR",
				"https://example.com/a.jpg",
				Arg.Any<CancellationToken>())
				.Returns(BuildVisualizationResponse(
					"https://example.com/a.jpg",
					qualityScore: 82.615,
					verdict: "PENDING",
					visualizationUrl: "https://blob.example.com/a.jpg",
					yoloDetectionsCount: 0));

			await _service.ProcessQueuedJobAsync(
				jobId,
				"LOBBY_CORRIDOR",
				new[] { "https://example.com/a.jpg" },
				CancellationToken.None);

			await _inferenceClient.DidNotReceive().EvaluateBatchAsync(Arg.Any<string>(), Arg.Any<IReadOnlyCollection<string>>(), Arg.Any<CancellationToken>());
			await _inferenceClient.Received(1).EvaluateUrlVisualizeLinkAsync("LOBBY_CORRIDOR", "https://example.com/a.jpg", Arg.Any<CancellationToken>());
			Assert.NotNull(replacedResults);
			var saved = Assert.Single(replacedResults!);
			Assert.Equal("PENDING", saved.Verdict);
			Assert.Equal(82.615, saved.QualityScore);
			Assert.Contains("\"quality_score\":82.615", saved.PayloadJson);
			Assert.Contains("\"visualization_blob_url\":\"https://blob.example.com/a.jpg\"", saved.PayloadJson);
			Assert.Contains("\"detections_count\":0", saved.PayloadJson);
			Assert.Contains("\"backend_runtime\":", saved.PayloadJson);
			Assert.Contains("\"source_of_truth\":\"visualize-link-only\"", saved.PayloadJson);
			Assert.Contains("\"code_path_version\":\"visualize_single_source_v1\"", saved.PayloadJson);
			Assert.Equal(ScoringJobStatus.Succeeded, job.Status);
		}

		[Fact]
		public async Task ProcessQueuedJobAsync_ShouldPersistFailurePlaceholder_WhenVisualizationCallFails()
		{
			var jobId = Guid.NewGuid();
			var now = DateTime.UtcNow;
			var job = new ScoringJob
			{
				Id = jobId,
				RequestId = "req-visual-2",
				EnvironmentKey = "LOBBY_CORRIDOR",
				Status = ScoringJobStatus.Queued,
				RetryCount = 0,
				Created = now,
				LastModified = now,
				Results = new List<ScoringJobResult>()
			};
			IReadOnlyCollection<ScoringJobResult>? replacedResults = null;

			_repository.GetByIdWithResultsAsync(jobId, Arg.Any<CancellationToken>()).Returns(job);
			_repository
				.When(x => x.ReplaceResultsAsync(jobId, Arg.Any<IReadOnlyCollection<ScoringJobResult>>(), Arg.Any<CancellationToken>()))
				.Do(callInfo => replacedResults = callInfo.ArgAt<IReadOnlyCollection<ScoringJobResult>>(1));

			_inferenceClient.EvaluateUrlVisualizeLinkAsync(
				"LOBBY_CORRIDOR",
				"https://example.com/b.jpg",
				Arg.Any<CancellationToken>())
				.Returns<Task<ScoringVisualizationLinkResponse>>(_ => throw new InvalidOperationException("boom"));

			await _service.ProcessQueuedJobAsync(
				jobId,
				"LOBBY_CORRIDOR",
				new[] { "https://example.com/b.jpg" },
				CancellationToken.None);

			Assert.NotNull(replacedResults);
			var saved = Assert.Single(replacedResults!);
			Assert.Equal("FAIL", saved.Verdict);
			Assert.Equal(0, saved.QualityScore);
			Assert.Contains("\"message\":\"boom\"", saved.PayloadJson);
			Assert.Contains("\"backend_runtime\":", saved.PayloadJson);
			Assert.Contains("\"source_of_truth\":\"visualize-link-only\"", saved.PayloadJson);
			Assert.DoesNotContain("visualization_blob_url", saved.PayloadJson);
			Assert.Equal(ScoringJobStatus.Succeeded, job.Status);
		}

		[Fact]
		public async Task UpsertAnnotationCandidateAsync_ShouldCreateSubmittedAnnotation()
		{
			var candidateId = Guid.NewGuid();
			var now = DateTime.UtcNow;
			var candidate = new ScoringAnnotationCandidate
			{
				Id = candidateId,
				ResultId = Guid.NewGuid(),
				JobId = Guid.NewGuid(),
				RequestId = "req-ann-1",
				EnvironmentKey = "LOBBY_CORRIDOR",
				ImageUrl = "https://example.com/after.jpg",
				OriginalVerdict = "PENDING",
				ReviewedVerdict = "FAIL",
				SourceType = "reviewed-fail-from-pending",
				CandidateStatus = ScoringAnnotationCandidateStatus.Queued,
				CreatedAtUtc = now,
				Created = now,
				LastModified = now,
				Result = new ScoringJobResult
				{
					Id = Guid.NewGuid(),
					ScoringJobId = Guid.NewGuid(),
					SourceType = "url",
					Source = "https://example.com/after.jpg",
					Verdict = "FAIL",
					QualityScore = 52,
					PayloadJson = "{\"yolo\":{\"results\":[]}}",
					Created = now,
					LastModified = now,
					ScoringJob = new ScoringJob
					{
						Id = Guid.NewGuid(),
						RequestId = "req-ann-1",
						EnvironmentKey = "LOBBY_CORRIDOR",
						Status = ScoringJobStatus.Succeeded,
						Created = now,
						LastModified = now,
					}
				}
			};

			_userContext.Role.Returns("Manager");
			_userContext.UserId.Returns(Guid.NewGuid());
			_repository.GetAnnotationCandidateByIdAsync(candidateId, Arg.Any<CancellationToken>()).Returns(candidate);

			var result = await _service.UpsertAnnotationCandidateAsync(candidateId, new UpsertScoringAnnotationRequest
			{
				Labels = JsonDocument.Parse("[{\"label\":\"stain_or_water\",\"shapeType\":\"rectangle\",\"points\":[[10,10],[100,100]]}]").RootElement.Clone(),
				ReviewerNote = "Needs cleanup",
				Submit = true,
			});

			Assert.NotNull(result);
			Assert.Equal(ScoringAnnotationCandidateStatus.Submitted, candidate.CandidateStatus);
			Assert.NotNull(candidate.Annotation);
			Assert.Equal(1, candidate.Annotation!.Version);
			Assert.Contains("stain_or_water", candidate.Annotation.LabelsJson);
			await _repository.Received(1).InsertAnnotationAsync(Arg.Any<ScoringAnnotation>(), Arg.Any<CancellationToken>());
			await _repository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
		}

		[Fact]
		public async Task ApproveAnnotationCandidateAsync_ShouldPublishApprovedArtifact()
		{
			var candidateId = Guid.NewGuid();
			var reviewerId = Guid.NewGuid();
			var now = DateTime.UtcNow;
			var annotation = new ScoringAnnotation
			{
				Id = Guid.NewGuid(),
				CandidateId = candidateId,
				LabelsJson = "[]",
				Version = 1,
				Created = now,
				LastModified = now,
			};
			var candidate = new ScoringAnnotationCandidate
			{
				Id = candidateId,
				ResultId = Guid.NewGuid(),
				JobId = Guid.NewGuid(),
				RequestId = "req-ann-2",
				EnvironmentKey = "LOBBY_CORRIDOR",
				ImageUrl = "https://example.com/after.jpg",
				OriginalVerdict = "PENDING",
				ReviewedVerdict = "FAIL",
				SourceType = "reviewed-fail-from-pending",
				CandidateStatus = ScoringAnnotationCandidateStatus.Submitted,
				CreatedAtUtc = now,
				SnapshotBlobKey = "snapshots/file.jpg",
				MetadataBlobKey = "metadata/file.json",
				Created = now,
				LastModified = now,
				Annotation = annotation,
				Result = BuildPendingResult(Guid.NewGuid()),
			};

			_userContext.Role.Returns("Admin");
			_userContext.UserId.Returns(reviewerId);
			_repository.GetAnnotationCandidateByIdAsync(candidateId, Arg.Any<CancellationToken>()).Returns(candidate);

			var result = await _service.ApproveAnnotationCandidateAsync(candidateId, new ApproveScoringAnnotationCandidateRequest
			{
				Note = "Approved for retrain"
			});

			Assert.NotNull(result);
			Assert.Equal(ScoringAnnotationCandidateStatus.Approved, candidate.CandidateStatus);
			Assert.Equal(reviewerId, candidate.Annotation!.ApprovedByUserId);
			await _annotationArtifactService.Received(1).PublishApprovedAnnotationAsync(candidate, annotation, Arg.Any<CancellationToken>());
		}

		[Fact]
		public async Task TriggerRetrainAsync_ShouldPopulateAnnotationCounts()
		{
			var now = DateTime.UtcNow;
			var reviewed = new[] { BuildReviewedResult("PASS"), BuildReviewedResult("FAIL") };
			var annotatedCandidates = new[]
			{
				BuildAnnotatedCandidate(ScoringAnnotationCandidateStatus.Submitted),
				BuildAnnotatedCandidate(ScoringAnnotationCandidateStatus.Approved),
			};
			var approvedCandidates = new[]
			{
				BuildAnnotatedCandidate(ScoringAnnotationCandidateStatus.Approved),
			};

			_repository.GetReviewedResultsForRetrainAsync(Arg.Any<DateTime>(), 500, Arg.Any<CancellationToken>()).Returns(reviewed);
			_repository.GetAnnotatedCandidatesForRetrainAsync(Arg.Any<DateTime>(), 500, Arg.Any<CancellationToken>()).Returns(annotatedCandidates);
			_repository.GetApprovedAnnotationCandidatesForRetrainAsync(Arg.Any<DateTime>(), 500, Arg.Any<CancellationToken>()).Returns(approvedCandidates);

			var response = await _service.TriggerRetrainAsync(new TriggerScoringRetrainRequest
			{
				LookbackDays = 7,
				MinReviewedSamples = 1,
				MaxSamplesPerBatch = 500,
			});

			Assert.Equal(2, response.ReviewedSampleCount);
			Assert.Equal(2, response.AnnotatedSampleCount);
			Assert.Equal(1, response.ApprovedAnnotationCount);
			Assert.Equal(2, response.CalibrationSampleCount);
			await _repository.Received(1).InsertRetrainBatchAsync(
				Arg.Is<ScoringRetrainBatch>(x =>
					x.ReviewedSampleCount == 2 &&
					x.AnnotatedSampleCount == 2 &&
					x.ApprovedAnnotationCount == 1 &&
					x.CalibrationSampleCount == 2),
				Arg.Any<CancellationToken>());
		}

		private static ScoringJobResult BuildPendingResult(Guid? submittedByUserId, Guid? resultId = null)
		{
			var now = DateTime.UtcNow;

			return new ScoringJobResult
			{
				Id = resultId ?? Guid.NewGuid(),
				ScoringJobId = Guid.NewGuid(),
				SourceType = "url",
				Source = "https://example.com/image.jpg",
				Verdict = "PENDING",
				QualityScore = 0.82,
				PayloadJson = "{}",
				Created = now,
				LastModified = now,
				ScoringJob = new ScoringJob
				{
					Id = Guid.NewGuid(),
					RequestId = "req-1",
					EnvironmentKey = "LOBBY_CORRIDOR",
					Status = ScoringJobStatus.Succeeded,
					SubmittedByUserId = submittedByUserId,
					Created = now,
					LastModified = now
				}
			};
		}

		private static ScoringVisualizationLinkResponse BuildVisualizationResponse(
			string source,
			double qualityScore,
			string verdict,
			string visualizationUrl,
			int yoloDetectionsCount)
		{
			return new ScoringVisualizationLinkResponse
			{
				SourceType = "url",
				Source = source,
				EnvironmentKey = "LOBBY_CORRIDOR",
				Visualization = new ScoringVisualizationMetadata
				{
					Url = visualizationUrl,
					MimeType = "image/jpeg",
					ByteSize = 12345
				},
				Scoring = new ScoringInferenceScore
				{
					Verdict = verdict,
					QualityScore = qualityScore,
					BaseCleanScore = qualityScore,
					ObjectPenalty = 0,
					PassThreshold = 90,
					Reasons = new List<string> { "test reason" }
				},
				AdditionalData = new Dictionary<string, System.Text.Json.JsonElement>
				{
					["yolo"] = JsonSerializer.SerializeToElement(new
					{
						results = Array.Empty<object>(),
						detections_count = yoloDetectionsCount
					}),
					["unet"] = JsonSerializer.SerializeToElement(new
					{
						total_dirty_coverage_pct = 17.385
					}),
					["llm_filter"] = JsonSerializer.SerializeToElement(new
					{
						route_mode = "visualize_enhanced"
					})
				}
			};
		}

		private static ScoringJobResult BuildReviewedResult(string reviewedVerdict)
		{
			var now = DateTime.UtcNow;
			return new ScoringJobResult
			{
				Id = Guid.NewGuid(),
				ScoringJobId = Guid.NewGuid(),
				SourceType = "url",
				Source = "https://example.com/reviewed.jpg",
				Verdict = reviewedVerdict,
				QualityScore = 77,
				PayloadJson = "{\"human_review\":{\"original_verdict\":\"PENDING\",\"reviewed_at_utc\":\"2026-04-24T00:00:00Z\"}}",
				Created = now,
				LastModified = now,
				ScoringJob = new ScoringJob
				{
					Id = Guid.NewGuid(),
					RequestId = "req-review",
					EnvironmentKey = "LOBBY_CORRIDOR",
					Status = ScoringJobStatus.Succeeded,
					Created = now,
					LastModified = now,
				}
			};
		}

		private static ScoringAnnotationCandidate BuildAnnotatedCandidate(ScoringAnnotationCandidateStatus status)
		{
			var now = DateTime.UtcNow;
			return new ScoringAnnotationCandidate
			{
				Id = Guid.NewGuid(),
				ResultId = Guid.NewGuid(),
				JobId = Guid.NewGuid(),
				RequestId = "req-candidate",
				EnvironmentKey = "LOBBY_CORRIDOR",
				ImageUrl = "https://example.com/after.jpg",
				OriginalVerdict = "PENDING",
				ReviewedVerdict = "FAIL",
				SourceType = "reviewed-fail-from-pending",
				CandidateStatus = status,
				CreatedAtUtc = now,
				Created = now,
				LastModified = now,
				Annotation = new ScoringAnnotation
				{
					Id = Guid.NewGuid(),
					CandidateId = Guid.NewGuid(),
					LabelsJson = "[]",
					Version = 1,
					Created = now,
					LastModified = now,
				}
			};
		}
	}
}
