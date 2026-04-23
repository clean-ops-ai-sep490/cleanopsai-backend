using AutoMapper;
using CleanOpsAi.BuildingBlocks.Application;
using CleanOpsAi.BuildingBlocks.Application.Common;
using CleanOpsAi.BuildingBlocks.Application.Exceptions;
using CleanOpsAi.BuildingBlocks.Application.Interfaces;
using CleanOpsAi.BuildingBlocks.Application.Pagination;
using CleanOpsAi.BuildingBlocks.Infrastructure.Events.Request;
using CleanOpsAi.Modules.TaskOperations.Application.Common.Interfaces.Repositories;
using CleanOpsAi.Modules.TaskOperations.Application.Common.Interfaces.Services;
using CleanOpsAi.Modules.TaskOperations.Application.DTOs.Request;
using CleanOpsAi.Modules.TaskOperations.Application.DTOs.Response;
using CleanOpsAi.Modules.TaskOperations.Application.Services;
using CleanOpsAi.Modules.TaskOperations.Domain.Entities;
using CleanOpsAi.Modules.TaskOperations.Domain.Enums;
using NSubstitute;

namespace CleanOpsAi.Modules.TaskOperations.UnitTests.Services
{
	public class TaskSwapRequestServiceTests
	{
		private readonly ITaskSwapRequestRepository _swapRequestRepo;
		private readonly ITaskAssignmentRepository _taskAssignmentRepo;
		private readonly IMapper _mapper;
		private readonly IDateTimeProvider _dateTimeProvider;
		private readonly IIdGenerator _idGenerator;
		private readonly IUserContext _userContext;
		private readonly IWorkerQueryService _workerQueryService;
		private readonly ISopRequirementsQueryService _sopRequirementsQueryService;
		private readonly IWorkerCertificationSkillQueryService _workerCertificationSkillQueryService;
		private readonly ISupervisorQueryService _supervisorQueryService;
		private readonly INotificationPublisher _notificationPublisher;
		private readonly TaskSwapRequestService _service;

		// ---------------------------------------------------------------
		// Shared helpers
		// ---------------------------------------------------------------

		/// <summary>
		/// Returns a SopRequirementsIntegrated that has no requirements,
		/// so ValidateWorkerCompetencyAsync passes without extra stubs.
		/// </summary>
		private static SopRequirementsIntegrated NoRequirements() => new()
		{
			Found = false,
			RequiredSkillIds = new List<Guid>(),
			RequiredCertificationIds = new List<Guid>()
		};

		/// <summary>Stub both SaveChangesAsync overloads to return 1.</summary>
		private void StubSaveChanges()
		{
			_swapRequestRepo.SaveChangesAsync().Returns(1);
			_swapRequestRepo.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);
		}

		// ---------------------------------------------------------------
		// Constructor
		// ---------------------------------------------------------------

		public TaskSwapRequestServiceTests()
		{
			_swapRequestRepo = Substitute.For<ITaskSwapRequestRepository>();
			_taskAssignmentRepo = Substitute.For<ITaskAssignmentRepository>();
			_mapper = Substitute.For<IMapper>();
			_dateTimeProvider = Substitute.For<IDateTimeProvider>();
			_idGenerator = Substitute.For<IIdGenerator>();
			_userContext = Substitute.For<IUserContext>();
			_workerQueryService = Substitute.For<IWorkerQueryService>();
			_sopRequirementsQueryService = Substitute.For<ISopRequirementsQueryService>();
			_workerCertificationSkillQueryService = Substitute.For<IWorkerCertificationSkillQueryService>();
			_supervisorQueryService = Substitute.For<ISupervisorQueryService>();
			_notificationPublisher = Substitute.For<INotificationPublisher>();

			_service = new TaskSwapRequestService(
				_swapRequestRepo,
				_taskAssignmentRepo,
				_mapper,
				_dateTimeProvider,
				_idGenerator,
				_userContext,
				_workerQueryService,
				_sopRequirementsQueryService,
				_workerCertificationSkillQueryService,
				_supervisorQueryService,
				_notificationPublisher);
		}

		// ===============================================================
		// GET BY ID
		// ===============================================================

		[Fact]
		public async Task GetById_ShouldReturnDto_WhenSwapRequestExists()
		{
			var swapRequestId = Guid.NewGuid();
			var requesterId = Guid.NewGuid();
			var targetWorkerId = Guid.NewGuid();

			var entity = new TaskSwapRequest
			{
				Id = swapRequestId,
				RequesterId = requesterId,
				RequesterName = "Worker A",
				TargetWorkerId = targetWorkerId,
				TargetWorkerName = "Worker B",
				Status = SwapRequestStatus.PendingTargetApproval
			};

			_swapRequestRepo
				.GetByIdWithDetailsAsync(swapRequestId, Arg.Any<CancellationToken>())
				.Returns(entity);

			var expectedDto = new SwapRequestDto
			{
				Id = swapRequestId,
				RequesterName = "Worker A",
				TargetWorkerName = "Worker B"
			};

			_mapper.Map<SwapRequestDto>(entity).Returns(expectedDto);

			var result = await _service.GetById(swapRequestId);

			Assert.NotNull(result);
			Assert.Equal(swapRequestId, result.Id);
			Assert.Equal("Worker A", result.RequesterName);
		}

