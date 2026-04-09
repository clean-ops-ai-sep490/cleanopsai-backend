using CleanOpsAi.BuildingBlocks.Application;
using CleanOpsAi.BuildingBlocks.Application.Exceptions;
using CleanOpsAi.BuildingBlocks.Application.Interfaces;
using CleanOpsAi.Modules.ClientManagement.Application.Dtos.Locations;
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
    public class LocationServiceTests
    {
        private readonly ILocationRepository _repoMock;
        private readonly IUserContext _userContextMock;
        private readonly IDateTimeProvider _dateTimeMock;
        private readonly IIdGenerator _idGenMock;

        private readonly LocationService _service;

        public LocationServiceTests()
        {
            _repoMock = Substitute.For<ILocationRepository>();
            _userContextMock = Substitute.For<IUserContext>();
            _dateTimeMock = Substitute.For<IDateTimeProvider>();
            _idGenMock = Substitute.For<IIdGenerator>();

            _service = new LocationService(
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
        public async Task CreateAsync_ShouldReturnLocation()
        {
            var request = new LocationCreateRequest
            {
                Name = "Location A",
                Address = "HCM",
                Latitude = 10.1,
                Longitude = 106.1,
                ClientId = Guid.NewGuid()
            };

            var newId = Guid.NewGuid();

            _idGenMock.Generate().Returns(newId);
            _userContextMock.UserId.Returns(Guid.NewGuid());
            _dateTimeMock.UtcNow.Returns(DateTime.UtcNow);

            _repoMock.CreateAsync(Arg.Any<Location>()).Returns(1);

            var result = await _service.CreateAsync(request);

            Assert.NotNull(result);
            Assert.Equal(newId, result.Id);
            Assert.Equal("Location A", result.Name);

            await _repoMock.Received(1).CreateAsync(Arg.Any<Location>());
        }

        // ================================
        // GET BY ID
        // ================================
        [Fact]
        public async Task GetByIdAsync_ShouldReturnLocation()
        {
            var id = Guid.NewGuid();

            var location = new Location
            {
                Id = id,
                Name = "Test",
                Address = "HCM"
            };

            _repoMock.GetByIdAsync(id).Returns(location);

            var result = await _service.GetByIdAsync(id);

            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal(id, result[0].Id);
        }

        // ================================
        // GET BY ID - NOT FOUND
        // ================================
        [Fact]
        public async Task GetByIdAsync_ShouldReturnNull_WhenNotFound()
        {
            var id = Guid.NewGuid();

            _repoMock.GetByIdAsync(id).Returns((Location)null);

            var result = await _service.GetByIdAsync(id);

            Assert.Null(result);
        }

        // ================================
        // UPDATE
        // ================================
        [Fact]
        public async Task UpdateAsync_ShouldUpdateLocation()
        {
            var id = Guid.NewGuid();

            var location = new Location
            {
                Id = id,
                Name = "Old Name",
                Address = "Old Address",
                Latitude = 1,
                Longitude = 1
            };

            var request = new LocationUpdateRequest
            {
                Name = "New Name",
                Address = "New Address",
                Latitude = 10,
                Longitude = 20
            };

            _repoMock.GetByIdAsync(id).Returns(location);
            _repoMock.UpdateAsync(location).Returns(1);

            _userContextMock.UserId.Returns(Guid.NewGuid());
            _dateTimeMock.UtcNow.Returns(DateTime.UtcNow);

            var result = await _service.UpdateAsync(id, request);

            Assert.NotNull(result);
            Assert.Equal("New Name", result.Name);
            Assert.Equal("New Address", result.Address);
            Assert.Equal(10, result.Latitude);
            Assert.Equal(20, result.Longitude);

            await _repoMock.Received(1).UpdateAsync(location);
        }

        // ================================
        // UPDATE - NOT FOUND
        // ================================
        [Fact]
        public async Task UpdateAsync_ShouldThrowException_WhenNotFound()
        {
            var id = Guid.NewGuid();

            _repoMock.GetByIdAsync(id).Returns((Location)null);

            var request = new LocationUpdateRequest();

            await Assert.ThrowsAsync<NotFoundException>(() =>
                _service.UpdateAsync(id, request));
        }

        // ================================
        // DELETE
        // ================================
        [Fact]
        public async Task DeleteAsync_ShouldCallRepository()
        {
            var id = Guid.NewGuid();

            var location = new Location { Id = id };

            _repoMock.GetByIdAsync(id).Returns(location);
            _repoMock.DeleteAsync(id).Returns(1);

            var result = await _service.DeleteAsync(id);

            Assert.Equal(1, result);
            await _repoMock.Received(1).DeleteAsync(id);
        }

        // ================================
        // DELETE - NOT FOUND
        // ================================
        [Fact]
        public async Task DeleteAsync_ShouldThrowException_WhenNotFound()
        {
            var id = Guid.NewGuid();

            _repoMock.GetByIdAsync(id).Returns((Location)null);

            await Assert.ThrowsAsync<NotFoundException>(() =>
                _service.DeleteAsync(id));
        }

        // ================================
        // GET ALL
        // ================================
        [Fact]
        public async Task GetAllAsync_ShouldReturnList()
        {
            var locations = new List<Location>
            {
                new Location { Id = Guid.NewGuid(), Name = "A" },
                new Location { Id = Guid.NewGuid(), Name = "B" }
            };

            _repoMock.GetAllAsync().Returns(locations);

            var result = await _service.GetAllAsync();

            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
        }

        // ================================
        // GET BY CLIENT ID
        // ================================
        [Fact]
        public async Task GetByClientIdAsync_ShouldReturnList()
        {
            var clientId = Guid.NewGuid();

            var locations = new List<Location>
            {
                new Location { Id = Guid.NewGuid(), ClientId = clientId },
                new Location { Id = Guid.NewGuid(), ClientId = clientId }
            };

            _repoMock.GetByClientIdAsync(clientId).Returns(locations);

            var result = await _service.GetByClientIdAsync(clientId);

            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
        }
    }
}
