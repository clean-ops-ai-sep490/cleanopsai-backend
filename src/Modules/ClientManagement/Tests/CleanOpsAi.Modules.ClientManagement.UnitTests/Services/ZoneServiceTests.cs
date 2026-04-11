using CleanOpsAi.BuildingBlocks.Application;
using CleanOpsAi.BuildingBlocks.Application.Interfaces;
using CleanOpsAi.Modules.ClientManagement.Application.Dtos.Zones;
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
    public class ZoneServiceTests
    {
        private readonly IZoneRepository _repoMock = Substitute.For<IZoneRepository>();
        private readonly IUserContext _userContextMock = Substitute.For<IUserContext>();
        private readonly IDateTimeProvider _dateTimeMock = Substitute.For<IDateTimeProvider>();
        private readonly IIdGenerator _idGenMock = Substitute.For<IIdGenerator>();

        private readonly ZoneService _service;

        public ZoneServiceTests()
        {
            _service = new ZoneService(
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
        public async Task CreateAsync_ShouldCreateZone()
        {
            var newId = Guid.NewGuid();

            var request = new ZoneCreateRequest
            {
                Name = "Zone A",
                Description = "Desc",
                LocationId = Guid.NewGuid()
            };

            _idGenMock.Generate().Returns(newId);
            _userContextMock.UserId.Returns(Guid.NewGuid());
            _dateTimeMock.UtcNow.Returns(DateTime.UtcNow);

            //  repo trả object → KHÔNG dùng Returns(1)
            _repoMock.CreateAsync(Arg.Any<Zone>())
                     .Returns(call => Task.FromResult(1));

            var result = await _service.CreateAsync(request);

            Assert.NotNull(result);
            Assert.Equal(newId, result.Id);
            Assert.Equal("Zone A", result.Name);

            await _repoMock.Received(1).CreateAsync(Arg.Any<Zone>());
        }

        // ================================
        // GET BY ID
        // ================================
        [Fact]
        public async Task GetByIdAsync_ShouldReturnZone()
        {
            var id = Guid.NewGuid();

            var entity = new Zone
            {
                Id = id,
                Name = "Zone A",
                LocationId = Guid.NewGuid(),
                Location = new Location { Name = "Location A" }
            };

            _repoMock.GetByIdAsync(id).Returns(entity);

            var result = await _service.GetByIdAsync(id);

            Assert.NotNull(result);
            Assert.Equal(id, result.Id);
            Assert.Equal("Location A", result.LocationName);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnNull_WhenNotFound()
        {
            var id = Guid.NewGuid();

            _repoMock.GetByIdAsync(id).Returns((Zone)null);

            var result = await _service.GetByIdAsync(id);

            Assert.Null(result);
        }

        // ================================
        // GET ALL
        // ================================
        [Fact]
        public async Task GetAllAsync_ShouldReturnList()
        {
            var list = new List<Zone>
            {
                new Zone { Id = Guid.NewGuid(), Name = "A" },
                new Zone { Id = Guid.NewGuid(), Name = "B" }
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
        public async Task GetAllPaginationAsync_ShouldReturnPaged()
        {
            var list = new List<Zone>
            {
                new Zone { Id = Guid.NewGuid(), Name = "A" }
            };

            _repoMock.GetAllPaginationAsync(1, 10).Returns((list, 1));

            var result = await _service.GetAllPaginationAsync(1, 10);

            Assert.NotNull(result);
            Assert.Single(result.Content);
            Assert.Equal(1, result.TotalElements);
        }

        [Fact]
        public async Task GetByLocationIdPaginationAsync_ShouldReturnPaged()
        {
            var locationId = Guid.NewGuid();

            var list = new List<Zone>
            {
                new Zone { Id = Guid.NewGuid(), LocationId = locationId }
            };

            _repoMock.GetByLocationIdPaginationAsync(locationId, 1, 10)
                     .Returns((list, 1));

            var result = await _service.GetByLocationIdPaginationAsync(locationId, 1, 10);

            Assert.NotNull(result);
            Assert.Single(result.Content);
        }

        // ================================
        // UPDATE
        // ================================
        [Fact]
        public async Task UpdateAsync_ShouldUpdateZone()
        {
            var id = Guid.NewGuid();

            var entity = new Zone
            {
                Id = id,
                Name = "Old",
                Description = "Old Desc"
            };

            var request = new ZoneUpdateRequest
            {
                Name = "New",
                Description = "New Desc"
            };

            _repoMock.GetByIdAsync(id).Returns(entity);
            _repoMock.UpdateAsync(entity).Returns(1);

            _userContextMock.UserId.Returns(Guid.NewGuid());
            _dateTimeMock.UtcNow.Returns(DateTime.UtcNow);

            var result = await _service.UpdateAsync(id, request);

            Assert.NotNull(result);
            Assert.Equal("New", result.Name);
            Assert.Equal("New Desc", result.Description);

            await _repoMock.Received(1).UpdateAsync(entity);
        }

        [Fact]
        public async Task UpdateAsync_ShouldThrow_WhenNotFound()
        {
            var id = Guid.NewGuid();

            _repoMock.GetByIdAsync(id).Returns((Zone)null);

            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _service.UpdateAsync(id, new ZoneUpdateRequest()));
        }

        // ================================
        // DELETE
        // ================================
        [Fact]
        public async Task DeleteAsync_ShouldDeleteZone()
        {
            var id = Guid.NewGuid();

            _repoMock.GetByIdAsync(id).Returns(new Zone { Id = id });
            _repoMock.DeleteAsync(id).Returns(1);

            var result = await _service.DeleteAsync(id);

            Assert.Equal(1, result);
            await _repoMock.Received(1).DeleteAsync(id);
        }

        [Fact]
        public async Task DeleteAsync_ShouldThrow_WhenNotFound()
        {
            var id = Guid.NewGuid();

            _repoMock.GetByIdAsync(id).Returns((Zone)null);

            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _service.DeleteAsync(id));
        }
    }
}
