using CleanOpsAi.BuildingBlocks.Application;
using CleanOpsAi.BuildingBlocks.Application.Exceptions;
using CleanOpsAi.BuildingBlocks.Application.Interfaces;
using CleanOpsAi.Modules.ClientManagement.Application.Dtos.Slas;
using CleanOpsAi.Modules.ClientManagement.Application.Interfaces;
using CleanOpsAi.Modules.ClientManagement.Application.Services;
using CleanOpsAi.Modules.ClientManagement.Domain.Entities;
using CleanOpsAi.Modules.ClientManagement.Domain.Enums;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace CleanOpsAi.Modules.ClientManagement.UnitTests.Services
{
    public class SlaServiceTests
    {
        private readonly ISlaRepository _slaRepoMock = Substitute.For<ISlaRepository>();
        private readonly IWorkAreaRepository _workAreaRepoMock = Substitute.For<IWorkAreaRepository>();
        private readonly IContractRepository _contractRepoMock = Substitute.For<IContractRepository>();
        private readonly IUserContext _userContextMock = Substitute.For<IUserContext>();
        private readonly IDateTimeProvider _dateTimeMock = Substitute.For<IDateTimeProvider>();
        private readonly IIdGenerator _idGenMock = Substitute.For<IIdGenerator>();

        private readonly SlaService _service;

        public SlaServiceTests()
        {
            _service = new SlaService(
                _slaRepoMock,
                _contractRepoMock,
                _workAreaRepoMock,
                _userContextMock,
                _dateTimeMock,
                _idGenMock
            );
        }

        [Fact]
        public async Task CreateAsync_ShouldCreateSla()
        {
            var request = new SlaCreateRequest
            {
                Name = "SLA A",
                Description = "Test",
                EnvironmentTypeId = Guid.NewGuid(),
                ServiceType = ServiceType.Cleaning,
                WorkAreaId = Guid.NewGuid(),
                ContractId = Guid.NewGuid()
            };

            _workAreaRepoMock.GetByIdAsync(request.WorkAreaId)
                .Returns(new WorkArea());

            _contractRepoMock.GetByIdAsync(request.ContractId)
                .Returns(new Contract());

            _slaRepoMock.CreateAsync(Arg.Any<Sla>())
                .Returns(callInfo =>
                {
                    var sla = callInfo.Arg<Sla>();

                    // FIX tại đây
                    sla.WorkArea = new WorkArea { Name = "Area A" };
                    sla.Contract = new Contract { Name = "Contract A" };

                    return 1;
                });

            var result = await _service.CreateAsync(request);

            Assert.NotNull(result);
            Assert.Equal("SLA A", result.Name);
        }

        [Fact]
        public async Task UpdateAsync_ShouldUpdateSla()
        {
            var id = Guid.NewGuid();
            var envId = Guid.NewGuid();

            var sla = new Sla
            {
                Id = id,
                Name = "Old",
                Description = "Old",
                EnvironmentTypeId = Guid.NewGuid(),
                WorkArea = new WorkArea { Name = "Area" },
                Contract = new Contract { Name = "Contract" }
            };

            _slaRepoMock.GetByIdAsync(id).Returns(sla);
            _slaRepoMock.UpdateAsync(sla).Returns(1);

            _userContextMock.UserId.Returns(Guid.NewGuid());
            _dateTimeMock.UtcNow.Returns(DateTime.UtcNow);

            var request = new SlaUpdateRequest
            {
                Name = "New",
                Description = "New",
                EnvironmentTypeId = envId,
                ServiceType = ServiceType.Cleaning
            };

            var result = await _service.UpdateAsync(id, request);

            Assert.Equal("New", result.Name);
            Assert.Equal(envId, result.EnvironmentTypeId);
        }

        [Fact]
        public async Task DeleteAsync_ShouldCallRepo()
        {
            var id = Guid.NewGuid();

            _slaRepoMock.GetByIdAsync(id).Returns(new Sla { Id = id });
            _slaRepoMock.DeleteAsync(id).Returns(1);

            var result = await _service.DeleteAsync(id);

            Assert.Equal(1, result);
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnList()
        {
            _slaRepoMock.GetAllAsync().Returns(new List<Sla>
            {
                new Sla { Id = Guid.NewGuid(), Name = "A" }
            });

            var result = await _service.GetAllAsync();

            Assert.Single(result);
        }

        [Fact]
        public async Task FilterAsync_ShouldReturnPaged()
        {
            _slaRepoMock.FilterPaginationAsync(
                Arg.Any<Guid?>(),
                Arg.Any<Guid?>(),
                Arg.Any<int>(),
                Arg.Any<int>())
            .Returns((new List<Sla> { new Sla { Id = Guid.NewGuid() } }, 1));

            var result = await _service.FilterAsync(null, null, 1, 10);

            Assert.Equal(1, result.TotalElements);
        }
    }
}