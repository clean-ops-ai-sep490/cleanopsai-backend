using CleanOpsAi.BuildingBlocks.Application;
using CleanOpsAi.BuildingBlocks.Application.Interfaces;
using CleanOpsAi.Modules.ClientManagement.Application.Dtos.Contracts;
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
    public class ContractServiceTests
    {
        private readonly IContractRepository _repoMock;
        private readonly IFileStorageService _fileMock;
        private readonly IUserContext _userContextMock;
        private readonly IDateTimeProvider _dateTimeMock;
        private readonly IIdGenerator _idGenMock;

        private readonly ContractService _service;

        public ContractServiceTests()
        {
            _repoMock = Substitute.For<IContractRepository>();
            _fileMock = Substitute.For<IFileStorageService>();
            _userContextMock = Substitute.For<IUserContext>();
            _dateTimeMock = Substitute.For<IDateTimeProvider>();
            _idGenMock = Substitute.For<IIdGenerator>();

            _service = new ContractService(
                _repoMock,
                _fileMock,
                _userContextMock,
                _dateTimeMock,
                _idGenMock
            );
        }

        // ================================
        // CREATE
        // ================================
        [Fact]
        public async Task CreateAsync_ShouldCreateContract_WithFile()
        {
            var request = new ContractCreateRequest
            {
                Name = "Contract A",
                ClientId = Guid.NewGuid(),
                ContractStartDate = DateTime.UtcNow,
                ContractEndDate = DateTime.UtcNow.AddDays(10),
                FileName = "file.pdf",
                FileStream = new MemoryStream()
            };

            var newId = Guid.NewGuid();

            _idGenMock.Generate().Returns(newId);
            _userContextMock.UserId.Returns(Guid.NewGuid());
            _dateTimeMock.UtcNow.Returns(DateTime.UtcNow);

            _fileMock.UploadFileAsync(
                Arg.Any<Stream>(),
                Arg.Any<string>(),
                Arg.Any<string>()
            ).Returns("url-file");

            _repoMock.CreateAsync(Arg.Any<Contract>()).Returns(1);

            var result = await _service.CreateAsync(request);

            Assert.NotNull(result);
            Assert.Equal(newId, result.Id);
            Assert.Equal("Contract A", result.Name);
            Assert.Equal("url-file", result.UrlFile);

            await _repoMock.Received(1).CreateAsync(Arg.Any<Contract>());
        }

        // ================================
        // GET BY ID
        // ================================
        [Fact]
        public async Task GetByIdAsync_ShouldReturnContract()
        {
            var id = Guid.NewGuid();

            var contract = new Contract
            {
                Id = id,
                Name = "Test Contract",
                ClientId = Guid.NewGuid()
            };

            _repoMock.GetByIdAsync(id).Returns(contract);

            var result = await _service.GetByIdAsync(id);

            Assert.NotNull(result);
            Assert.Equal(id, result.Id);
        }

        // ================================
        // UPDATE
        // ================================
        [Fact]
        public async Task UpdateAsync_ShouldUpdateContract_WithNewFile()
        {
            var id = Guid.NewGuid();

            var contract = new Contract
            {
                Id = id,
                Name = "Old Name"
            };

            var request = new ContractUpdateRequest
            {
                Name = "New Name",
                FileName = "new.pdf",
                FileStream = new MemoryStream()
            };

            _repoMock.GetByIdAsync(id).Returns(contract);

            _fileMock.UploadFileAsync(
                Arg.Any<Stream>(),
                Arg.Any<string>(),
                Arg.Any<string>()
            ).Returns("new-url");

            _repoMock.UpdateAsync(contract).Returns(1);

            _userContextMock.UserId.Returns(Guid.NewGuid());
            _dateTimeMock.UtcNow.Returns(DateTime.UtcNow);

            var result = await _service.UpdateAsync(id, request);

            Assert.NotNull(result);
            Assert.Equal("New Name", result.Name);
            Assert.Equal("new-url", result.UrlFile);

            await _repoMock.Received(1).UpdateAsync(contract);
        }

        // ================================
        // DELETE
        // ================================
        [Fact]
        public async Task DeleteAsync_ShouldCallRepository()
        {
            var id = Guid.NewGuid();

            var contract = new Contract
            {
                Id = id
            };

            _repoMock.GetByIdAsync(id).Returns(contract);
            _repoMock.DeleteAsync(id).Returns(1);

            var result = await _service.DeleteAsync(id);

            Assert.Equal(1, result);

            await _repoMock.Received(1).DeleteAsync(id);
        }

        // ================================
        // GET ALL
        // ================================
        [Fact]
        public async Task GetAllAsync_ShouldReturnList()
        {
            var contracts = new List<Contract>
            {
                new Contract { Id = Guid.NewGuid(), Name = "A" },
                new Contract { Id = Guid.NewGuid(), Name = "B" }
            };

            _repoMock.GetAllAsync().Returns(contracts);

            var result = await _service.GetAllAsync();

            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
        }

        // ================================
        // GET BY CLIENT ID
        // ================================
        [Fact]
        public async Task GetByClientIdAsync_ShouldReturnContracts()
        {
            var clientId = Guid.NewGuid();

            var contracts = new List<Contract>
            {
                new Contract { Id = Guid.NewGuid(), ClientId = clientId },
                new Contract { Id = Guid.NewGuid(), ClientId = clientId }
            };

            _repoMock.GetByClientIdAsync(clientId).Returns(contracts);

            var result = await _service.GetByClientIdAsync(clientId);

            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
        }
    }
}
