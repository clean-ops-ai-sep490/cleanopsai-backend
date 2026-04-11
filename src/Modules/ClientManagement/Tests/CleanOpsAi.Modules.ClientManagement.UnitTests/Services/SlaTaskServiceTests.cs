using CleanOpsAi.BuildingBlocks.Application;
using CleanOpsAi.BuildingBlocks.Application.Exceptions;
using CleanOpsAi.BuildingBlocks.Application.Interfaces;
using CleanOpsAi.BuildingBlocks.Domain.Dtos;
using CleanOpsAi.Modules.ClientManagement.Application.Dtos.SlaTasks;
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
    public class SlaTaskServiceTests
    {
        private readonly ISlaTaskRepository _repoMock = Substitute.For<ISlaTaskRepository>();
        private readonly ISlaRepository _slaRepoMock = Substitute.For<ISlaRepository>();
        private readonly IUserContext _userContextMock = Substitute.For<IUserContext>();
        private readonly IDateTimeProvider _dateTimeMock = Substitute.For<IDateTimeProvider>();
        private readonly IIdGenerator _idGenMock = Substitute.For<IIdGenerator>();

        private readonly SlaTaskService _service;

        public SlaTaskServiceTests()
        {
            _service = new SlaTaskService(
                _repoMock,
                _slaRepoMock,
                _userContextMock,
                _dateTimeMock,
                _idGenMock
            );
        }

        // ================================
        // CREATE - SUCCESS (DAILY)
        // ================================
        [Fact]
        public async Task CreateAsync_ShouldCreateTask_Daily()
        {
            var slaId = Guid.NewGuid();

            var request = new SlaTaskCreateRequest
            {
                Name = "Task A",
                SlaId = slaId,
                RecurrenceType = RecurrenceType.Daily,
                RecurrenceConfig = new RecurrenceConfigSlaTask
                {
                    Interval = 1
                }
            };

            _slaRepoMock.GetByIdAsync(slaId)
                .Returns(new Sla { Id = slaId, Name = "SLA A" });

            _idGenMock.Generate().Returns(Guid.NewGuid());
            _userContextMock.UserId.Returns(Guid.NewGuid());
            _dateTimeMock.UtcNow.Returns(DateTime.UtcNow);

            _repoMock.CreateAsync(Arg.Any<SlaTask>()).Returns(1);

            var result = await _service.CreateAsync(request);

            Assert.NotNull(result);
            Assert.Equal("Task A", result.Name);
            Assert.Equal("SLA A", result.SlaName);
            Assert.Equal(RecurrenceType.Daily, result.RecurrenceType);
        }

        // ================================
        // CREATE - INVALID CONFIG
        // ================================
        [Fact]
        public async Task CreateAsync_ShouldThrow_WhenInvalidConfig()
        {
            var request = new SlaTaskCreateRequest
            {
                SlaId = Guid.NewGuid(),
                RecurrenceType = RecurrenceType.Weekly,
                RecurrenceConfig = new RecurrenceConfigSlaTask
                {
                    Interval = 1
                    // ❌ thiếu DaysOfWeek
                }
            };

            await Assert.ThrowsAsync<ArgumentException>(() =>
                _service.CreateAsync(request));
        }

        // ================================
        // CREATE - SLA NOT FOUND
        // ================================
        [Fact]
        public async Task CreateAsync_ShouldThrow_WhenSlaNotFound()
        {
            var request = new SlaTaskCreateRequest
            {
                SlaId = Guid.NewGuid(),
                RecurrenceType = RecurrenceType.Daily,
                RecurrenceConfig = new RecurrenceConfigSlaTask
                {
                    Interval = 1
                }
            };

            _slaRepoMock.GetByIdAsync(request.SlaId)
                .Returns((Sla)null);

            await Assert.ThrowsAsync<NotFoundException>(() =>
                _service.CreateAsync(request));
        }

        // ================================
        // GET BY ID
        // ================================
        [Fact]
        public async Task GetByIdAsync_ShouldReturnTask()
        {
            var id = Guid.NewGuid();

            var entity = new SlaTask
            {
                Id = id,
                Name = "Task A",
                SlaId = Guid.NewGuid(),
                Sla = new Sla { Name = "SLA A" },
                RecurrenceType = "Daily",
                RecurrenceConfig = "{\"Interval\":1}"
            };

            _repoMock.GetByIdAsync(id).Returns(entity);

            var result = await _service.GetByIdAsync(id);

            Assert.NotNull(result);
            Assert.Equal(id, result.Id);
            Assert.Equal(RecurrenceType.Daily, result.RecurrenceType);
        }

        // ================================
        // GET BY ID - NOT FOUND
        // ================================
        [Fact]
        public async Task GetByIdAsync_ShouldThrow_WhenNotFound()
        {
            var id = Guid.NewGuid();

            _repoMock.GetByIdAsync(id).Returns((SlaTask)null);

            await Assert.ThrowsAsync<NotFoundException>(() =>
                _service.GetByIdAsync(id));
        }

        // ================================
        // UPDATE
        // ================================
        [Fact]
        public async Task UpdateAsync_ShouldUpdateTask()
        {
            var id = Guid.NewGuid();

            var entity = new SlaTask
            {
                Id = id,
                Name = "Old",
                SlaId = Guid.NewGuid(),
                Sla = new Sla { Name = "SLA A" },
                RecurrenceType = "Daily",
                RecurrenceConfig = "{\"Interval\":1}"
            };

            _repoMock.GetByIdAsync(id).Returns(entity);
            _repoMock.UpdateAsync(entity).Returns(1);

            _userContextMock.UserId.Returns(Guid.NewGuid());
            _dateTimeMock.UtcNow.Returns(DateTime.UtcNow);

            var request = new SlaTaskUpdateRequest
            {
                RecurrenceType = RecurrenceType.Weekly,
                RecurrenceConfig = new RecurrenceConfigSlaTask
                {
                    Interval = 1,
                    DaysOfWeek = new List<DayOfWeek>
                    {
                        DayOfWeek.Monday,
                        DayOfWeek.Tuesday
                    }
                }
            };

            var result = await _service.UpdateAsync(id, request);

            Assert.NotNull(result);
            Assert.Equal(RecurrenceType.Weekly, result.RecurrenceType);
        }

        // ================================
        // UPDATE - NOT FOUND
        // ================================
        [Fact]
        public async Task UpdateAsync_ShouldThrow_WhenNotFound()
        {
            var id = Guid.NewGuid();

            _repoMock.GetByIdAsync(id).Returns((SlaTask)null);

            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _service.UpdateAsync(id, new SlaTaskUpdateRequest()));
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

            var list = new List<SlaTask>
            {
                new SlaTask
                {
                    Id = Guid.NewGuid(),
                    Sla = new Sla { Name = "SLA A" },
                    RecurrenceType = "Daily",
                    RecurrenceConfig = "{\"Interval\":1}"
                }
            };

            _repoMock.GetBySlaIdAsync(slaId).Returns(list);

            var result = await _service.GetBySlaIdAsync(slaId);

            Assert.NotNull(result);
            Assert.Single(result);
        }
    }
}