		[Fact]
		public async Task GetById_ShouldThrow_WhenSwapRequestNotFound()
		{
			var swapRequestId = Guid.NewGuid();

			_swapRequestRepo
				.GetByIdWithDetailsAsync(swapRequestId, Arg.Any<CancellationToken>())
				.Returns((TaskSwapRequest?)null);

			await Assert.ThrowsAsync<NotFoundException>(() => _service.GetById(swapRequestId));
		}

		// ===============================================================
		// CANCEL
		// ===============================================================

		[Fact]
		public async Task CancelSwapRequestAsync_ShouldCancelRequest_WhenExists()
		{
			var swapRequestId = Guid.NewGuid();
			var requesterId = Guid.NewGuid();

			var entity = new TaskSwapRequest
			{
				Id = swapRequestId,
				Status = SwapRequestStatus.PendingTargetApproval
			};

			_swapRequestRepo
				.GetByIdWithDetailsAsync(swapRequestId, Arg.Any<CancellationToken>())
				.Returns(entity);

			StubSaveChanges();

			var result = await _service.CancelSwapRequestAsync(swapRequestId, requesterId);

			Assert.True(result.Succeeded);
			Assert.Equal(SwapRequestStatus.CancelledByRequester, entity.Status);
			await _swapRequestRepo.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
		}

		[Fact]
		public async Task CancelSwapRequestAsync_ShouldThrow_WhenNotFound()
		{
			var swapRequestId = Guid.NewGuid();

			_swapRequestRepo
				.GetByIdWithDetailsAsync(swapRequestId, Arg.Any<CancellationToken>())
				.Returns((TaskSwapRequest?)null);

			await Assert.ThrowsAsync<NotFoundException>(() =>
				_service.CancelSwapRequestAsync(swapRequestId, Guid.NewGuid()));
		}

		// ===============================================================
		// CREATE SWAP REQUEST
		// ===============================================================

		/// <summary>
		/// Builds a valid pair of TaskAssignment stubs that pass all
		/// ValidateRequesterTask / ValidateTargetTask / ValidateSwapRules checks:
		///   - requester task starts in 3 h (>= 2 h threshold)
		///   - target  task starts in 13 h (>= 12 h threshold)
		///   - both tasks NotStarted and in the same WorkArea
		///   - both TaskScheduleIds are set (needed for competency check)
		///   - within same week
		/// </summary>
		private (TaskAssignment requester, TaskAssignment target) BuildValidTaskPair(
			Guid requesterId, Guid targetWorkerId,
			Guid taskAssignmentId, Guid targetTaskAssignmentId,
			Guid workAreaId, DateTime now)
		{
			var requester = new TaskAssignment
			{
				Id = taskAssignmentId,
				AssigneeId = requesterId,
				Status = TaskAssignmentStatus.NotStarted,
				ScheduledStartAt = now.AddHours(3),
				ScheduledEndAt = now.AddHours(4),
				WorkAreaId = workAreaId,
				TaskScheduleId = Guid.NewGuid()
			};

			var target = new TaskAssignment
			{
				Id = targetTaskAssignmentId,
				AssigneeId = targetWorkerId,
				Status = TaskAssignmentStatus.NotStarted,
				ScheduledStartAt = now.AddHours(13),
				ScheduledEndAt = now.AddHours(14),
				WorkAreaId = workAreaId,
				TaskScheduleId = Guid.NewGuid()
			};

			return (requester, target);
		}

