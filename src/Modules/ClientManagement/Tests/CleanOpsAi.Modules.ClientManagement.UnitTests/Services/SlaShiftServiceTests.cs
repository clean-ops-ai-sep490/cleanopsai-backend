using CleanOpsAi.BuildingBlocks.Application;
using CleanOpsAi.BuildingBlocks.Application.Exceptions;
using CleanOpsAi.BuildingBlocks.Application.Interfaces;
using CleanOpsAi.Modules.ClientManagement.Application.Dtos.SlaShifts;
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
    public class SlaShiftServiceTests
    {
        private readonly ISlaShiftRepository _repoMock = Substitute.For<ISlaShiftRepository>();
        private readonly ISlaRepository _slaRepoMock = Substitute.For<ISlaRepository>();
        private readonly IUserContext _userContextMock = Substitute.For<IUserContext>();
        private readonly IDateTimeProvider _dateTimeMock = Substitute.For<IDateTimeProvider>();

        private readonly SlaShiftService _service;

        public SlaShiftServiceTests()
        {
            _service = new SlaShiftService(
                _repoMock,
                _slaRepoMock,
                _userContextMock,
                _dateTimeMock
            );
        }

        // ================================
        // CREATE
        // ================================
        [Fact]
        public async Task CreateAsync_ShouldCreateShift()
        {
            var slaId = Guid.NewGuid();

            var request = new SlaShiftCreateRequest
            {
                Name = "Shift A",
                SlaId = slaId,
                StartTime = new TimeOnly(8, 0),
                EndTime = new TimeOnly(17, 0),
                RequiredWorker = 5,
                BreakTime = 60
            };

            var sla = new Sla { Id = slaId, Name = "SLA A" };

            _slaRepoMock.GetByIdAsync(slaId).Returns(sla);
            _repoMock.CreateAsync(Arg.Any<SlaShift>()).Returns(1);

            _userContextMock.UserId.Returns(Guid.NewGuid());
            _dateTimeMock.UtcNow.Returns(DateTime.UtcNow);

            var result = await _service.CreateAsync(request);

            Assert.NotNull(result);
            Assert.Equal("Shift A", result.Name);
            Assert.Equal("SLA A", result.SlaName);

            await _repoMock.Received(1).CreateAsync(Arg.Any<SlaShift>());
        }

        // ================================
        // CREATE - SLA NOT FOUND
        // ================================
        [Fact]
        public async Task CreateAsync_ShouldThrow_WhenSlaNotFound()
        {
            var request = new SlaShiftCreateRequest
            {
                SlaId = Guid.NewGuid()
            };

            _slaRepoMock.GetByIdAsync(request.SlaId).Returns((Sla)null);

            await Assert.ThrowsAsync<NotFoundException>(() =>
                _service.CreateAsync(request));
        }

        // ================================
        // GET BY ID
        // ================================
        [Fact]
        public async Task GetByIdAsync_ShouldReturnShift()
        {
            var id = Guid.NewGuid();

            var shift = new SlaShift
            {
                Id = id,
                Name = "Shift A",
                SlaId = Guid.NewGuid(),
                Sla = new Sla { Name = "SLA A" }
            };

            _repoMock.GetByIdAsync(id).Returns(shift);

            var result = await _service.GetByIdAsync(id);

            Assert.NotNull(result);
            Assert.Equal(id, result.Id);
            Assert.Equal("SLA A", result.SlaName);
        }

        // ================================
        // GET BY ID - NOT FOUND
        // ================================
        [Fact]
        public async Task GetByIdAsync_ShouldReturnNull_WhenNotFound()
        {
            var id = Guid.NewGuid();

            _repoMock.GetByIdAsync(id).Returns((SlaShift)null);

            var result = await _service.GetByIdAsync(id);

            Assert.Null(result);
        }

        // ================================
        // UPDATE
        // ================================
        [Fact]
        public async Task UpdateAsync_ShouldUpdateShift()
        {
            var id = Guid.NewGuid();

            var shift = new SlaShift
            {
                Id = id,
                Name = "Old",
                StartTime = new TimeOnly(8, 0),
                EndTime = new TimeOnly(17, 0),
                RequiredWorker = 3,
                BreakTime = 30,
                Sla = new Sla { Name = "SLA A" }
            };

            _repoMock.GetByIdAsync(id).Returns(shift);
            _repoMock.UpdateAsync(shift).Returns(1);

            _userContextMock.UserId.Returns(Guid.NewGuid());
            _dateTimeMock.UtcNow.Returns(DateTime.UtcNow);

            var request = new SlaShiftUpdateRequest
            {
                Name = "New",
                RequiredWorker = 10
            };

            var result = await _service.UpdateAsync(id, request);

            Assert.NotNull(result);
            Assert.Equal("New", result.Name);
            Assert.Equal(10, result.RequiredWorker);
        }

        // ================================
        // UPDATE - NOT FOUND
        // ================================
        [Fact]
        public async Task UpdateAsync_ShouldThrow_WhenNotFound()
        {
            var id = Guid.NewGuid();

            _repoMock.GetByIdAsync(id).Returns((SlaShift)null);

            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _service.UpdateAsync(id, new SlaShiftUpdateRequest()));
        }

        // ================================
        // DELETE
        // ================================
        [Fact]
        public async Task DeleteAsync_ShouldCallRepository()
        {
            var id = Guid.NewGuid();

            _repoMock.DeleteAsync(id).Returns(1);

            var result = await _service.DeleteAsync(id);

            Assert.Equal(1, result);
            await _repoMock.Received(1).DeleteAsync(id);
        }

        // ================================
        // GET BY SLA ID
        // ================================
        [Fact]
        public async Task GetBySlaIdAsync_ShouldReturnList()
        {
            var slaId = Guid.NewGuid();

            var list = new List<SlaShift>
            {
                new SlaShift { Id = Guid.NewGuid(), Sla = new Sla { Name = "SLA A" } },
                new SlaShift { Id = Guid.NewGuid(), Sla = new Sla { Name = "SLA A" } }
            };

            _repoMock.GetBySlaIdAsync(slaId).Returns(list);

            var result = await _service.GetBySlaIdAsync(slaId);

            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
        }
    }
}
