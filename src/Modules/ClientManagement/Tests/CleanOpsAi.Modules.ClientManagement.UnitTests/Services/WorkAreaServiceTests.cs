using CleanOpsAi.BuildingBlocks.Application;
using CleanOpsAi.BuildingBlocks.Application.Exceptions;
using CleanOpsAi.BuildingBlocks.Application.Interfaces;
using CleanOpsAi.Modules.ClientManagement.Application.Dtos.Workareas;
using CleanOpsAi.Modules.ClientManagement.Application.Interfaces;
using CleanOpsAi.Modules.ClientManagement.Application.Services;
using CleanOpsAi.Modules.ClientManagement.Domain.Entities;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanOpsAi.Modules.ClientManagement.UnitTests.Services
{
    public class WorkAreaServiceTests
    {
        private readonly IWorkAreaRepository _repoMock = Substitute.For<IWorkAreaRepository>();
        private readonly IUserContext _userContextMock = Substitute.For<IUserContext>();
        private readonly IDateTimeProvider _dateTimeMock = Substitute.For<IDateTimeProvider>();
        private readonly IIdGenerator _idGenMock = Substitute.For<IIdGenerator>();

        private readonly WorkAreaService _service;

        public WorkAreaServiceTests()
        {
            _service = new WorkAreaService(
                _repoMock,
                _userContextMock,
                _dateTimeMock,
                _idGenMock
            );
        }

        // ================================
        // CREATE
        // ================================
        [Fact]
        public async Task CreateAsync_ShouldCreateWorkArea()
        {
            var newId = Guid.NewGuid();

            var request = new WorkAreaCreateRequest
            {
                Name = "Area A",
                ZoneId = Guid.NewGuid()
            };

            _idGenMock.Generate().Returns(newId);
            _userContextMock.UserId.Returns(Guid.NewGuid());
            _dateTimeMock.UtcNow.Returns(DateTime.UtcNow);

            _repoMock.CreateAsync(Arg.Any<WorkArea>()).Returns(call => Task.FromResult(1));

            var result = await _service.CreateAsync(request);

            Assert.NotNull(result);
            Assert.Equal(newId, result.Id);
            Assert.Equal("Area A", result.Name);

            await _repoMock.Received(1).CreateAsync(Arg.Any<WorkArea>());
        }

        // ================================
        // GET BY ID
        // ================================
        [Fact]
        public async Task GetByIdAsync_ShouldReturnWorkArea()
        {
            var id = Guid.NewGuid();

            var entity = new WorkArea
            {
                Id = id,
                Name = "Area A",
                ZoneId = Guid.NewGuid(),
                Zone = new Zone { Name = "Zone A" }
            };

            _repoMock.GetByIdAsync(id).Returns(entity);

            var result = await _service.GetByIdAsync(id);

            Assert.NotNull(result);
            Assert.Equal(id, result.Id);
            Assert.Equal("Zone A", result.ZoneName);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnNull_WhenNotFound()
        {
            var id = Guid.NewGuid();

            _repoMock.GetByIdAsync(id).Returns((WorkArea)null);

            var result = await _service.GetByIdAsync(id);

            Assert.Null(result);
        }

        // ================================
        // GET ALL
        // ================================
        [Fact]
        public async Task GetAllAsync_ShouldReturnList()
        {
            var list = new List<WorkArea>
            {
                new WorkArea { Id = Guid.NewGuid(), Name = "A" },
                new WorkArea { Id = Guid.NewGuid(), Name = "B" }
            };

            _repoMock.GetAllAsync().Returns(list);

            var result = await _service.GetAllAsync();

            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
        }

        // ================================
        // PAGINATION
        // ================================
        [Fact]
        public async Task GetAllPaginationAsync_ShouldReturnPagedResult()
        {
            var list = new List<WorkArea>
            {
                new WorkArea { Id = Guid.NewGuid(), Name = "A" }
            };

            _repoMock.GetAllPaginationAsync(1, 10).Returns((list, 1));

            var result = await _service.GetAllPaginationAsync(1, 10);

            Assert.NotNull(result);
            Assert.Single(result.Content);
            Assert.Equal(1, result.TotalElements);
        }

        [Fact]
        public async Task GetByZoneIdPaginationAsync_ShouldReturnPagedResult()
        {
            var zoneId = Guid.NewGuid();

            var list = new List<WorkArea>
            {
                new WorkArea { Id = Guid.NewGuid(), ZoneId = zoneId }
            };

            _repoMock.GetByZoneIdPaginationAsync(zoneId, 1, 10)
                     .Returns((list, 1));

            var result = await _service.GetByZoneIdPaginationAsync(zoneId, 1, 10);

            Assert.NotNull(result);
            Assert.Single(result.Content);
        }

        // ================================
        // UPDATE
        // ================================
        [Fact]
        public async Task UpdateAsync_ShouldUpdateWorkArea()
        {
            var id = Guid.NewGuid();

            var entity = new WorkArea
            {
                Id = id,
                Name = "Old"
            };

            var request = new WorkAreaUpdateRequest
            {
                Name = "New"
            };

            _repoMock.GetByIdAsync(id).Returns(entity);
            _repoMock.UpdateAsync(entity).Returns(1);

            _userContextMock.UserId.Returns(Guid.NewGuid());
            _dateTimeMock.UtcNow.Returns(DateTime.UtcNow);

            var result = await _service.UpdateAsync(id, request);

            Assert.NotNull(result);
            Assert.Equal("New", result.Name);

            await _repoMock.Received(1).UpdateAsync(entity);
        }

        [Fact]
        public async Task UpdateAsync_ShouldThrow_WhenNotFound()
        {
            var id = Guid.NewGuid();

            _repoMock.GetByIdAsync(id).Returns((WorkArea)null);

            await Assert.ThrowsAsync<NotFoundException>(() =>
                _service.UpdateAsync(id, new WorkAreaUpdateRequest()));
        }

        // ================================
        // DELETE
        // ================================
        [Fact]
        public async Task DeleteAsync_ShouldDeleteWorkArea()
        {
            var id = Guid.NewGuid();

            _repoMock.GetByIdAsync(id).Returns(new WorkArea { Id = id });
            _repoMock.DeleteAsync(id).Returns(1);

            var result = await _service.DeleteAsync(id);

            Assert.Equal(1, result);
            await _repoMock.Received(1).DeleteAsync(id);
        }

        [Fact]
        public async Task DeleteAsync_ShouldThrow_WhenNotFound()
        {
            var id = Guid.NewGuid();

            _repoMock.GetByIdAsync(id).Returns((WorkArea)null);

            await Assert.ThrowsAsync<NotFoundException>(() =>
                _service.DeleteAsync(id));
        }
    }
}
