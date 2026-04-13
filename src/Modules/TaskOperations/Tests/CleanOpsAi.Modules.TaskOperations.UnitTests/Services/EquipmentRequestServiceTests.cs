using AutoMapper;
using CleanOpsAi.BuildingBlocks.Application;
using CleanOpsAi.BuildingBlocks.Application.Interfaces;
using CleanOpsAi.BuildingBlocks.Application.Pagination;
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
    public class EquipmentRequestServiceTests
    {
        private readonly IEquipmentRequestRepository _repo;
        private readonly IMapper _mapper;
        private readonly IUserContext _userContext;
        private readonly IDateTimeProvider _dateTime;
        private readonly IWorkerQueryService _workerService;

        private readonly EquipmentRequestService _service;

        public EquipmentRequestServiceTests()
        {
            _repo = Substitute.For<IEquipmentRequestRepository>();
            _mapper = Substitute.For<IMapper>();
            _userContext = Substitute.For<IUserContext>();
            _dateTime = Substitute.For<IDateTimeProvider>();
            _workerService = Substitute.For<IWorkerQueryService>();

            _service = new EquipmentRequestService(
                _repo,
                _mapper,
                _userContext,
                _dateTime,
                _workerService
            );
        }

        // =========================
        // GET BY ID
        // =========================
        [Fact]
        public async Task GetById_ShouldReturnDto_WithWorkerName()
        {
            var workerId = Guid.NewGuid();

            var entity = new EquipmentRequest
            {
                Id = Guid.NewGuid(),
                WorkerId = workerId
            };

            _repo.GetByIdExistAsync(entity.Id, default).Returns(entity);

            var dto = new EquipmentRequestDto
            {
                Id = entity.Id,
                WorkerId = workerId
            };

            _mapper.Map<EquipmentRequestDto>(entity).Returns(dto);

            _workerService.GetUserNames(Arg.Any<List<Guid>>())
                .Returns(new Dictionary<Guid, string>
                {
                    { workerId, "Worker A" }
                });

            var result = await _service.GetById(entity.Id);

            Assert.NotNull(result);
            Assert.Equal("Worker A", result.WorkerName);
        }

        // =========================
        // CREATE
        // =========================
        [Fact]
        public async Task Create_ShouldReturnDto()
        {
            var userId = Guid.NewGuid();
            _userContext.UserId.Returns(userId);
            _dateTime.UtcNow.Returns(DateTime.UtcNow);

            var workerId = Guid.NewGuid();

            var request = new CreateEquipmentRequestDto
            {
                WorkerId = workerId,
                EquipmentId = Guid.NewGuid(),
                Quantity = 5
            };

            var entity = new EquipmentRequest
            {
                Id = Guid.NewGuid(),
                WorkerId = workerId
            };

            _mapper.Map<EquipmentRequest>(request).Returns(entity);

            var dto = new EquipmentRequestDto
            {
                Id = entity.Id,
                WorkerId = workerId
            };

            _mapper.Map<EquipmentRequestDto>(entity).Returns(dto);

            _workerService.GetUserNames(Arg.Any<List<Guid>>())
                .Returns(new Dictionary<Guid, string>
                {
                    { workerId, "Worker A" }
                });

            var result = await _service.Create(request);

            Assert.NotNull(result);
            Assert.Equal("Worker A", result.WorkerName);

            await _repo.Received(1).AddAsync(entity, default);
        }

        // =========================
        // UPDATE
        // =========================
        [Fact]
        public async Task Update_ShouldUpdateEntity()
        {
            var workerId = Guid.NewGuid();

            var entity = new EquipmentRequest
            {
                Id = Guid.NewGuid(),
                WorkerId = workerId
            };

            _repo.GetByIdExistAsync(entity.Id, default).Returns(entity);

            _userContext.UserId.Returns(Guid.NewGuid());
            _dateTime.UtcNow.Returns(DateTime.UtcNow);

            var dtoRequest = new UpdateEquipmentRequestDto
            {
                Quantity = 10
            };

            _mapper.Map(dtoRequest, entity);

            _mapper.Map<EquipmentRequestDto>(entity)
                .Returns(new EquipmentRequestDto
                {
                    Id = entity.Id,
                    WorkerId = workerId
                });

            _workerService.GetUserNames(Arg.Any<List<Guid>>())
                .Returns(new Dictionary<Guid, string>());

            var result = await _service.Update(entity.Id, dtoRequest);

            Assert.NotNull(result);

            await _repo.Received(1).UpdateAsync(entity, default);
        }

        // =========================
        // REVIEW
        // =========================
        [Fact]
        public async Task Review_ShouldSetApproved()
        {
            var workerId = Guid.NewGuid();

            var entity = new EquipmentRequest
            {
                Id = Guid.NewGuid(),
                WorkerId = workerId
            };

            _repo.GetByIdExistAsync(entity.Id, default).Returns(entity);

            _userContext.UserId.Returns(Guid.NewGuid());
            _userContext.FullName.Returns("Manager A");
            _dateTime.UtcNow.Returns(DateTime.UtcNow);

            var reviewDto = new ReviewEquipmentRequestDto
            {
                Status = EquipmentRequestStatus.Approved
            };

            _mapper.Map<EquipmentRequestDto>(entity)
                .Returns(new EquipmentRequestDto
                {
                    Id = entity.Id,
                    WorkerId = workerId
                });

            _workerService.GetUserNames(Arg.Any<List<Guid>>())
                .Returns(new Dictionary<Guid, string>());

            var result = await _service.Review(entity.Id, reviewDto);

            Assert.NotNull(result);
            Assert.Equal(EquipmentRequestStatus.Approved, entity.Status);
            Assert.Equal("Manager A", result.ReviewedByUserName);

            await _repo.Received(1).UpdateAsync(entity, default);
        }

        // =========================
        // DELETE
        // =========================
        [Fact]
        public async Task Delete_ShouldReturnTrue()
        {
            var entity = new EquipmentRequest
            {
                Id = Guid.NewGuid()
            };

            _repo.GetByIdExistAsync(entity.Id, default).Returns(entity);

            var result = await _service.Delete(entity.Id);

            Assert.True(result);

            await _repo.Received(1).DeleteAsync(entity, default);
        }

        // =========================
        // GET LIST (PAGING + ENRICH)
        // =========================
        [Fact]
        public async Task Gets_ShouldReturnPagedResult()
        {
            var workerId = Guid.NewGuid();

            var entities = new List<EquipmentRequest>
    {
        new EquipmentRequest { Id = Guid.NewGuid(), WorkerId = workerId }
    };

            var paged = new PaginatedResult<EquipmentRequest>(
                1, 10, 1, entities
            );

            _repo.GetsPagingAsync(Arg.Any<PaginationRequest>(), default)
                .Returns(paged);

            // FIX CHỖ NÀY
            _mapper.Map<List<EquipmentRequestDto>>(Arg.Any<List<EquipmentRequest>>())
                .Returns(new List<EquipmentRequestDto>
                {
            new EquipmentRequestDto { WorkerId = workerId }
                });

            _workerService.GetUserNames(Arg.Any<List<Guid>>())
                .Returns(new Dictionary<Guid, string>
                {
            { workerId, "Worker A" }
                });

            var result = await _service.Gets(new PaginationRequest());

            Assert.NotNull(result);
            Assert.Single(result.Content);
            Assert.Equal("Worker A", result.Content[0].WorkerName);
        }
    }
}
