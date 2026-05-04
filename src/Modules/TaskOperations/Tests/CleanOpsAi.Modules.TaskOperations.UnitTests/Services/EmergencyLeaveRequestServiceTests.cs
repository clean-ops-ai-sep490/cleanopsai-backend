using AutoMapper;
using CleanOpsAi.BuildingBlocks.Application;
using CleanOpsAi.BuildingBlocks.Application.Interfaces;
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
    public class EmergencyLeaveRequestServiceTests
    {
        private readonly IEmergencyLeaveRequestRepository _repo;
        private readonly IFileStorageService _fileStorage;
        private readonly IMapper _mapper;
        private readonly IUserContext _userContext;
        private readonly IDateTimeProvider _dateTime;
        private readonly IWorkerQueryService _workerService; 
        private readonly ITaskAssignmentRepository _taskAssignmentRepo;
		private readonly INotificationPublisher _notificationPublisher;
        private readonly IIdGenerator _idGenerator;


		private readonly EmergencyLeaveRequestService _service;

        public EmergencyLeaveRequestServiceTests()
        {
            _repo = Substitute.For<IEmergencyLeaveRequestRepository>();
            _fileStorage = Substitute.For<IFileStorageService>();
            _mapper = Substitute.For<IMapper>();
            _userContext = Substitute.For<IUserContext>();
            _dateTime = Substitute.For<IDateTimeProvider>();
            _workerService = Substitute.For<IWorkerQueryService>(); 
            _taskAssignmentRepo = Substitute.For<ITaskAssignmentRepository>();
            _notificationPublisher = Substitute.For<INotificationPublisher>();
            _idGenerator = Substitute.For<IIdGenerator>();

			_service = new EmergencyLeaveRequestService(
                _repo,
                _fileStorage,
                _mapper,
                _userContext,
                _dateTime,
                _workerService,
                _taskAssignmentRepo,
				_notificationPublisher,
				_idGenerator
			);
        }

        // =========================
        // CREATE SUCCESS (WITH AUDIO)
        // =========================
        [Fact]
        public async Task Create_ShouldUploadFile_AndReturnDto()
        {
            var userId = Guid.NewGuid();
            _userContext.UserId.Returns(userId);
            _dateTime.UtcNow.Returns(DateTime.UtcNow);

            var request = new CreateEmergencyLeaveRequestDto
            {
                LeaveDateFrom = DateTime.UtcNow,
                LeaveDateTo = DateTime.UtcNow.AddDays(1),
                AudioFileName = "test.mp3",
                AudioStream = new MemoryStream()
            };

            var entity = new EmergencyLeaveRequest
            {
                Id = Guid.NewGuid(),
                WorkerId = Guid.NewGuid()
            };

            _mapper.Map<EmergencyLeaveRequest>(request).Returns(entity);

            _fileStorage.UploadFileAsync(
                Arg.Any<Stream>(),
                Arg.Any<string>(),
                Arg.Any<string>())
                .Returns("audio-url");

            var dto = new EmergencyLeaveRequestDto
            {
                Id = entity.Id,
                WorkerId = entity.WorkerId
            };

            _mapper.Map<EmergencyLeaveRequestDto>(entity).Returns(dto);

            _workerService.GetUserNames(Arg.Any<List<Guid>>())
                .Returns(new Dictionary<Guid, string>
                {
                    { entity.WorkerId, "Worker A" }
                });

            var result = await _service.Create(request);

            Assert.NotNull(result);
            Assert.Equal("audio-url", entity.AudioUrl);
            Assert.Equal("Worker A", result.WorkerName);

            await _repo.Received(1).AddAsync(entity, default);
        }

        // =========================
        // CREATE FAIL - INVALID DATE
        // =========================
        [Fact]
        public async Task Create_ShouldThrow_WhenDateInvalid()
        {
            var request = new CreateEmergencyLeaveRequestDto
            {
                LeaveDateFrom = DateTime.UtcNow,
                LeaveDateTo = DateTime.UtcNow.AddDays(-1)
            };

            await Assert.ThrowsAsync<ArgumentException>(() => _service.Create(request));
        }

        // =========================
        // GET BY ID
        // =========================
        [Fact]
        public async Task GetById_ShouldReturnDto()
        {
            var entity = new EmergencyLeaveRequest
            {
                Id = Guid.NewGuid(),
                WorkerId = Guid.NewGuid()
            };

            _repo.GetByIdExistAsync(entity.Id, default).Returns(entity);

            var dto = new EmergencyLeaveRequestDto
            {
                Id = entity.Id,
                WorkerId = entity.WorkerId
            };

            _mapper.Map<EmergencyLeaveRequestDto>(entity).Returns(dto);

            _workerService.GetUserNames(Arg.Any<List<Guid>>())
                .Returns(new Dictionary<Guid, string>
                {
                    { entity.WorkerId, "Worker A" }
                });

            var result = await _service.GetById(entity.Id);

            Assert.NotNull(result);
            Assert.Equal("Worker A", result.WorkerName);
        }

        // =========================
        // UPDATE SUCCESS
        // =========================
        [Fact]
        public async Task Update_ShouldUpdateEntity()
        {
            var entity = new EmergencyLeaveRequest
            {
                Id = Guid.NewGuid(),
                WorkerId = Guid.NewGuid(),
                LeaveDateFrom = DateTime.UtcNow,
                LeaveDateTo = DateTime.UtcNow.AddDays(1)
            };

            _repo.GetByIdExistAsync(entity.Id, default).Returns(entity);

            _userContext.UserId.Returns(Guid.NewGuid());
            _dateTime.UtcNow.Returns(DateTime.UtcNow);

            var dto = new UpdateEmergencyLeaveRequestDto
            {
                LeaveDateTo = DateTime.UtcNow.AddDays(2)
            };

            _mapper.Map<EmergencyLeaveRequestDto>(entity)
                .Returns(new EmergencyLeaveRequestDto { Id = entity.Id });

            _workerService.GetUserNames(Arg.Any<List<Guid>>())
                .Returns(new Dictionary<Guid, string>());

            var result = await _service.Update(entity.Id, dto);

            Assert.NotNull(result);
            Assert.Equal(dto.LeaveDateTo, entity.LeaveDateTo);

            await _repo.Received(1).UpdateAsync(entity, default);
        }

        // =========================
        // UPDATE FAIL - INVALID DATE
        // =========================
        [Fact]
        public async Task Update_ShouldThrow_WhenDateInvalid()
        {
            var entity = new EmergencyLeaveRequest
            {
                Id = Guid.NewGuid(),
                LeaveDateFrom = DateTime.UtcNow,
                LeaveDateTo = DateTime.UtcNow.AddDays(1)
            };

            _repo.GetByIdExistAsync(entity.Id, default).Returns(entity);

            var dto = new UpdateEmergencyLeaveRequestDto
            {
                LeaveDateFrom = DateTime.UtcNow.AddDays(5),
                LeaveDateTo = DateTime.UtcNow
            };

            await Assert.ThrowsAsync<ArgumentException>(() =>
                _service.Update(entity.Id, dto));
        }

        // =========================
        // REVIEW
        // =========================
        [Fact]
        public async Task Review_ShouldUpdateStatus()
        {
            var entity = new EmergencyLeaveRequest
            {
                Id = Guid.NewGuid(),
                WorkerId = Guid.NewGuid()
            };

            _repo.GetByIdExistAsync(entity.Id, default).Returns(entity);

            _userContext.UserId.Returns(Guid.NewGuid());
            _userContext.FullName.Returns("Supervisor A");
            _dateTime.UtcNow.Returns(DateTime.UtcNow);

            var dto = new ReviewEmergencyLeaveRequestDto
            {
                Status = RequestStatus.Approved
            };

            _mapper.Map<EmergencyLeaveRequestDto>(entity)
                .Returns(new EmergencyLeaveRequestDto { Id = entity.Id });

            _workerService.GetUserNames(Arg.Any<List<Guid>>())
                .Returns(new Dictionary<Guid, string>());

            var result = await _service.Review(entity.Id, dto);

            Assert.NotNull(result);
            Assert.Equal(RequestStatus.Approved, entity.Status);
            Assert.Equal("Supervisor A", result.ReviewedByUserName);

            await _repo.Received(1).UpdateAsync(entity, default);
        }

        // =========================
        // DELETE
        // =========================
        [Fact]
        public async Task Delete_ShouldReturnTrue()
        {
            var entity = new EmergencyLeaveRequest
            {
                Id = Guid.NewGuid()
            };

            _repo.GetByIdExistAsync(entity.Id, default).Returns(entity);

            var result = await _service.Delete(entity.Id);

            Assert.True(result);

            await _repo.Received(1).DeleteAsync(entity, default);
        }
    }
}