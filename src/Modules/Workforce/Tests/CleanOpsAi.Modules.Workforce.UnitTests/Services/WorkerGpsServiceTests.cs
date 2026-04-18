using CleanOpsAi.BuildingBlocks.Application;
using CleanOpsAi.BuildingBlocks.Application.Interfaces;
using CleanOpsAi.Modules.Workforce.Application.Dtos.WorkerGps;
using CleanOpsAi.Modules.Workforce.Application.Interfaces;
using CleanOpsAi.Modules.Workforce.Application.Services;
using CleanOpsAi.Modules.Workforce.Domain.Entities;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanOpsAi.Modules.Workforce.UnitTests.Services
{
    public class WorkerGpsServiceTests
    {
        private readonly IWorkerGpsRepository _repoMock;
        private readonly IWorkerRepository _workerRepoMock;
        private readonly IUserContext _userContextMock;
        private readonly IDateTimeProvider _dateTimeMock;

        private readonly WorkerGpsService _service;

        public WorkerGpsServiceTests()
        {
            _repoMock = Substitute.For<IWorkerGpsRepository>();
            _workerRepoMock = Substitute.For<IWorkerRepository>();
            _userContextMock = Substitute.For<IUserContext>();
            _dateTimeMock = Substitute.For<IDateTimeProvider>();

            _service = new WorkerGpsService(
                _repoMock,
                _workerRepoMock,
                _userContextMock,
                _dateTimeMock
            );
        }

        // ================================
        // GET BY ID
        // ================================
        [Fact]
        public async Task GetByIdAsync_ShouldReturnData()
        {
            var id = Guid.NewGuid();

            var entity = new WorkerGps
            {
                Id = id,
                WorkerId = Guid.NewGuid(),
                Worker = new Worker { FullName = "John" }
            };

            _repoMock.GetByIdAsync(id).Returns(entity);

            var result = await _service.GetByIdAsync(id);

            Assert.NotNull(result);
            Assert.Equal(id, result!.Id);
            Assert.Equal("John", result.WorkerName);
        }

        // ================================
        // GET ALL
        // ================================
        [Fact]
        public async Task GetAllAsync_ShouldReturnList()
        {
            var data = new List<WorkerGps>
            {
                new WorkerGps { Id = Guid.NewGuid() },
                new WorkerGps { Id = Guid.NewGuid() }
            };

            _repoMock.GetAllAsync().Returns(data);

            var result = await _service.GetAllAsync();

            Assert.Equal(2, result.Count);
        }

        // ================================
        // PAGINATION
        // ================================
        [Fact]
        public async Task GetAllPaginationAsync_ShouldReturnPaged()
        {
            var data = new List<WorkerGps>
            {
                new WorkerGps { Id = Guid.NewGuid() }
            };

            _repoMock.GetAllPaginationAsync(1, 10)
                     .Returns((data, 1));

            var result = await _service.GetAllPaginationAsync(1, 10);

            Assert.Single(result.Content);
            Assert.Equal(1, result.TotalElements);
        }

        // ================================
        // CREATE SUCCESS
        // ================================
        [Fact]
        public async Task CreateAsync_ShouldCreateSuccessfully()
        {
            var workerId = Guid.NewGuid();
            var newId = Guid.NewGuid();

            var request = new WorkerGpsCreateRequest
            {
                WorkerId = workerId,
                Latitude = 10,
                Longitude = 20,
                IsConfirmed = true
            };

            var worker = new Worker { Id = workerId };

            _workerRepoMock.GetByIdAsync(workerId).Returns(worker);

            _userContextMock.UserId.Returns(Guid.NewGuid());
            _dateTimeMock.UtcNow.Returns(DateTime.UtcNow);

            _repoMock.CreateAsync(Arg.Any<WorkerGps>()).Returns(1);

            _repoMock.GetByIdAsync(Arg.Any<Guid>())
                .Returns(callInfo =>
                {
                    var id = callInfo.Arg<Guid>();
                    return new WorkerGps
                    {
                        Id = id,
                        WorkerId = workerId,
                        Worker = new Worker { FullName = "John" }
                    };
                });

            var result = await _service.CreateAsync(request);

            Assert.NotNull(result);
            Assert.Equal(workerId, result!.WorkerId);

            await _repoMock.Received(1).CreateAsync(Arg.Any<WorkerGps>());
        }

        // ================================
        // CREATE - WORKER NOT FOUND
        // ================================
        [Fact]
        public async Task CreateAsync_ShouldThrow_WhenWorkerNotFound()
        {
            var request = new WorkerGpsCreateRequest
            {
                WorkerId = Guid.NewGuid()
            };

            _workerRepoMock.GetByIdAsync(request.WorkerId)
                           .Returns((Worker?)null);

            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _service.CreateAsync(request));
        }

        // ================================
        // GET LATEST BY WORKER
        // ================================
        [Fact]
        public async Task GetLatestByWorkerIdAsync_ShouldReturnData()
        {
            var workerId = Guid.NewGuid();

            var entity = new WorkerGps
            {
                WorkerId = workerId,
                Worker = new Worker { FullName = "Latest" }
            };

            _repoMock.GetLatestByWorkerIdAsync(workerId).Returns(entity);

            var result = await _service.GetLatestByWorkerIdAsync(workerId);

            Assert.NotNull(result);
            Assert.Equal("Latest", result!.WorkerName);
        }

        // ================================
        // GET LATEST BY MULTIPLE WORKERS
        // ================================
        [Fact]
        public async Task GetLatestByWorkerIdsAsync_ShouldReturnList()
        {
            var data = new List<WorkerGps>
            {
                new WorkerGps { Worker = new Worker { FullName = "A" } }
            };

            _repoMock.GetLatestByWorkerIdsAsync(Arg.Any<List<Guid>>())
                     .Returns(data);

            var result = await _service.GetLatestByWorkerIdsAsync(new List<Guid> { Guid.NewGuid() });

            Assert.Single(result);
        }

        // ================================
        // PAGINATION BY WORKER
        // ================================
        [Fact]
        public async Task GetByWorkerIdPaginationAsync_ShouldReturnPaged()
        {
            var data = new List<WorkerGps>
            {
                new WorkerGps { Id = Guid.NewGuid() }
            };

            _repoMock.GetByWorkerIdPaginationAsync(Arg.Any<Guid>(), 1, 10)
                     .Returns((data, 1));

            var result = await _service.GetByWorkerIdPaginationAsync(Guid.NewGuid(), 1, 10);

            Assert.Single(result.Content);
            Assert.Equal(1, result.TotalElements);
        }
    }
}
