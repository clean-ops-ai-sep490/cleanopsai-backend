using CleanOpsAi.BuildingBlocks.Application;
using CleanOpsAi.BuildingBlocks.Application.Interfaces;
using CleanOpsAi.Modules.Workforce.Application.Dtos.WorkAreaSupervisors;
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
    public class WorkAreaSupervisorServiceTests
    {
        private readonly IWorkAreaSupervisorRepository _repoMock;
        private readonly IUserContext _userContextMock;
        private readonly IDateTimeProvider _dateTimeMock;

        private readonly WorkAreaSupervisorService _service;

        public WorkAreaSupervisorServiceTests()
        {
            _repoMock = Substitute.For<IWorkAreaSupervisorRepository>();
            _userContextMock = Substitute.For<IUserContext>();
            _dateTimeMock = Substitute.For<IDateTimeProvider>();

            _service = new WorkAreaSupervisorService(
                _repoMock,
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

            var entity = new WorkAreaSupervisor
            {
                Id = id,
                WorkAreaId = Guid.NewGuid(),
                WorkerId = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
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
            var data = new List<WorkAreaSupervisor>
            {
                new WorkAreaSupervisor { Id = Guid.NewGuid() },
                new WorkAreaSupervisor { Id = Guid.NewGuid() }
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
            var data = new List<WorkAreaSupervisor>
            {
                new WorkAreaSupervisor { Id = Guid.NewGuid() }
            };

            _repoMock.GetAllPaginationAsync(1, 10)
                     .Returns((data, 1));

            var result = await _service.GetAllPaginationAsync(1, 10);

            Assert.Single(result.Content);
            Assert.Equal(1, result.TotalElements);
        }

        // ================================
        // UPDATE (REPLACE ALL)
        // ================================
        [Fact]
        public async Task UpdateAsync_ShouldReplaceAssignments()
        {
            var request = new WorkAreaSupervisorUpdateRequest
            {
                WorkAreaId = Guid.NewGuid(),
                SupervisorId = Guid.NewGuid(),
                WorkerIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() }
            };

            _userContextMock.UserId.Returns(Guid.NewGuid());
            _dateTimeMock.UtcNow.Returns(DateTime.UtcNow);

            _repoMock.GetByWorkAreaIdAsync(request.WorkAreaId)
                     .Returns(new List<WorkAreaSupervisor>());

            var result = await _service.UpdateAsync(request);

            await _repoMock.Received(1)
                .DeleteByWorkAreaAndSupervisorAsync(request.WorkAreaId, request.SupervisorId);

            await _repoMock.Received(1)
                .CreateRangeAsync(Arg.Is<List<WorkAreaSupervisor>>(x => x.Count == 2));

            Assert.Equal(0, result.TotalAssigned); // vì repo mock trả empty
        }

        // ================================
        // UPDATE - EMPTY LIST
        // ================================
        [Fact]
        public async Task UpdateAsync_ShouldThrow_WhenWorkerIdsEmpty()
        {
            var request = new WorkAreaSupervisorUpdateRequest
            {
                WorkerIds = new List<Guid>()
            };

            await Assert.ThrowsAsync<ArgumentException>(() =>
                _service.UpdateAsync(request));
        }

        // ================================
        // ASSIGN WORKERS (NO DUPLICATE)
        // ================================
        [Fact]
        public async Task AssignWorkersAsync_ShouldSkipExisting()
        {
            var workerId = Guid.NewGuid();

            var request = new WorkAreaSupervisorAssignRequest
            {
                WorkAreaId = Guid.NewGuid(),
                SupervisorId = Guid.NewGuid(),
                WorkerIds = new List<Guid> { workerId }
            };

            _repoMock.ExistsAsync(request.WorkAreaId, request.SupervisorId, workerId)
                     .Returns(true); // already exists

            _repoMock.GetByWorkAreaIdAsync(request.WorkAreaId)
                     .Returns(new List<WorkAreaSupervisor>());

            var result = await _service.AssignWorkersAsync(request);

            await _repoMock.DidNotReceive().CreateRangeAsync(Arg.Any<List<WorkAreaSupervisor>>());
        }

        // ================================
        // UNASSIGN
        // ================================
        [Fact]
        public async Task UnassignWorkerAsync_ShouldDelete()
        {
            var entity = new WorkAreaSupervisor
            {
                Id = Guid.NewGuid()
            };

            _repoMock.GetByWorkAreaUserWorkerAsync(
                Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<Guid>())
                .Returns(entity);

            _repoMock.DeleteAsync(entity.Id).Returns(1);

            var result = await _service.UnassignWorkerAsync(
                Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());

            Assert.Equal(1, result);
        }

        // ================================
        // UNASSIGN - NOT FOUND
        // ================================
        [Fact]
        public async Task UnassignWorkerAsync_ShouldThrow_WhenNotFound()
        {
            _repoMock.GetByWorkAreaUserWorkerAsync(
                Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<Guid>())
                .Returns((WorkAreaSupervisor?)null);

            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _service.UnassignWorkerAsync(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()));
        }

        // ================================
        // GPS
        // ================================
        [Fact]
        public async Task GetWorkersLatestGpsByWorkAreaIdAsync_ShouldReturnList()
        {
            var data = new List<WorkerGps>
            {
                new WorkerGps
                {
                    WorkerId = Guid.NewGuid(),
                    Worker = new Worker { FullName = "GPS User" },
                    Latitude = 10,
                    Longitude = 20
                }
            };

            _repoMock.GetWorkersLatestGpsByWorkAreaIdAsync(Arg.Any<Guid>())
                     .Returns(data);

            var result = await _service.GetWorkersLatestGpsByWorkAreaIdAsync(Guid.NewGuid());

            Assert.Single(result);
            Assert.Equal("GPS User", result[0].WorkerName);
        }

        // ================================
        // COMMON SUPERVISOR
        // ================================
        [Fact]
        public async Task GetCommonSupervisorAsync_ShouldReturnTrue_WhenExists()
        {
            var supId = Guid.NewGuid();

            _repoMock.GetSupervisorIdsAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>())
                .Returns(new List<Guid> { supId });

            var result = await _service.GetCommonSupervisorAsync(
                Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());

            Assert.True(result.Found);
            Assert.Equal(supId, result.SupervisorUserId);
        }

        // ================================
        // COMMON SUPERVISOR - NOT FOUND
        // ================================
        [Fact]
        public async Task GetCommonSupervisorAsync_ShouldReturnFalse_WhenNone()
        {
            _repoMock.GetSupervisorIdsAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>())
                .Returns(new List<Guid>());

            var result = await _service.GetCommonSupervisorAsync(
                Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());

            Assert.False(result.Found);
            Assert.Null(result.SupervisorUserId);
        }
    }
}
