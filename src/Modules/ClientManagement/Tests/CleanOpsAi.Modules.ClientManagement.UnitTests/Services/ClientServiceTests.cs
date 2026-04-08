using CleanOpsAi.BuildingBlocks.Application;
using CleanOpsAi.BuildingBlocks.Application.Interfaces;
using CleanOpsAi.Modules.ClientManagement.Application.Dtos.Clients;
using CleanOpsAi.Modules.ClientManagement.Application.Interfaces;
using CleanOpsAi.Modules.ClientManagement.Application.Services;
using CleanOpsAi.Modules.ClientManagement.Domain.Entities;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace CleanOpsAi.Modules.ClientManagement.UnitTests.Services
{
    public class ClientServiceTests
    {
        private readonly IClientRepository _repoMock;
        private readonly IUserContext _userContextMock;
        private readonly IDateTimeProvider _dateTimeMock;
        private readonly IIdGenerator _idGenMock;

        private readonly ClientService _service;

        public ClientServiceTests()
        {
            _repoMock = Substitute.For<IClientRepository>();
            _userContextMock = Substitute.For<IUserContext>();
            _dateTimeMock = Substitute.For<IDateTimeProvider>();
            _idGenMock = Substitute.For<IIdGenerator>();

            _service = new ClientService(
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
        public async Task CreateAsync_ShouldReturnClientResponse()
        {
            var request = new ClientCreateRequest
            {
                Name = "Test",
                Email = "test@gmail.com"
            };

            var newId = Guid.NewGuid();

            _idGenMock.Generate().Returns(newId);
            _userContextMock.UserId.Returns(Guid.NewGuid());
            _dateTimeMock.UtcNow.Returns(DateTime.UtcNow);

            _repoMock.CreateAsync(Arg.Any<Client>())
                     .Returns(1);

            var result = await _service.CreateAsync(request);

            Assert.NotNull(result);
            Assert.Equal(newId, result.Id);
            Assert.Equal(request.Name, result.Name);
            Assert.Equal(request.Email, result.Email);

            await _repoMock.Received(1).CreateAsync(Arg.Any<Client>());
        }

        // ================================
        // GET BY ID
        // ================================
        [Fact]
        public async Task GetByIdAsync_ShouldReturnClient()
        {
            var id = Guid.NewGuid();

            var client = new Client
            {
                Id = id,
                Name = "Test",
                Email = "test@gmail.com"
            };

            _repoMock.GetByIdAsync(id)
                     .Returns(client);

            var result = await _service.GetByIdAsync(id);

            Assert.Single(result);
            Assert.Equal(id, result[0].Id);
        }

        // ================================
        // UPDATE
        // ================================
        [Fact]
        public async Task UpdateAsync_ShouldUpdateClient()
        {
            var id = Guid.NewGuid();

            var client = new Client
            {
                Id = id,
                Name = "Old",
                Email = "old@gmail.com"
            };

            var request = new ClientUpdateRequest
            {
                Name = "New",
                Email = "new@gmail.com"
            };

            _repoMock.GetByIdAsync(id).Returns(client);
            _repoMock.UpdateAsync(client).Returns(1);

            _userContextMock.UserId.Returns(Guid.NewGuid());
            _dateTimeMock.UtcNow.Returns(DateTime.UtcNow);

            var result = await _service.UpdateAsync(id, request);

            Assert.NotNull(result);
            Assert.Equal("New", result.Name);
            Assert.Equal("new@gmail.com", result.Email);

            await _repoMock.Received(1).UpdateAsync(client);
        }

        // ================================
        // DELETE
        // ================================
        [Fact]
        public async Task DeleteAsync_ShouldSoftDelete()
        {
            var id = Guid.NewGuid();

            var client = new Client
            {
                Id = id,
                Name = "Test",
                Email = "test@gmail.com",
                IsDeleted = false
            };

            _repoMock.GetByIdAsync(id).Returns(client);
            _repoMock.UpdateAsync(client).Returns(1);

            var result = await _service.DeleteAsync(id);

            Assert.NotNull(client);
            Assert.True(client.IsDeleted);
            Assert.Equal(1, result);

            await _repoMock.Received().UpdateAsync(client);
        }

        // ================================
        // GET ALL
        // ================================
        [Fact]
        public async Task GetAllAsync_ShouldReturnList()
        {
            var clients = new List<Client>
            {
                new Client { Id = Guid.NewGuid(), Name = "A", Email = "a@gmail.com" },
                new Client { Id = Guid.NewGuid(), Name = "B", Email = "b@gmail.com" }
            };

            _repoMock.GetAllAsync().Returns(clients);

            var result = await _service.GetAllAsync();

            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
        }
    }
}