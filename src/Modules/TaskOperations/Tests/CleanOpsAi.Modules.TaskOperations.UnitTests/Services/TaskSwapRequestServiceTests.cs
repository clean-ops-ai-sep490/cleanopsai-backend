using AutoMapper;
using CleanOpsAi.BuildingBlocks.Application;
using CleanOpsAi.BuildingBlocks.Application.Exceptions;
using CleanOpsAi.BuildingBlocks.Application.Interfaces;
using CleanOpsAi.BuildingBlocks.Application.Pagination;
using CleanOpsAi.Modules.TaskOperations.Application.Common.Interfaces.Repositories;
using CleanOpsAi.Modules.TaskOperations.Application.Common.Interfaces.Services;
using CleanOpsAi.Modules.TaskOperations.Application.DTOs.Request;
using CleanOpsAi.Modules.TaskOperations.Application.DTOs.Response;
using CleanOpsAi.Modules.TaskOperations.Application.Services;
using CleanOpsAi.BuildingBlocks.Infrastructure.Events.Request;
using CleanOpsAi.Modules.TaskOperations.Domain.Entities;
using CleanOpsAi.Modules.TaskOperations.Domain.Enums;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

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

        private readonly TaskSwapRequestService _service;

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
                _supervisorQueryService
            );
        }

        // =========================
        // GET BY ID SUCCESS
        // =========================
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

            _swapRequestRepo.GetByIdWithDetailsAsync(swapRequestId, default)
                .Returns(entity);

            var dto = new SwapRequestDto
            {
                Id = swapRequestId,
                RequesterName = "Worker A",
                TargetWorkerName = "Worker B"
            };

            _mapper.Map<SwapRequestDto>(entity).Returns(dto);

            var result = await _service.GetById(swapRequestId);

            Assert.NotNull(result);
            Assert.Equal(swapRequestId, result.Id);
            Assert.Equal("Worker A", result.RequesterName);
        }

        // =========================
        // GET BY ID FAIL - NOT FOUND
        // =========================
        [Fact]
        public async Task GetById_ShouldThrow_WhenSwapRequestNotFound()
        {
            var swapRequestId = Guid.NewGuid();

			_swapRequestRepo.GetByIdWithDetailsAsync(swapRequestId, Arg.Any<CancellationToken>()).Returns((TaskSwapRequest?)null);

			await Assert.ThrowsAsync<NotFoundException>(() => _service.GetById(swapRequestId));
        }   

        // =========================
        // CANCEL SUCCESS
        // =========================
        [Fact]
        public async Task CancelSwapRequestAsync_ShouldCancelRequest()
        {
            var swapRequestId = Guid.NewGuid();
            var requesterId = Guid.NewGuid();

            var entity = new TaskSwapRequest
            {
                Id = swapRequestId,
                Status = SwapRequestStatus.PendingTargetApproval
            };

            _swapRequestRepo.GetByIdWithDetailsAsync(swapRequestId, default)
                .Returns(entity);

            var result = await _service.CancelSwapRequestAsync(swapRequestId, requesterId);

            Assert.True(result.Succeeded);
            Assert.Equal(SwapRequestStatus.CancelledByRequester, entity.Status);
            await _swapRequestRepo.Received(1).SaveChangesAsync(default);
        }

        // =========================
        // CANCEL FAIL - NOT FOUND
        // =========================
        [Fact]
        public async Task CancelSwapRequestAsync_ShouldThrow_WhenNotFound()
        {
            var swapRequestId = Guid.NewGuid();
            var requesterId = Guid.NewGuid();

            _swapRequestRepo.GetByIdWithDetailsAsync(swapRequestId, default)
                .Returns((TaskSwapRequest)null);

            await Assert.ThrowsAsync<NotFoundException>(() =>
                _service.CancelSwapRequestAsync(swapRequestId, requesterId));
        }

		// =========================
		// CREATE SWAP REQUEST SUCCESS
		// =========================
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
			_idGenerator.Generate().Returns(Guid.NewGuid());

			var requesterTask = new TaskAssignment
			{
				Id = taskAssignmentId,
				AssigneeId = requesterId,
				Status = TaskAssignmentStatus.NotStarted,
				ScheduledStartAt = now.AddDays(2),
				ScheduledEndAt = now.AddDays(2).AddHours(1),
				WorkAreaId = workAreaId,
				TaskScheduleId = Guid.NewGuid() // 🔥 quan trọng
			};

			var targetTask = new TaskAssignment
			{
				Id = targetTaskAssignmentId,
				AssigneeId = targetWorkerId,
				Status = TaskAssignmentStatus.NotStarted,
				ScheduledStartAt = now.AddDays(1),
				ScheduledEndAt = now.AddDays(1).AddHours(1),
				WorkAreaId = workAreaId,
				TaskScheduleId = Guid.NewGuid() // 🔥 quan trọng
			};

			// repo
			_taskAssignmentRepo
				.GetByIdAsync(taskAssignmentId, Arg.Any<CancellationToken>())
				.Returns(requesterTask);

			_taskAssignmentRepo
				.GetByIdAsync(targetTaskAssignmentId, Arg.Any<CancellationToken>())
				.Returns(targetTask);

			_swapRequestRepo
				.HasPendingSwapAsync(taskAssignmentId)
				.Returns(false);

			_taskAssignmentRepo
				.HasTimeConflictAsync(
					Arg.Any<Guid>(), requesterId,
					Arg.Any<DateTime>(), Arg.Any<DateTime>(),
					Arg.Any<CancellationToken>())
				.Returns(false);

			_taskAssignmentRepo
				.HasTimeConflictAsync(
					Arg.Any<Guid>(), targetWorkerId,
					Arg.Any<DateTime>(), Arg.Any<DateTime>(),
					Arg.Any<CancellationToken>())
				.Returns(false); 

			_sopRequirementsQueryService
				.GetSopRequirementsByScheduleId(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
				.Returns(Task.FromResult(new SopRequirementsIntegrated
				{
					Found = false, // skip validation
					RequiredSkillIds = new List<Guid>(),
					RequiredCertificationIds = new List<Guid>()
				}));

			_workerCertificationSkillQueryService
				.IsWorkerQualifiedAsync(
					Arg.Any<Guid>(),
					Arg.Any<List<Guid>>(),
					Arg.Any<List<Guid>>(),
					Arg.Any<CancellationToken>())
				.Returns(true);

			// user name
			_workerQueryService.GetUserNames(Arg.Any<List<Guid>>())
				.Returns(new Dictionary<Guid, string>
				{
			{ requesterId, "Worker A" },
			{ targetWorkerId, "Worker B" }
				});

			// mapper
			_mapper.Map<SwapRequestDto>(Arg.Any<TaskSwapRequest>())
				.Returns(new SwapRequestDto
				{
					Id = Guid.NewGuid(),
					RequesterName = "Worker A",
					TargetWorkerName = "Worker B"
				});

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

			await _swapRequestRepo.Received(1)
				.InsertAsync(Arg.Any<TaskSwapRequest>(), Arg.Any<CancellationToken>());

			await _swapRequestRepo.Received(1)
				.SaveChangesAsync(Arg.Any<CancellationToken>());
		}

		// =========================
		// CREATE FAIL - REQUESTER TASK NOT FOUND
		// =========================
		[Fact]
        public async Task CreateSwapRequestAsync_ShouldThrow_WhenRequesterTaskNotFound()
        {
            var taskAssignmentId = Guid.NewGuid();

            _taskAssignmentRepo.GetByIdAsync(taskAssignmentId, default)
                .Returns((TaskAssignment)null);

            var dto = new TaskSwapRequestCreateDto
            {
                TaskAssignmentId = taskAssignmentId,
                RequesterId = Guid.NewGuid()
            };

            await Assert.ThrowsAsync<NotFoundException>(() =>
                _service.CreateSwapRequestAsync(dto));
        }

        // =========================
        // CREATE FAIL - TARGET TASK NOT FOUND
        // =========================
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
                ScheduledStartAt = now.AddHours(3)
            };

            _taskAssignmentRepo.GetByIdAsync(taskAssignmentId, default)
                .Returns(requesterTask);

            _taskAssignmentRepo.GetByIdAsync(targetTaskAssignmentId, default)
                .Returns((TaskAssignment)null);

            var dto = new TaskSwapRequestCreateDto
            {
                TaskAssignmentId = taskAssignmentId,
                TargetTaskAssignmentId = targetTaskAssignmentId,
                RequesterId = requesterId,
                TargetWorkerId = Guid.NewGuid()
            };

            await Assert.ThrowsAsync<NotFoundException>(() =>
                _service.CreateSwapRequestAsync(dto));
        }

        // =========================
        // CREATE FAIL - PENDING SWAP EXISTS
        // =========================
        [Fact]
        public async Task CreateSwapRequestAsync_ShouldThrow_WhenPendingSwapExists()
        {
            var taskAssignmentId = Guid.NewGuid();
            var targetTaskAssignmentId = Guid.NewGuid();
            var requesterId = Guid.NewGuid();
            var targetWorkerId = Guid.NewGuid();
            var workAreaId = Guid.NewGuid();
            var now = DateTime.UtcNow;

            _dateTimeProvider.UtcNow.Returns(now);

            var requesterTask = new TaskAssignment
            {
                Id = taskAssignmentId,
                AssigneeId = requesterId,
                Status = TaskAssignmentStatus.NotStarted,
                ScheduledStartAt = now.AddHours(3),
                ScheduledEndAt = now.AddHours(4),
                WorkAreaId = workAreaId
            };

            var targetTask = new TaskAssignment
            {
                Id = targetTaskAssignmentId,
                AssigneeId = targetWorkerId,
                Status = TaskAssignmentStatus.NotStarted,
                ScheduledStartAt = now.AddHours(5),
                ScheduledEndAt = now.AddHours(6),
                WorkAreaId = workAreaId
            };

            _taskAssignmentRepo.GetByIdAsync(taskAssignmentId, default)
                .Returns(requesterTask);

            _taskAssignmentRepo.GetByIdAsync(targetTaskAssignmentId, default)
                .Returns(targetTask);

            _swapRequestRepo.HasPendingSwapAsync(taskAssignmentId)
                .Returns(true);

            var dto = new TaskSwapRequestCreateDto
            {
                TaskAssignmentId = taskAssignmentId,
                TargetTaskAssignmentId = targetTaskAssignmentId,
                RequesterId = requesterId,
                TargetWorkerId = targetWorkerId
            };

            await Assert.ThrowsAsync<BadRequestException>(() =>
                _service.CreateSwapRequestAsync(dto));
        }

        // =========================
        // GET SWAP CANDIDATES SUCCESS
        // =========================
        [Fact]
        public async Task GetSwapCandidatesAsync_ShouldReturnCandidates()
        {
            var taskAssignmentId = Guid.NewGuid();
            var taskScheduleId = Guid.NewGuid();
            var workAreaId = Guid.NewGuid();
            var now = DateTime.UtcNow;

            _dateTimeProvider.UtcNow.Returns(now);

            var requesterTask = new TaskAssignment
            {
                Id = taskAssignmentId,
                TaskScheduleId = taskScheduleId,
                WorkAreaId = workAreaId,
                AssigneeId = Guid.NewGuid(),
                ScheduledStartAt = now.AddHours(5),
                ScheduledEndAt = now.AddHours(6)
            };

			_sopRequirementsQueryService
	            .GetSopRequirementsByScheduleId(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
	            .Returns(Task.FromResult(new SopRequirementsIntegrated
	            {
		            Found = false,
		            RequiredSkillIds = new List<Guid>(),
		            RequiredCertificationIds = new List<Guid>()
	            }));

			_mapper
	            .Map<List<SwapCandidateDto>>(Arg.Any<List<TaskAssignment>>())
	            .Returns(call =>
	            {
		            var src = call.Arg<List<TaskAssignment>>();

		            return src.Select(x => new SwapCandidateDto
		            {
			            WorkerId = x.AssigneeId,  
			            AssigneeName = "Test User",
			            Task = new SwapTaskInfoDto
			            {
				            TaskAssignmentId = x.Id
			            }
		            }).ToList();
	            }); 

			_taskAssignmentRepo.GetByIdAsync(taskAssignmentId, default)
                .Returns(requesterTask);

            var paginationRequest = new PaginationRequest(1, 10);

            _taskAssignmentRepo.GetSwapCandidatesAsync(
                Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<DateTime>(), Arg.Any<DateTime>(),
                Arg.Any<DateTime>(), Arg.Any<DateTime>(), Arg.Any<DateOnly?>(),
                Arg.Any<TimeOnly?>(), Arg.Any<List<Guid>>(),
                Arg.Any<PaginationRequest>(), default)
                .Returns(new PaginatedResult<TaskAssignment>(1, 10, 2, new List<TaskAssignment> 
                { 
                    new TaskAssignment { Id = Guid.NewGuid() },
                    new TaskAssignment { Id = Guid.NewGuid() }
                }));

            var getDto = new GetSwapCandidatesDto { TaskAssignmentId = taskAssignmentId };

            var result = await _service.GetSwapCandidatesAsync(getDto, paginationRequest);

            Assert.True(result.Succeeded);
            Assert.Equal(2, result.Value.Content.Count);
        }

        // =========================
        // RESPOND SWAP REQUEST SUCCESS - ACCEPT
        // =========================
        [Fact]
        public async Task RespondSwapRequestAsync_ShouldSetPendingManagerApproval_WhenAccepted()
        {
            var swapRequestId = Guid.NewGuid();
            var responderId = Guid.NewGuid();
            var requesterId = Guid.NewGuid();
            var supervisorUserId = Guid.NewGuid();
            var now = DateTime.UtcNow;

            _dateTimeProvider.UtcNow.Returns(now);

            var swapRequest = new TaskSwapRequest
            {
                Id = swapRequestId,
                Status = SwapRequestStatus.PendingTargetApproval,
                ExpiredAt = now.AddHours(1),
                TargetWorkerId = responderId,
                RequesterId = requesterId,
                TaskAssignment = new TaskAssignment { WorkAreaId = Guid.NewGuid() }
            };

            _swapRequestRepo.GetByIdWithDetailsAsync(swapRequestId, default)
                .Returns(swapRequest);

            _supervisorQueryService.GetSupervisorWorkAreasAsync(
                requesterId, responderId, swapRequest.TaskAssignment.WorkAreaId, default)
                .Returns(new GetSupervisorWorkAreasResponse { Found = true, SupervisorUserId = supervisorUserId });

            var dto = new RespondSwapRequestDto
            {
                SwapRequestId = swapRequestId,
                ResponderId = responderId,
                IsAccepted = true
            };

            var result = await _service.RespondSwapRequestAsync(dto);

            Assert.True(result.Succeeded);
            Assert.Equal(SwapRequestStatus.PendingManagerApproval, swapRequest.Status);
            Assert.Equal(supervisorUserId, swapRequest.ReviewedByUserId);
            await _swapRequestRepo.Received(1).SaveChangesAsync(default);
        }

        // =========================
        // RESPOND SWAP REQUEST SUCCESS - REJECT
        // =========================
        [Fact]
        public async Task RespondSwapRequestAsync_ShouldSetRejectedByTarget_WhenNotAccepted()
        {
            var swapRequestId = Guid.NewGuid();
            var responderId = Guid.NewGuid();
            var now = DateTime.UtcNow;

            _dateTimeProvider.UtcNow.Returns(now);

            var swapRequest = new TaskSwapRequest
            {
                Id = swapRequestId,
                Status = SwapRequestStatus.PendingTargetApproval,
                ExpiredAt = now.AddHours(1),
                TargetWorkerId = responderId
            };

            _swapRequestRepo.GetByIdWithDetailsAsync(swapRequestId, default)
                .Returns(swapRequest);

            var dto = new RespondSwapRequestDto
            {
                SwapRequestId = swapRequestId,
                ResponderId = responderId,
                IsAccepted = false
            };

            var result = await _service.RespondSwapRequestAsync(dto);

            Assert.True(result.Succeeded);
            Assert.Equal(SwapRequestStatus.RejectedByTarget, swapRequest.Status);
            await _swapRequestRepo.Received(1).SaveChangesAsync(default);
        }

        // =========================
        // RESPOND FAIL - WRONG RESPONDER
        // =========================
        [Fact]
        public async Task RespondSwapRequestAsync_ShouldThrow_WhenWrongResponder()
        {
            var swapRequestId = Guid.NewGuid();
            var responderId = Guid.NewGuid();
            var correctResponderId = Guid.NewGuid();

            var swapRequest = new TaskSwapRequest
            {
                Id = swapRequestId,
                TargetWorkerId = correctResponderId,
                Status = SwapRequestStatus.PendingTargetApproval
            };

            _swapRequestRepo.GetByIdWithDetailsAsync(swapRequestId, default)
                .Returns(swapRequest);

            var dto = new RespondSwapRequestDto
            {
                SwapRequestId = swapRequestId,
                ResponderId = responderId,
                IsAccepted = true
            };

            await Assert.ThrowsAsync<ForbiddenException>(() =>
                _service.RespondSwapRequestAsync(dto));
        }

        // =========================
        // RESPOND FAIL - EXPIRED
        // =========================
        [Fact]
        public async Task RespondSwapRequestAsync_ShouldThrow_WhenExpired()
        {
            var swapRequestId = Guid.NewGuid();
            var responderId = Guid.NewGuid();
            var now = DateTime.UtcNow;

            _dateTimeProvider.UtcNow.Returns(now);

            var swapRequest = new TaskSwapRequest
            {
                Id = swapRequestId,
                TargetWorkerId = responderId,
                Status = SwapRequestStatus.PendingTargetApproval,
                ExpiredAt = now.AddHours(-1) // Already expired
            };

            _swapRequestRepo.GetByIdWithDetailsAsync(swapRequestId, default)
                .Returns(swapRequest);

            var dto = new RespondSwapRequestDto
            {
                SwapRequestId = swapRequestId,
                ResponderId = responderId,
                IsAccepted = true
            };

            await Assert.ThrowsAsync<BadRequestException>(() =>
                _service.RespondSwapRequestAsync(dto));
        }

        // =========================
        // REVIEW SWAP REQUEST SUCCESS - APPROVE
        // =========================
        [Fact]
        public async Task ReviewSwapRequestAsync_ShouldApproveRequest()
        {
            var swapRequestId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var now = DateTime.UtcNow;

            _dateTimeProvider.UtcNow.Returns(now);
            _userContext.UserId.Returns(userId);
            _userContext.FullName.Returns("Supervisor A");

			var requesterTask = new TaskAssignment
			{
				Id = Guid.NewGuid(),
				AssigneeId = Guid.NewGuid()
			};

			var targetTask = new TaskAssignment
			{
				Id = Guid.NewGuid(),
				AssigneeId = Guid.NewGuid()
			};

			var swapRequest = new TaskSwapRequest
			{
				Id = swapRequestId,
				Status = SwapRequestStatus.PendingManagerApproval,
				TaskAssignment = requesterTask,      
				TargetTaskAssignment = targetTask     
			};

			_swapRequestRepo.GetByIdWithDetailsAsync(swapRequestId, default)
                .Returns(swapRequest);

            var dto = new ReviewSwapRequestDto
            {
                TaskSwapRequestId = swapRequestId,
                IsApproved = true,
                ReviewNote = "Approved"
            };

            var result = await _service.ReviewSwapRequestAsync(dto);

            Assert.True(result.Succeeded);
            await _swapRequestRepo.Received(1).SaveChangesAsync(default);
        }

        // =========================
        // REVIEW FAIL - NOT FOUND
        // =========================
        [Fact]
        public async Task ReviewSwapRequestAsync_ShouldThrow_WhenNotFound()
        {
            var swapRequestId = Guid.NewGuid();

            _swapRequestRepo.GetByIdWithDetailsAsync(swapRequestId, default)
                .Returns((TaskSwapRequest)null);

            var dto = new ReviewSwapRequestDto
            {
                TaskSwapRequestId = swapRequestId,
                IsApproved = true
            };

            await Assert.ThrowsAsync<NotFoundException>(() =>
                _service.ReviewSwapRequestAsync(dto));
        }

        // =========================
        // REVIEW FAIL - NOT IN PENDING STATE
        // =========================
        [Fact]
        public async Task ReviewSwapRequestAsync_ShouldThrow_WhenNotInPendingState()
        {
            var swapRequestId = Guid.NewGuid();

            var swapRequest = new TaskSwapRequest
            {
                Id = swapRequestId,
                Status = SwapRequestStatus.Approved
            };

            _swapRequestRepo.GetByIdWithDetailsAsync(swapRequestId, default)
                .Returns(swapRequest);

            var dto = new ReviewSwapRequestDto
            {
                TaskSwapRequestId = swapRequestId,
                IsApproved = true
            };

            await Assert.ThrowsAsync<BadRequestException>(() =>
                _service.ReviewSwapRequestAsync(dto));
        }
    }
}
