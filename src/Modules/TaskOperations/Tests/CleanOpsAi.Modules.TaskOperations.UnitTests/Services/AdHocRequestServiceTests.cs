using AutoMapper;
using CleanOpsAi.BuildingBlocks.Application;
using CleanOpsAi.BuildingBlocks.Application.Exceptions;
using CleanOpsAi.BuildingBlocks.Application.Interfaces;
using CleanOpsAi.Modules.TaskOperations.Application.Common.Interfaces.Repositories;
using CleanOpsAi.Modules.TaskOperations.Application.Common.Interfaces.Services;
using CleanOpsAi.Modules.TaskOperations.Application.DTOs.Request;
using CleanOpsAi.Modules.TaskOperations.Application.DTOs.Response;
using CleanOpsAi.Modules.TaskOperations.Application.Services;
using CleanOpsAi.Modules.TaskOperations.Domain.Entities;
using CleanOpsAi.Modules.TaskOperations.Domain.Enums;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanOpsAi.Modules.TaskOperations.UnitTests.Services
{
    public class AdHocRequestServiceTests
    {
        private readonly IAdHocRequestRepository _repo;
        private readonly IMapper _mapper;
        private readonly IUserContext _userContext;
        private readonly IDateTimeProvider _dateTime;
        private readonly IWorkerQueryService _workerService;
        private readonly ISupervisorQueryService _supervisorService;
        private readonly IWorkAreaQueryService _workAreaService;

        private readonly AdHocRequestService _service;

        public AdHocRequestServiceTests()
        {
            _repo = Substitute.For<IAdHocRequestRepository>();
            _mapper = Substitute.For<IMapper>();
            _userContext = Substitute.For<IUserContext>();
            _dateTime = Substitute.For<IDateTimeProvider>();
            _workerService = Substitute.For<IWorkerQueryService>();
            _supervisorService = Substitute.For<ISupervisorQueryService>();
            _workAreaService = Substitute.For<IWorkAreaQueryService>();

            _service = new AdHocRequestService(
                _repo,
                _mapper,
                _userContext,
                _dateTime,
                _workerService,
                _supervisorService,
                _workAreaService
            );
        }

        // =========================
        // CREATE SUCCESS
        // =========================
        [Fact]
        public async Task Create_ShouldReturnDto()
        {
            var userId = Guid.NewGuid();
            var workerId = Guid.NewGuid();
            var supervisorId = Guid.NewGuid();

            _userContext.UserId.Returns(userId);
            _dateTime.UtcNow.Returns(DateTime.UtcNow);

            var request = new CreateAdHocRequestDto
            {
                WorkAreaId = Guid.NewGuid(),
                RequestDateFrom = DateTime.UtcNow,
                RequestDateTo = DateTime.UtcNow.AddDays(1)
            };

            _workerService.GetWorkerIdByUserIdAsync(userId, default)
                          .Returns(workerId);

            _supervisorService.GetSupervisorIdAsync(
                request.WorkAreaId, workerId, default)
                .Returns(supervisorId);

            var entity = new AdHocRequest
            {
                Id = Guid.NewGuid(),
                RequestedByWorkerId = workerId
            };

            _mapper.Map<AdHocRequest>(request).Returns(entity);

            var dto = new AdHocRequestDto
            {
                Id = entity.Id,
                RequestedByWorkerId = workerId
            };

            _mapper.Map<AdHocRequestDto>(entity).Returns(dto);

            _workerService.GetUserNames(Arg.Any<List<Guid>>())
                .Returns(new Dictionary<Guid, string> { { workerId, "Worker A" } });

            _workAreaService.GetWorkAreaNameAsync(request.WorkAreaId, default)
                .Returns("Area A");

            _supervisorService.GetSupervisorNameAsync(supervisorId, default)
                .Returns("Supervisor A");

            var result = await _service.Create(request);

            Assert.NotNull(result);
            Assert.Equal("Worker A", result.WorkerName);
            Assert.Equal("Area A", result.WorkAreaName);

            await _repo.Received(1).AddAsync(Arg.Any<AdHocRequest>(), default);
        }

        // =========================
        // CREATE FAIL - NO WORKER
        // =========================
        [Fact]
        public async Task Create_ShouldThrow_WhenWorkerNotFound()
        {
            _userContext.UserId.Returns(Guid.NewGuid());

            _workerService.GetWorkerIdByUserIdAsync(Arg.Any<Guid>(), default)
                          .Returns((Guid?)null);

            var request = new CreateAdHocRequestDto
            {
                RequestDateFrom = DateTime.UtcNow
            };

            await Assert.ThrowsAsync<BadRequestException>(() => _service.Create(request));
        }

        // =========================
        // CREATE FAIL - INVALID DATE
        // =========================
        [Fact]
        public async Task Create_ShouldThrow_WhenFromGreaterThanTo()
        {
            var userId = Guid.NewGuid();
            _userContext.UserId.Returns(userId);

            _workerService.GetWorkerIdByUserIdAsync(userId, default)
                          .Returns(Guid.NewGuid());

            var request = new CreateAdHocRequestDto
            {
                RequestDateFrom = DateTime.UtcNow,
                RequestDateTo = DateTime.UtcNow.AddDays(-1)
            };

            await Assert.ThrowsAsync<BadRequestException>(() => _service.Create(request));
        }

        // =========================
        // GET BY ID
        // =========================
        [Fact]
        public async Task GetById_ShouldReturnDto()
        {
            var id = Guid.NewGuid();
            var entity = new AdHocRequest
            {
                Id = id,
                RequestedByWorkerId = Guid.NewGuid()
            };

            _repo.GetByIdExistAsync(id, default).Returns(entity);

            var dto = new AdHocRequestDto
            {
                Id = id,
                RequestedByWorkerId = entity.RequestedByWorkerId
            };

            _mapper.Map<AdHocRequestDto>(entity).Returns(dto);

            _workerService.GetUserNames(Arg.Any<List<Guid>>())
                .Returns(new Dictionary<Guid, string>
                {
                    { entity.RequestedByWorkerId, "Worker A" }
                });

            var result = await _service.GetById(id);

            Assert.NotNull(result);
            Assert.Equal("Worker A", result.WorkerName);
        }

        // =========================
        // UPDATE SUCCESS
        // =========================
        [Fact]
        public async Task Update_ShouldUpdateEntity()
        {
            var id = Guid.NewGuid();

            var entity = new AdHocRequest
            {
                Id = id,
                Status = AdHocRequestStatus.Pending,
                RequestedByWorkerId = Guid.NewGuid()
            };

            _repo.GetByIdExistAsync(id, default).Returns(entity);

            _userContext.UserId.Returns(Guid.NewGuid());
            _dateTime.UtcNow.Returns(DateTime.UtcNow);

            var dto = new UpdateAdHocRequestDto
            {
                Reason = "Updated"
            };

            _mapper.Map<AdHocRequestDto>(entity)
                .Returns(new AdHocRequestDto { Id = id });

            _workerService.GetUserNames(Arg.Any<List<Guid>>())
                .Returns(new Dictionary<Guid, string>());

            var result = await _service.Update(id, dto);

            Assert.NotNull(result);

            await _repo.Received(1).UpdateAsync(entity, default);
        }

        // =========================
        // UPDATE FAIL - APPROVED
        // =========================
        [Fact]
        public async Task Update_ShouldThrow_WhenApproved()
        {
            var entity = new AdHocRequest
            {
                Id = Guid.NewGuid(),
                Status = AdHocRequestStatus.Approved
            };

            _repo.GetByIdExistAsync(entity.Id, default).Returns(entity);

            await Assert.ThrowsAsync<BadRequestException>(() =>
                _service.Update(entity.Id, new UpdateAdHocRequestDto()));
        }

        // =========================
        // DELETE
        // =========================
        [Fact]
        public async Task Delete_ShouldReturnTrue()
        {
            var entity = new AdHocRequest
            {
                Id = Guid.NewGuid()
            };

            _repo.GetByIdExistAsync(entity.Id, default).Returns(entity);

            var result = await _service.Delete(entity.Id);

            Assert.True(result);

            await _repo.Received(1).DeleteAsync(entity, default);
        }

        // =========================
        // REVIEW
        // =========================
        [Fact]
        public async Task Review_ShouldUpdateStatus()
        {
            var entity = new AdHocRequest
            {
                Id = Guid.NewGuid(),
                RequestedByWorkerId = Guid.NewGuid()
            };

            _repo.GetByIdExistAsync(entity.Id, default).Returns(entity);

            _dateTime.UtcNow.Returns(DateTime.UtcNow);

            var dto = new ReviewAdHocRequestDto
            {
                Status = AdHocRequestStatus.Approved
            };

            _mapper.Map<AdHocRequestDto>(entity)
                .Returns(new AdHocRequestDto { Id = entity.Id });

            _workerService.GetUserNames(Arg.Any<List<Guid>>())
                .Returns(new Dictionary<Guid, string>());

            var result = await _service.Review(entity.Id, dto);

            Assert.NotNull(result);
            Assert.Equal(AdHocRequestStatus.Approved, entity.Status);

            await _repo.Received(1).UpdateAsync(entity, default);
        }
    }
}
