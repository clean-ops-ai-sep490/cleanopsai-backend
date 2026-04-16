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
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace CleanOpsAi.Modules.TaskOperations.UnitTests.Services
{
    public class EquipmentRequestServiceTests
    {
        private readonly IEquipmentRequestRepository _repo;
        private readonly IMapper _mapper;
        private readonly IUserContext _userContext;
        private readonly IDateTimeProvider _dateTime;
        private readonly IWorkerQueryService _workerService;
        private readonly IEquipmentQueryService _equipmentService;

        private readonly EquipmentRequestService _service;

        public EquipmentRequestServiceTests()
        {
            _repo = Substitute.For<IEquipmentRequestRepository>();
            _mapper = Substitute.For<IMapper>();
            _userContext = Substitute.For<IUserContext>();
            _dateTime = Substitute.For<IDateTimeProvider>();
            _workerService = Substitute.For<IWorkerQueryService>();
            _equipmentService = Substitute.For<IEquipmentQueryService>();

            _service = new EquipmentRequestService(
                _repo,
                _mapper,
                _userContext,
                _dateTime,
                _workerService,
                _equipmentService
            );
        }

        // ================= GET BY ID =================
        [Fact]
        public async Task GetById_ShouldReturnDto()
        {
            var id = Guid.NewGuid();
            var workerId = Guid.NewGuid();

            var entity = new EquipmentRequest
            {
                Id = id,
                WorkerId = workerId,
                Items = new List<EquipmentRequestItem>()
            };

            _repo.GetByIdAsync(id, default).Returns(entity);

            var dto = new EquipmentRequestDto
            {
                Id = id,
                WorkerId = workerId,
                Items = new List<EquipmentRequestItemDto>()
            };

            _mapper.Map<EquipmentRequestDto>(entity).Returns(dto);

            _workerService.GetUserNames(Arg.Any<List<Guid>>())
                .Returns(new Dictionary<Guid, string> { { workerId, "Worker A" } });

            _equipmentService.GetNamesAsync(Arg.Any<List<Guid>>(), default)
                .Returns(new Dictionary<Guid, string>());

            var result = await _service.GetById(id);

            Assert.NotNull(result);
            Assert.Equal("Worker A", result.WorkerName);
        }

        // ================= CREATE =================
        [Fact]
        public async Task CreateBatch_ShouldReturnDto()
        {
            var workerId = Guid.NewGuid();
            var equipmentId = Guid.NewGuid();

            _userContext.UserId.Returns(Guid.NewGuid());
            _dateTime.UtcNow.Returns(DateTime.UtcNow);

            var request = new CreateEquipmentRequestBatchDto
            {
                WorkerId = workerId,
                TaskAssignmentId = Guid.NewGuid(),
                Items = new List<CreateEquipmentRequestItemDto>
                {
                    new CreateEquipmentRequestItemDto
                    {
                        EquipmentId = equipmentId,
                        Quantity = 2
                    }
                }
            };

            var entity = new EquipmentRequest
            {
                Id = Guid.NewGuid(),
                WorkerId = workerId,
                Items = new List<EquipmentRequestItem>()
            };

            _repo.AddAsync(Arg.Any<EquipmentRequest>(), default)
                .Returns(Task.CompletedTask);

            _mapper.Map<EquipmentRequestDto>(Arg.Any<EquipmentRequest>())
                .Returns(new EquipmentRequestDto
                {
                    WorkerId = workerId,
                    Items = new List<EquipmentRequestItemDto>()
                });

            _workerService.GetUserNames(Arg.Any<List<Guid>>())
                .Returns(new Dictionary<Guid, string> { { workerId, "Worker A" } });

            _equipmentService.GetNamesAsync(Arg.Any<List<Guid>>(), default)
                .Returns(new Dictionary<Guid, string>());

            var result = await _service.CreateBatch(request);

            Assert.NotNull(result);
        }

        // ================= UPDATE =================
        [Fact]
        public async Task Update_ShouldReturnDto()
        {
            var id = Guid.NewGuid();

            var entity = new EquipmentRequest
            {
                Id = id,
                WorkerId = Guid.NewGuid(),
                Items = new List<EquipmentRequestItem>()
            };

            _repo.GetByIdAsync(id, default).Returns(entity);

            _mapper.Map<EquipmentRequestDto>(Arg.Any<EquipmentRequest>())
                .Returns(new EquipmentRequestDto
                {
                    Id = id,
                    Items = new List<EquipmentRequestItemDto>()
                });

            _workerService.GetUserNames(Arg.Any<List<Guid>>())
                .Returns(new Dictionary<Guid, string>());

            _equipmentService.GetNamesAsync(Arg.Any<List<Guid>>(), default)
                .Returns(new Dictionary<Guid, string>());

            var dto = new UpdateEquipmentRequestDto
            {
                Items = new List<CreateEquipmentRequestItemDto>()
            };

            var result = await _service.Update(id, dto);

            Assert.NotNull(result);
            await _repo.Received(1).UpdateAsync(entity, default);
        }

        // ================= DELETE =================
        [Fact]
        public async Task Delete_ShouldReturnTrue()
        {
            var id = Guid.NewGuid();

            var entity = new EquipmentRequest
            {
                Id = id
            };

            _repo.GetByIdAsync(id, default).Returns(entity);

            var result = await _service.Delete(id);

            Assert.True(result);
            await _repo.Received(1).DeleteAsync(entity, default);
        }
    }
}