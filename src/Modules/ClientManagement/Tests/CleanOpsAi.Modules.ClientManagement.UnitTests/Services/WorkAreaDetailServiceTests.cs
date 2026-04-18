using CleanOpsAi.BuildingBlocks.Application;
using CleanOpsAi.BuildingBlocks.Application.Interfaces;
using CleanOpsAi.Modules.ClientManagement.Application.Dtos.WorkareaDetails;
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
    public class WorkAreaDetailServiceTests
    {
        private readonly IWorkAreaDetailRepository _repoMock = Substitute.For<IWorkAreaDetailRepository>();
        private readonly IUserContext _userContextMock = Substitute.For<IUserContext>();
        private readonly IDateTimeProvider _dateTimeMock = Substitute.For<IDateTimeProvider>();
        private readonly IIdGenerator _idGenMock = Substitute.For<IIdGenerator>();

        private readonly WorkAreaDetailService _service;

        public WorkAreaDetailServiceTests()
        {
            _service = new WorkAreaDetailService(
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
        public async Task CreateAsync_ShouldCreateWorkAreaDetail()
        {
            var newId = Guid.NewGuid();

            var request = new WorkAreaDetailCreateRequest
            {
                Name = "Area A",
                Area = 100,
                TotalArea = 200,
                WorkAreaId = Guid.NewGuid()
            };

            _idGenMock.Generate().Returns(newId);
            _userContextMock.UserId.Returns(Guid.NewGuid());
            _dateTimeMock.UtcNow.Returns(DateTime.UtcNow);

            _repoMock.CreateAsync(Arg.Any<WorkAreaDetail>())
                .Returns(call => Task.FromResult(call.Arg<WorkAreaDetail>()));

            var result = await _service.CreateAsync(request);

            Assert.NotNull(result);
            Assert.Equal(newId, result.Id);
            Assert.Equal("Area A", result.Name);

            await _repoMock.Received(1).CreateAsync(Arg.Any<WorkAreaDetail>());
        }

        // ================================
        // GET BY ID
        // ================================
        [Fact]
        public async Task GetByIdAsync_ShouldReturnEntity()
        {
            var id = Guid.NewGuid();

            var entity = new WorkAreaDetail
            {
                Id = id,
                Name = "Area A",
                Area = 100,
                TotalArea = 200,
                WorkAreaId = Guid.NewGuid(),
                WorkArea = new WorkArea { Name = "WorkArea A" }
            };

            _repoMock.GetByIdAsync(id).Returns(entity);

            var result = await _service.GetByIdAsync(id);

            Assert.NotNull(result);
            Assert.Equal(id, result.Id);
            Assert.Equal("WorkArea A", result.WorkAreaName);
        }

        // ================================
        // GET BY ID - NOT FOUND
        // ================================
        [Fact]
        public async Task GetByIdAsync_ShouldReturnNull_WhenNotFound()
        {
            var id = Guid.NewGuid();

            _repoMock.GetByIdAsync(id).Returns((WorkAreaDetail)null);

            var result = await _service.GetByIdAsync(id);

            Assert.Null(result);
        }

        // ================================
        // UPDATE
        // ================================
        [Fact]
        public async Task UpdateAsync_ShouldUpdateEntity()
        {
            var id = Guid.NewGuid();

            var entity = new WorkAreaDetail
            {
                Id = id,
                Name = "Old",
                Area = 50,
                TotalArea = 100
            };

            var request = new WorkAreaDetailUpdateRequest
            {
                Name = "New",
                Area = 150,
                TotalArea = 300
            };

            _repoMock.GetByIdAsync(id).Returns(entity);
            _repoMock.UpdateAsync(entity).Returns(1);

            _userContextMock.UserId.Returns(Guid.NewGuid());
            _dateTimeMock.UtcNow.Returns(DateTime.UtcNow);

            var result = await _service.UpdateAsync(id, request);

            Assert.NotNull(result);
            Assert.Equal("New", result.Name);
            Assert.Equal(150, result.Area);
        }

        // ================================
        // UPDATE - NOT FOUND
        // ================================
        [Fact]
        public async Task UpdateAsync_ShouldReturnNull_WhenNotFound()
        {
            var id = Guid.NewGuid();

            _repoMock.GetByIdAsync(id).Returns((WorkAreaDetail)null);

            var result = await _service.UpdateAsync(id, new WorkAreaDetailUpdateRequest());

            Assert.Null(result);
        }

        // ================================
        // DELETE
        // ================================
        [Fact]
        public async Task DeleteAsync_ShouldDeleteEntity()
        {
            var id = Guid.NewGuid();

            var entity = new WorkAreaDetail { Id = id };

            _repoMock.GetByIdAsync(id).Returns(entity);
            _repoMock.DeleteAsync(entity).Returns(1);

            var result = await _service.DeleteAsync(id);

            Assert.Equal(1, result);
            await _repoMock.Received(1).DeleteAsync(entity);
        }

        // ================================
        // DELETE - NOT FOUND
        // ================================
        [Fact]
        public async Task DeleteAsync_ShouldReturnZero_WhenNotFound()
        {
            var id = Guid.NewGuid();

            _repoMock.GetByIdAsync(id).Returns((WorkAreaDetail)null);

            var result = await _service.DeleteAsync(id);

            Assert.Equal(0, result);
        }

        // ================================
        // GET PAGINATION
        // ================================
        [Fact]
        public async Task GetAllPaginationAsync_ShouldReturnPagedResult()
        {
            var list = new List<WorkAreaDetail>
            {
                new WorkAreaDetail { Id = Guid.NewGuid(), Name = "A" },
                new WorkAreaDetail { Id = Guid.NewGuid(), Name = "B" }
            };

            _repoMock.GetAllPaginationAsync(1, 10).Returns((list, 2));

            var result = await _service.GetAllPaginationAsync(1, 10);

            Assert.NotNull(result);
            Assert.Equal(2, result.Content.Count);
            Assert.Equal(2, result.TotalElements);
        }

        // ================================
        // GET BY WORK AREA ID PAGINATION
        // ================================
        [Fact]
        public async Task GetByWorkAreaIdPaginationAsync_ShouldReturnPagedResult()
        {
            var workAreaId = Guid.NewGuid();

            var list = new List<WorkAreaDetail>
            {
                new WorkAreaDetail { Id = Guid.NewGuid(), WorkAreaId = workAreaId }
            };

            _repoMock.GetByWorkAreaIdPaginationAsync(workAreaId, 1, 10)
                .Returns((list, 1));

            var result = await _service.GetByWorkAreaIdPaginationAsync(workAreaId, 1, 10);

            Assert.NotNull(result);
            Assert.Single(result.Content);
        }
    }
}
