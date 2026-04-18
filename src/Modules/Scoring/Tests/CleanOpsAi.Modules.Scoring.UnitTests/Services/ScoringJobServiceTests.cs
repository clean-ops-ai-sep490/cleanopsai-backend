using CleanOpsAi.BuildingBlocks.Application;
using CleanOpsAi.BuildingBlocks.Application.Exceptions;
using CleanOpsAi.BuildingBlocks.Application.Interfaces.Messaging;
using CleanOpsAi.Modules.Scoring.Application.Common.Interfaces.Repositories;
using CleanOpsAi.Modules.Scoring.Application.Common.Interfaces.Services;
using CleanOpsAi.Modules.Scoring.Application.DTOs.Request;
using CleanOpsAi.Modules.Scoring.Application.Services;
using CleanOpsAi.Modules.Scoring.Domain.Entities;
using CleanOpsAi.Modules.Scoring.Domain.Enums;
using Microsoft.Extensions.Logging;
using NSubstitute;

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
			_logger = Substitute.For<ILogger<ScoringJobService>>();

			_service = new ScoringJobService(
				_repository,
				_inferenceClient,
				_eventBus,
				_userContext,
				_managedWorkerQueryService,
				_workerLookupQueryService,
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
	}
}
