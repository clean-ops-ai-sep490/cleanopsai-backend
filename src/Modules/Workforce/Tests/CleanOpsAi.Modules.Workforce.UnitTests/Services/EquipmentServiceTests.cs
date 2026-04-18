using CleanOpsAi.BuildingBlocks.Application;
using CleanOpsAi.BuildingBlocks.Application.Interfaces;
using CleanOpsAi.Modules.Workforce.Application.Dtos.Equipments;
using CleanOpsAi.Modules.Workforce.Application.Interfaces;
using CleanOpsAi.Modules.Workforce.Application.Services;
using CleanOpsAi.Modules.Workforce.Domain.Entities;
using CleanOpsAi.Modules.Workforce.Domain.Enums;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanOpsAi.Modules.Workforce.UnitTests.Services
{
    public class EquipmentServiceTests
    {
        private readonly IEquipmentRepository _repoMock;
        private readonly IUserContext _userContextMock;
        private readonly IDateTimeProvider _dateTimeMock;

        private readonly EquipmentService _service;

        public EquipmentServiceTests()
        {
            _repoMock = Substitute.For<IEquipmentRepository>();
            _userContextMock = Substitute.For<IUserContext>();
            _dateTimeMock = Substitute.For<IDateTimeProvider>();

            _service = new EquipmentService(
                _repoMock,
                _userContextMock,
                _dateTimeMock
            );
        }

        // ================================
        // CREATE
        // ================================
        [Fact]
        public async Task CreateAsync_ShouldReturnEquipmentResponse()
        {
            var request = new EquipmentCreateRequest
            {
                Name = "Vacuum",
                Type = EquipmentType.Dustpan,
                Description = "Cleaning device"
            };

            _userContextMock.UserId.Returns(Guid.NewGuid());
            _dateTimeMock.UtcNow.Returns(DateTime.UtcNow);

            _repoMock.CreateAsync(Arg.Any<Equipment>())
                     .Returns(1);

            var result = await _service.CreateAsync(request);

            Assert.NotNull(result);
            Assert.Equal(request.Name, result!.Name);
            Assert.Equal(request.Type, result.Type);

            await _repoMock.Received(1).CreateAsync(Arg.Any<Equipment>());
        }

        // ================================
        // GET BY ID
        // ================================
        [Fact]
        public async Task GetByIdAsync_ShouldReturnEquipment()
        {
            var id = Guid.NewGuid();

            var equipment = new Equipment
            {
                Id = id,
                Name = "Vacuum",
                Type = EquipmentType.DisinfectantSprayer,
                Description = "Cleaning device"
            };

            _repoMock.GetByIdAsync(id).Returns(equipment);

            var result = await _service.GetByIdAsync(id);

            Assert.NotNull(result);
            Assert.Single(result!);
            Assert.Equal(id, result![0].Id);
        }

        // ================================
        // GET ALL
        // ================================
        [Fact]
        public async Task GetAllAsync_ShouldReturnList()
        {
            var data = new List<Equipment>
            {
                new Equipment { Id = Guid.NewGuid(), Name = "A" },
                new Equipment { Id = Guid.NewGuid(), Name = "B" }
            };

            _repoMock.GetAllAsync().Returns(data);

            var result = await _service.GetAllAsync();

            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
        }

        // ================================
        // UPDATE
        // ================================
        [Fact]
        public async Task UpdateAsync_ShouldUpdateEquipment()
        {
            var id = Guid.NewGuid();

            var equipment = new Equipment
            {
                Id = id,
                Name = "Old",
                Type = EquipmentType.Squeegee,
                Description = "Old desc"
            };

            var request = new EquipmentUpdateRequest
            {
                Name = "New",
                Type = EquipmentType.Mop,
                Description = "New desc"
            };

            _repoMock.GetByIdAsync(id).Returns(equipment);
            _repoMock.UpdateAsync(equipment).Returns(1);

            _userContextMock.UserId.Returns(Guid.NewGuid());
            _dateTimeMock.UtcNow.Returns(DateTime.UtcNow);

            var result = await _service.UpdateAsync(id, request);

            Assert.NotNull(result);
            Assert.Equal("New", result!.Name);
            Assert.Equal(EquipmentType.Mop, result.Type);

            await _repoMock.Received(1).UpdateAsync(equipment);
        }

        // ================================
        // UPDATE - NOT FOUND
        // ================================
        [Fact]
        public async Task UpdateAsync_ShouldReturnNull_WhenNotFound()
        {
            var id = Guid.NewGuid();

            _repoMock.GetByIdAsync(id).Returns((Equipment?)null);

            var result = await _service.UpdateAsync(id, new EquipmentUpdateRequest());

            Assert.Null(result);
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
        // GET ALL PAGINATION
        // ================================
        [Fact]
        public async Task GetAllPaginationAsync_ShouldReturnPagedResult()
        {
            var data = new List<Equipment>
            {
                new Equipment { Id = Guid.NewGuid(), Name = "A" },
                new Equipment { Id = Guid.NewGuid(), Name = "B" }
            };

            _repoMock.GetAllPaginationAsync(1, 10)
                     .Returns((data, 2));

            var result = await _service.GetAllPaginationAsync(1, 10);

            Assert.NotNull(result);
            Assert.Equal(2, result.Content.Count);
            Assert.Equal(1, result.PageNumber);
        }

        // ================================
        // SEARCH PAGINATION
        // ================================
        [Fact]
        public async Task SearchPaginationAsync_ShouldReturnPagedResult()
        {
            var data = new List<Equipment>
            {
                new Equipment { Id = Guid.NewGuid(), Name = "Vacuum" }
            };

            _repoMock.SearchPaginationAsync("Vac", 1, 10)
                     .Returns((data, 1));

            var result = await _service.SearchPaginationAsync("Vac", 1, 10);

            Assert.NotNull(result);
            Assert.Single(result.Content);
            Assert.Equal(1, result.TotalElements);
        }
    }
}