		/// <summary>Stubs all side-services so competency / conflict checks pass.</summary>
		private void StubCreateHappyPath(
			Guid requesterId, Guid targetWorkerId,
			Guid taskAssignmentId, Guid targetTaskAssignmentId,
			TaskAssignment requesterTask, TaskAssignment targetTask)
		{
			_taskAssignmentRepo
				.GetByIdAsync(taskAssignmentId, Arg.Any<CancellationToken>())
				.Returns(requesterTask);

			_taskAssignmentRepo
				.GetByIdAsync(targetTaskAssignmentId, Arg.Any<CancellationToken>())
				.Returns(targetTask);

			// No pending swap
			_swapRequestRepo.HasPendingSwapAsync(taskAssignmentId).Returns(false);

			// No time conflicts
			_taskAssignmentRepo
				.HasTimeConflictAsync(
					Arg.Any<Guid>(), Arg.Any<Guid>(),
					Arg.Any<DateTime>(), Arg.Any<DateTime>(),
					Arg.Any<CancellationToken>())
				.Returns(false);

			// SOP — no requirements → skip competency check
			_sopRequirementsQueryService
				.GetSopRequirementsByScheduleId(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
				.Returns(Task.FromResult(NoRequirements()));

			// Worker names
			_workerQueryService
				.GetUserNames(Arg.Any<List<Guid>>())
				.Returns(new Dictionary<Guid, string>
				{
					{ requesterId, "Worker A" },
					{ targetWorkerId, "Worker B" }
				});

			// Mapper
			_mapper
				.Map<SwapRequestDto>(Arg.Any<TaskSwapRequest>())
				.Returns(new SwapRequestDto { Id = Guid.NewGuid(), RequesterName = "Worker A", TargetWorkerName = "Worker B" });

			_idGenerator.Generate().Returns(Guid.NewGuid());
			_userContext.UserId.Returns(requesterId);

			StubSaveChanges();
		}

		[Fact]
		public async Task CreateSwapRequestAsync_ShouldCreateRequest_WhenValidationsPassed()
		{
			var requesterId = Guid.NewGuid();
			var targetWorkerId = Guid.NewGuid();
			var taskAssignmentId = Guid.NewGuid();
			var targetTaskAssignmentId = Guid.NewGuid();
			var workAreaId = Guid.NewGuid();
			var now = DateTime.UtcNow;

			_dateTimeProvider.UtcNow.Returns(now);

			var (requesterTask, targetTask) = BuildValidTaskPair(
				requesterId, targetWorkerId,
				taskAssignmentId, targetTaskAssignmentId,
				workAreaId, now);

			StubCreateHappyPath(
				requesterId, targetWorkerId,
				taskAssignmentId, targetTaskAssignmentId,
				requesterTask, targetTask);

			var dto = new TaskSwapRequestCreateDto
			{
				TaskAssignmentId = taskAssignmentId,
				TargetTaskAssignmentId = targetTaskAssignmentId,
				RequesterId = requesterId,
				TargetWorkerId = targetWorkerId,
				RequesterNote = "Please help swap"
			};

			var result = await _service.CreateSwapRequestAsync(dto);

			Assert.True(result.Succeeded);
			Assert.NotNull(result.Value);
			await _swapRequestRepo.Received(1).InsertAsync(Arg.Any<TaskSwapRequest>(), Arg.Any<CancellationToken>());
			await _swapRequestRepo.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
		}

		[Fact]
		public async Task CreateSwapRequestAsync_ShouldThrow_WhenRequesterTaskNotFound()
		{
			var taskAssignmentId = Guid.NewGuid();

			_taskAssignmentRepo
				.GetByIdAsync(taskAssignmentId, Arg.Any<CancellationToken>())
				.Returns((TaskAssignment?)null);

			var dto = new TaskSwapRequestCreateDto
			{
				TaskAssignmentId = taskAssignmentId,
				RequesterId = Guid.NewGuid()
			};

			await Assert.ThrowsAsync<NotFoundException>(() => _service.CreateSwapRequestAsync(dto));
		}

		[Fact]
		public async Task CreateSwapRequestAsync_ShouldThrow_WhenTargetTaskNotFound()
		{
			var taskAssignmentId = Guid.NewGuid();
			var targetTaskAssignmentId = Guid.NewGuid();
			var requesterId = Guid.NewGuid();
			var now = DateTime.UtcNow;

			_dateTimeProvider.UtcNow.Returns(now);

			var requesterTask = new TaskAssignment
			{
				Id = taskAssignmentId,
				AssigneeId = requesterId,
				Status = TaskAssignmentStatus.NotStarted,
				ScheduledStartAt = now.AddHours(3),
				ScheduledEndAt = now.AddHours(4),
				WorkAreaId = Guid.NewGuid(),
				TaskScheduleId = Guid.NewGuid()
			};

			_taskAssignmentRepo
				.GetByIdAsync(taskAssignmentId, Arg.Any<CancellationToken>())
				.Returns(requesterTask);

			_taskAssignmentRepo
				.GetByIdAsync(targetTaskAssignmentId, Arg.Any<CancellationToken>())
				.Returns((TaskAssignment?)null);

			var dto = new TaskSwapRequestCreateDto
			{
				TaskAssignmentId = taskAssignmentId,
				TargetTaskAssignmentId = targetTaskAssignmentId,
				RequesterId = requesterId,
				TargetWorkerId = Guid.NewGuid()
			};

			await Assert.ThrowsAsync<NotFoundException>(() => _service.CreateSwapRequestAsync(dto));
		}

		[Fact]
		public async Task CreateSwapRequestAsync_ShouldThrow_WhenPendingSwapExists()
		{
			var requesterId = Guid.NewGuid();
			var targetWorkerId = Guid.NewGuid();
			var taskAssignmentId = Guid.NewGuid();
			var targetTaskAssignmentId = Guid.NewGuid();
			var workAreaId = Guid.NewGuid();
			var now = DateTime.UtcNow;

			_dateTimeProvider.UtcNow.Returns(now);

			var (requesterTask, targetTask) = BuildValidTaskPair(
				requesterId, targetWorkerId,
				taskAssignmentId, targetTaskAssignmentId,
				workAreaId, now);

			_taskAssignmentRepo
				.GetByIdAsync(taskAssignmentId, Arg.Any<CancellationToken>())
				.Returns(requesterTask);

			_taskAssignmentRepo
				.GetByIdAsync(targetTaskAssignmentId, Arg.Any<CancellationToken>())
				.Returns(targetTask);

			// SOP must be stubbed because ValidateWorkerCompetencyAsync runs before HasPendingSwapAsync
			_sopRequirementsQueryService
				.GetSopRequirementsByScheduleId(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
				.Returns(Task.FromResult(NoRequirements()));

			// Pending swap exists
			_swapRequestRepo.HasPendingSwapAsync(taskAssignmentId).Returns(true);

			var dto = new TaskSwapRequestCreateDto
			{
				TaskAssignmentId = taskAssignmentId,
				TargetTaskAssignmentId = targetTaskAssignmentId,
				RequesterId = requesterId,
				TargetWorkerId = targetWorkerId
			};

			await Assert.ThrowsAsync<BadRequestException>(() => _service.CreateSwapRequestAsync(dto));
		}

		[Fact]
		public async Task CreateSwapRequestAsync_ShouldThrow_WhenRequesterHasTimeConflict()
		{
			var requesterId = Guid.NewGuid();
			var targetWorkerId = Guid.NewGuid();
			var taskAssignmentId = Guid.NewGuid();
			var targetTaskAssignmentId = Guid.NewGuid();
			var workAreaId = Guid.NewGuid();
			var now = DateTime.UtcNow;

			_dateTimeProvider.UtcNow.Returns(now);

			var (requesterTask, targetTask) = BuildValidTaskPair(
				requesterId, targetWorkerId,
				taskAssignmentId, targetTaskAssignmentId,
				workAreaId, now);

			_taskAssignmentRepo
				.GetByIdAsync(taskAssignmentId, Arg.Any<CancellationToken>())
				.Returns(requesterTask);

			_taskAssignmentRepo
				.GetByIdAsync(targetTaskAssignmentId, Arg.Any<CancellationToken>())
				.Returns(targetTask);

			_sopRequirementsQueryService
				.GetSopRequirementsByScheduleId(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
				.Returns(Task.FromResult(NoRequirements()));

			_swapRequestRepo.HasPendingSwapAsync(taskAssignmentId).Returns(false);

			// Requester has a conflict
			_taskAssignmentRepo
				.HasTimeConflictAsync(
					taskAssignmentId, requesterId,
					Arg.Any<DateTime>(), Arg.Any<DateTime>(),
					Arg.Any<CancellationToken>())
				.Returns(true);

			var dto = new TaskSwapRequestCreateDto
			{
				TaskAssignmentId = taskAssignmentId,
				TargetTaskAssignmentId = targetTaskAssignmentId,
				RequesterId = requesterId,
				TargetWorkerId = targetWorkerId
			};

			await Assert.ThrowsAsync<BadRequestException>(() => _service.CreateSwapRequestAsync(dto));
		}

		[Fact]
		public async Task CreateSwapRequestAsync_ShouldThrow_WhenTargetHasTimeConflict()
		{
			var requesterId = Guid.NewGuid();
			var targetWorkerId = Guid.NewGuid();
			var taskAssignmentId = Guid.NewGuid();
			var targetTaskAssignmentId = Guid.NewGuid();
			var workAreaId = Guid.NewGuid();
			var now = DateTime.UtcNow;

			_dateTimeProvider.UtcNow.Returns(now);

			var (requesterTask, targetTask) = BuildValidTaskPair(
				requesterId, targetWorkerId,
				taskAssignmentId, targetTaskAssignmentId,
				workAreaId, now);

			_taskAssignmentRepo
				.GetByIdAsync(taskAssignmentId, Arg.Any<CancellationToken>())
				.Returns(requesterTask);

			_taskAssignmentRepo
				.GetByIdAsync(targetTaskAssignmentId, Arg.Any<CancellationToken>())
				.Returns(targetTask);

			_sopRequirementsQueryService
				.GetSopRequirementsByScheduleId(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
				.Returns(Task.FromResult(NoRequirements()));

			_swapRequestRepo.HasPendingSwapAsync(taskAssignmentId).Returns(false);

			// Requester: no conflict
			_taskAssignmentRepo
				.HasTimeConflictAsync(
					taskAssignmentId, requesterId,
					Arg.Any<DateTime>(), Arg.Any<DateTime>(),
					Arg.Any<CancellationToken>())
				.Returns(false);

			// Target: has conflict
			_taskAssignmentRepo
				.HasTimeConflictAsync(
					targetTaskAssignmentId, targetWorkerId,
					Arg.Any<DateTime>(), Arg.Any<DateTime>(),
					Arg.Any<CancellationToken>())
				.Returns(true);

			var dto = new TaskSwapRequestCreateDto
			{
				TaskAssignmentId = taskAssignmentId,
				TargetTaskAssignmentId = targetTaskAssignmentId,
				RequesterId = requesterId,
				TargetWorkerId = targetWorkerId
			};

			await Assert.ThrowsAsync<BadRequestException>(() => _service.CreateSwapRequestAsync(dto));
		}

		[Fact]
		public async Task CreateSwapRequestAsync_ShouldThrow_WhenRequesterTaskTooSoon()
		{
			var requesterId = Guid.NewGuid();
			var now = DateTime.UtcNow;

			_dateTimeProvider.UtcNow.Returns(now);

			// Task starts in only 1 hour — fails the 2 h threshold
			var requesterTask = new TaskAssignment
			{
				Id = Guid.NewGuid(),
				AssigneeId = requesterId,
				Status = TaskAssignmentStatus.NotStarted,
				ScheduledStartAt = now.AddHours(1),
				TaskScheduleId = Guid.NewGuid()
			};

			_taskAssignmentRepo
				.GetByIdAsync(requesterTask.Id, Arg.Any<CancellationToken>())
				.Returns(requesterTask);

			var dto = new TaskSwapRequestCreateDto
			{
				TaskAssignmentId = requesterTask.Id,
				RequesterId = requesterId
			};

			await Assert.ThrowsAsync<BadRequestException>(() => _service.CreateSwapRequestAsync(dto));
		}

		[Fact]
		public async Task CreateSwapRequestAsync_ShouldThrow_WhenWorkerNotQualifiedForTargetTask()
		{
			var requesterId = Guid.NewGuid();
			var targetWorkerId = Guid.NewGuid();
			var taskAssignmentId = Guid.NewGuid();
			var targetTaskAssignmentId = Guid.NewGuid();
			var workAreaId = Guid.NewGuid();
			var now = DateTime.UtcNow;

			_dateTimeProvider.UtcNow.Returns(now);

			var (requesterTask, targetTask) = BuildValidTaskPair(
				requesterId, targetWorkerId,
				taskAssignmentId, targetTaskAssignmentId,
				workAreaId, now);

			_taskAssignmentRepo
				.GetByIdAsync(taskAssignmentId, Arg.Any<CancellationToken>())
				.Returns(requesterTask);

			_taskAssignmentRepo
				.GetByIdAsync(targetTaskAssignmentId, Arg.Any<CancellationToken>())
				.Returns(targetTask);

			// Task A has required skills → target worker B must qualify, but doesn't
			var reqWithSkills = new SopRequirementsIntegrated
			{
				Found = true,
				RequiredSkillIds = new List<Guid> { Guid.NewGuid() },
				RequiredCertificationIds = new List<Guid>()
			};

			_sopRequirementsQueryService
				.GetSopRequirementsByScheduleId(requesterTask.TaskScheduleId, Arg.Any<CancellationToken>())
				.Returns(Task.FromResult(reqWithSkills));

			_sopRequirementsQueryService
				.GetSopRequirementsByScheduleId(targetTask.TaskScheduleId, Arg.Any<CancellationToken>())
				.Returns(Task.FromResult(NoRequirements()));

			_workerCertificationSkillQueryService
				.IsWorkerQualifiedAsync(
					targetWorkerId,
					Arg.Any<List<Guid>>(),
					Arg.Any<List<Guid>>(),
					Arg.Any<CancellationToken>())
				.Returns(false); // B not qualified for A's task

			var dto = new TaskSwapRequestCreateDto
			{
				TaskAssignmentId = taskAssignmentId,
				TargetTaskAssignmentId = targetTaskAssignmentId,
				RequesterId = requesterId,
				TargetWorkerId = targetWorkerId
			};

			await Assert.ThrowsAsync<BadRequestException>(() => _service.CreateSwapRequestAsync(dto));
		}

		// ===============================================================
		// GET SWAP CANDIDATES
		// ===============================================================

		[Fact]
		public async Task GetSwapCandidatesAsync_ShouldReturnCandidates_WhenTaskExists()
		{
			var taskAssignmentId = Guid.NewGuid();
			var workAreaId = Guid.NewGuid();
			var now = DateTime.UtcNow;

			_dateTimeProvider.UtcNow.Returns(now);

			var requesterTask = new TaskAssignment
			{
				Id = taskAssignmentId,
				TaskScheduleId = Guid.NewGuid(),
				WorkAreaId = workAreaId,
				AssigneeId = Guid.NewGuid(),
				ScheduledStartAt = now.AddHours(5),
				ScheduledEndAt = now.AddHours(6)
			};

			_taskAssignmentRepo
				.GetByIdAsync(taskAssignmentId, Arg.Any<CancellationToken>())
				.Returns(requesterTask);

			_sopRequirementsQueryService
				.GetSopRequirementsByScheduleId(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
				.Returns(Task.FromResult(NoRequirements()));

			var candidateAssignments = new List<TaskAssignment>
			{
				new() { Id = Guid.NewGuid(), AssigneeId = Guid.NewGuid() },
				new() { Id = Guid.NewGuid(), AssigneeId = Guid.NewGuid() }
			};

			_taskAssignmentRepo
				.GetSwapCandidatesAsync(
					Arg.Any<Guid>(), Arg.Any<Guid>(),
					Arg.Any<DateTime>(), Arg.Any<DateTime>(),
					Arg.Any<DateTime>(), Arg.Any<DateTime>(),
					Arg.Any<DateOnly?>(), Arg.Any<TimeOnly?>(),
					Arg.Any<List<Guid>>(),
					Arg.Any<PaginationRequest>(),
					Arg.Any<CancellationToken>())
				.Returns(new PaginatedResult<TaskAssignment>(1, 10, 2, candidateAssignments));

			_mapper
				.Map<List<SwapCandidateDto>>(Arg.Any<List<TaskAssignment>>())
				.Returns(call =>
				{
					var src = call.Arg<List<TaskAssignment>>();
					return src.Select(x => new SwapCandidateDto
					{
						WorkerId = x.AssigneeId,
						AssigneeName = "Test User",
						Task = new SwapTaskInfoDto { TaskAssignmentId = x.Id }
					}).ToList();
				});

			var result = await _service.GetSwapCandidatesAsync(
				new GetSwapCandidatesDto { TaskAssignmentId = taskAssignmentId },
				new PaginationRequest(1, 10));

			Assert.True(result.Succeeded);
			Assert.Equal(2, result.Value.Content.Count);
		}

		[Fact]
		public async Task GetSwapCandidatesAsync_ShouldThrow_WhenTaskNotFound()
		{
			var taskAssignmentId = Guid.NewGuid();

			_taskAssignmentRepo
				.GetByIdAsync(taskAssignmentId, Arg.Any<CancellationToken>())
				.Returns((TaskAssignment?)null);

			await Assert.ThrowsAsync<NotFoundException>(() =>
				_service.GetSwapCandidatesAsync(
					new GetSwapCandidatesDto { TaskAssignmentId = taskAssignmentId },
					new PaginationRequest(1, 10)));
		}

		// ===============================================================
		// RESPOND SWAP REQUEST
		// ===============================================================

		[Fact]
		public async Task RespondSwapRequestAsync_ShouldSetPendingManagerApproval_WhenAccepted()
		{
			var swapRequestId = Guid.NewGuid();
			var responderId = Guid.NewGuid();
			var requesterId = Guid.NewGuid();
			var supervisorUserId = Guid.NewGuid();
			// Use a future ExpiredAt so the DateTime.UtcNow check in the service passes
			var expiredAt = DateTime.UtcNow.AddHours(2);

			var swapRequest = new TaskSwapRequest
			{
				Id = swapRequestId,
				Status = SwapRequestStatus.PendingTargetApproval,
				ExpiredAt = expiredAt,
				TargetWorkerId = responderId,
				RequesterId = requesterId,
				TaskAssignment = new TaskAssignment { WorkAreaId = Guid.NewGuid() }
			};

			_swapRequestRepo
				.GetByIdWithDetailsAsync(swapRequestId, Arg.Any<CancellationToken>())
				.Returns(swapRequest);

			_supervisorQueryService
				.GetSupervisorWorkAreasAsync(
					requesterId, responderId,
					swapRequest.TaskAssignment.WorkAreaId,
					Arg.Any<CancellationToken>())
				.Returns(new GetSupervisorWorkAreasResponse
				{
					Found = true,
					SupervisorUserId = supervisorUserId
				});

			StubSaveChanges();

			var result = await _service.RespondSwapRequestAsync(new RespondSwapRequestDto
			{
				SwapRequestId = swapRequestId,
				ResponderId = responderId,
				IsAccepted = true
			});

			Assert.True(result.Succeeded);
			Assert.Equal(SwapRequestStatus.PendingManagerApproval, swapRequest.Status);
			Assert.Equal(supervisorUserId, swapRequest.ReviewedByUserId);
			await _swapRequestRepo.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
		}

		[Fact]
		public async Task RespondSwapRequestAsync_ShouldSetRejectedByTarget_WhenNotAccepted()
		{
			var swapRequestId = Guid.NewGuid();
			var responderId = Guid.NewGuid();
			var expiredAt = DateTime.UtcNow.AddHours(2);

			var swapRequest = new TaskSwapRequest
			{
				Id = swapRequestId,
				Status = SwapRequestStatus.PendingTargetApproval,
				ExpiredAt = expiredAt,
				TargetWorkerId = responderId
			};

			_swapRequestRepo
				.GetByIdWithDetailsAsync(swapRequestId, Arg.Any<CancellationToken>())
				.Returns(swapRequest);

			StubSaveChanges();

			var result = await _service.RespondSwapRequestAsync(new RespondSwapRequestDto
			{
				SwapRequestId = swapRequestId,
				ResponderId = responderId,
				IsAccepted = false
			});

			Assert.True(result.Succeeded);
			Assert.Equal(SwapRequestStatus.RejectedByTarget, swapRequest.Status);
			await _swapRequestRepo.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
		}

		[Fact]
		public async Task RespondSwapRequestAsync_ShouldThrow_WhenSwapRequestNotFound()
		{
			var swapRequestId = Guid.NewGuid();

			_swapRequestRepo
				.GetByIdWithDetailsAsync(swapRequestId, Arg.Any<CancellationToken>())
				.Returns((TaskSwapRequest?)null);

			await Assert.ThrowsAsync<NotFoundException>(() =>
				_service.RespondSwapRequestAsync(new RespondSwapRequestDto
				{
					SwapRequestId = swapRequestId,
					ResponderId = Guid.NewGuid(),
					IsAccepted = true
				}));
		}

		[Fact]
		public async Task RespondSwapRequestAsync_ShouldThrow_WhenWrongResponder()
		{
			var swapRequestId = Guid.NewGuid();
			var correctResponderId = Guid.NewGuid();
			var wrongResponderId = Guid.NewGuid();

			var swapRequest = new TaskSwapRequest
			{
				Id = swapRequestId,
				TargetWorkerId = correctResponderId,
				Status = SwapRequestStatus.PendingTargetApproval,
				ExpiredAt = DateTime.UtcNow.AddHours(2)
			};

			_swapRequestRepo
				.GetByIdWithDetailsAsync(swapRequestId, Arg.Any<CancellationToken>())
				.Returns(swapRequest);

			await Assert.ThrowsAsync<ForbiddenException>(() =>
				_service.RespondSwapRequestAsync(new RespondSwapRequestDto
				{
					SwapRequestId = swapRequestId,
					ResponderId = wrongResponderId,
					IsAccepted = true
				}));
		}

		[Fact]
		public async Task RespondSwapRequestAsync_ShouldThrow_WhenExpired()
		{
			var swapRequestId = Guid.NewGuid();
			var responderId = Guid.NewGuid();

			var swapRequest = new TaskSwapRequest
			{
				Id = swapRequestId,
				TargetWorkerId = responderId,
				Status = SwapRequestStatus.PendingTargetApproval,
				// Already expired — service checks DateTime.UtcNow directly
				ExpiredAt = DateTime.UtcNow.AddHours(-1)
			};

			_swapRequestRepo
				.GetByIdWithDetailsAsync(swapRequestId, Arg.Any<CancellationToken>())
				.Returns(swapRequest);

			StubSaveChanges();

			await Assert.ThrowsAsync<BadRequestException>(() =>
				_service.RespondSwapRequestAsync(new RespondSwapRequestDto
				{
					SwapRequestId = swapRequestId,
					ResponderId = responderId,
					IsAccepted = true
				}));
		}

		[Fact]
		public async Task RespondSwapRequestAsync_ShouldThrow_WhenNotInPendingTargetApprovalState()
		{
			var swapRequestId = Guid.NewGuid();
			var responderId = Guid.NewGuid();

			var swapRequest = new TaskSwapRequest
			{
				Id = swapRequestId,
				TargetWorkerId = responderId,
				// Already moved out of PendingTargetApproval
				Status = SwapRequestStatus.PendingManagerApproval,
				ExpiredAt = DateTime.UtcNow.AddHours(2)
			};

			_swapRequestRepo
				.GetByIdWithDetailsAsync(swapRequestId, Arg.Any<CancellationToken>())
				.Returns(swapRequest);

			await Assert.ThrowsAsync<BadRequestException>(() =>
				_service.RespondSwapRequestAsync(new RespondSwapRequestDto
				{
					SwapRequestId = swapRequestId,
					ResponderId = responderId,
					IsAccepted = true
				}));
		}

		// ===============================================================
		// REVIEW SWAP REQUEST
		// ===============================================================

		[Fact]
		public async Task ReviewSwapRequestAsync_ShouldApproveAndSwapAssignees_WhenApproved()
		{
			var swapRequestId = Guid.NewGuid();
			var userId = Guid.NewGuid();
			var requesterAssigneeId = Guid.NewGuid();
			var targetAssigneeId = Guid.NewGuid();

			_userContext.UserId.Returns(userId);
			_userContext.FullName.Returns("Supervisor A");

			var requesterTask = new TaskAssignment { Id = Guid.NewGuid(), AssigneeId = requesterAssigneeId, AssigneeName = "Worker A" };
			var targetTask = new TaskAssignment { Id = Guid.NewGuid(), AssigneeId = targetAssigneeId, AssigneeName = "Worker B" };

			var swapRequest = new TaskSwapRequest
			{
				Id = swapRequestId,
				Status = SwapRequestStatus.PendingManagerApproval,
				RequesterId = requesterAssigneeId,
				TargetWorkerId = targetAssigneeId,
				RequesterName = "Worker A",
				TargetWorkerName = "Worker B",
				TaskAssignment = requesterTask,
				TargetTaskAssignment = targetTask
			};

			_swapRequestRepo
				.GetByIdWithDetailsAsync(swapRequestId, Arg.Any<CancellationToken>())
				.Returns(swapRequest);

			StubSaveChanges();

			var result = await _service.ReviewSwapRequestAsync(new ReviewSwapRequestDto
			{
				TaskSwapRequestId = swapRequestId,
				IsApproved = true,
				ReviewNote = "Approved"
			});

			Assert.True(result.Succeeded);
			Assert.Equal(SwapRequestStatus.Approved, swapRequest.Status);
			// Assignees should have been swapped
			Assert.Equal(targetAssigneeId, requesterTask.AssigneeId);
			Assert.Equal(requesterAssigneeId, targetTask.AssigneeId);
			await _swapRequestRepo.Received(1).SaveChangesAsync();
		}

		[Fact]
		public async Task ReviewSwapRequestAsync_ShouldRejectRequest_WhenNotApproved()
		{
			var swapRequestId = Guid.NewGuid();
			var userId = Guid.NewGuid();

			_userContext.UserId.Returns(userId);
			_userContext.FullName.Returns("Supervisor A");

			var swapRequest = new TaskSwapRequest
			{
				Id = swapRequestId,
				Status = SwapRequestStatus.PendingManagerApproval,
				RequesterId = Guid.NewGuid(),
				TargetWorkerId = Guid.NewGuid(),
				TaskAssignment = new TaskAssignment { Id = Guid.NewGuid() },
				TargetTaskAssignment = new TaskAssignment { Id = Guid.NewGuid() }
			};

			_swapRequestRepo
				.GetByIdWithDetailsAsync(swapRequestId, Arg.Any<CancellationToken>())
				.Returns(swapRequest);

			StubSaveChanges();

			var result = await _service.ReviewSwapRequestAsync(new ReviewSwapRequestDto
			{
				TaskSwapRequestId = swapRequestId,
				IsApproved = false,
				ReviewNote = "Not appropriate"
			});

			Assert.True(result.Succeeded);
			Assert.Equal(SwapRequestStatus.RejectedByManager, swapRequest.Status);
			await _swapRequestRepo.Received(1).SaveChangesAsync();
		}

		[Fact]
		public async Task ReviewSwapRequestAsync_ShouldThrow_WhenNotFound()
		{
			var swapRequestId = Guid.NewGuid();

			_swapRequestRepo
				.GetByIdWithDetailsAsync(swapRequestId, Arg.Any<CancellationToken>())
				.Returns((TaskSwapRequest?)null);

			await Assert.ThrowsAsync<NotFoundException>(() =>
				_service.ReviewSwapRequestAsync(new ReviewSwapRequestDto
				{
					TaskSwapRequestId = swapRequestId,
					IsApproved = true
				}));
		}

		[Fact]
		public async Task ReviewSwapRequestAsync_ShouldThrow_WhenNotInPendingManagerApprovalState()
		{
			var swapRequestId = Guid.NewGuid();

			var swapRequest = new TaskSwapRequest
			{
				Id = swapRequestId,
				Status = SwapRequestStatus.Approved // already approved
			};

			_swapRequestRepo
				.GetByIdWithDetailsAsync(swapRequestId, Arg.Any<CancellationToken>())
				.Returns(swapRequest);

			await Assert.ThrowsAsync<BadRequestException>(() =>
				_service.ReviewSwapRequestAsync(new ReviewSwapRequestDto
				{
					TaskSwapRequestId = swapRequestId,
					IsApproved = true
				}));
		}
	}
}