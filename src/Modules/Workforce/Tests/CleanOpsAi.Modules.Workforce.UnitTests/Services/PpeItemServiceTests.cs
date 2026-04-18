using CleanOpsAi.BuildingBlocks.Application;
using CleanOpsAi.BuildingBlocks.Application.Exceptions;
using CleanOpsAi.BuildingBlocks.Application.Interfaces;
using CleanOpsAi.Modules.Workforce.Application.Dtos.PpeItems;
using CleanOpsAi.Modules.Workforce.Application.Interfaces;
using CleanOpsAi.Modules.Workforce.Application.Services;
using CleanOpsAi.Modules.Workforce.Domain.Entities;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanOpsAi.Modules.Workforce.UnitTests.Services
{
    public class PpeItemServiceTests
    {
        private readonly IPpeItemRepository _repoMock;
        private readonly IUserContext _userContextMock;
        private readonly IDateTimeProvider _dateTimeMock;
        private readonly IFileStorageService _fileStorageMock;

        private readonly PpeItemService _service;

        public PpeItemServiceTests()
        {
            _repoMock = Substitute.For<IPpeItemRepository>();
            _userContextMock = Substitute.For<IUserContext>();
            _dateTimeMock = Substitute.For<IDateTimeProvider>();
            _fileStorageMock = Substitute.For<IFileStorageService>();

            _service = new PpeItemService(
                _repoMock,
                _userContextMock,
                _dateTimeMock,
                _fileStorageMock
            );
        }

        // ================================
        // CREATE - NO IMAGE
        // ================================
        [Fact]
        public async Task CreateAsync_WithoutImage_ShouldCreateSuccessfully()
        {
            var request = new PpeItemCreateRequest
            {
                ActionKey = " CLEAN ",
                Name = "Gloves",
                Description = "Protect hand"
            };

            _userContextMock.UserId.Returns(Guid.NewGuid());
            _dateTimeMock.UtcNow.Returns(DateTime.UtcNow);

            _repoMock.CreateAsync(Arg.Any<PpeItem>()).Returns(1);

            var result = await _service.CreateAsync(request);

            Assert.NotNull(result);
            Assert.Equal("clean", result.ActionKey); // check trim + lower
            Assert.Equal(request.Name, result.Name);
            Assert.Equal("", result.ImageUrl);

            await _repoMock.Received(1).CreateAsync(Arg.Any<PpeItem>());
        }

        // ================================
        // CREATE - WITH IMAGE
        // ================================
        [Fact]
        public async Task CreateAsync_WithImage_ShouldUploadAndSaveUrl()
        {
            var stream = new MemoryStream();

            var request = new PpeItemCreateRequest
            {
                ActionKey = "clean",
                Name = "Mask",
                ImageStream = stream,
                ImageFileName = "mask.png"
            };

            _userContextMock.UserId.Returns(Guid.NewGuid());
            _dateTimeMock.UtcNow.Returns(DateTime.UtcNow);

            _fileStorageMock.UploadFileAsync(stream, Arg.Any<string>(), Arg.Any<string>())
                .Returns("http://image-url");

            var result = await _service.CreateAsync(request);

            Assert.NotNull(result);
            Assert.Equal("http://image-url", result.ImageUrl);

            await _fileStorageMock.Received(1)
                .UploadFileAsync(stream, Arg.Any<string>(), Arg.Any<string>());
        }

        // ================================
        // GET BY ID
        // ================================
        [Fact]
        public async Task GetByIdAsync_ShouldReturnItem()
        {
            var id = Guid.NewGuid();

            var entity = new PpeItem
            {
                Id = id,
                ActionKey = "clean",
                Name = "Gloves"
            };

            _repoMock.GetByIdAsync(id).Returns(entity);

            var result = await _service.GetByIdAsync(id);

            Assert.NotNull(result);
            Assert.Single(result!);
            Assert.Equal(id, result![0].Id);
        }

        // ================================
        // UPDATE - SUCCESS
        // ================================
        [Fact]
        public async Task UpdateAsync_ShouldUpdateItem()
        {
            var id = Guid.NewGuid();

            var entity = new PpeItem
            {
                Id = id,
                ActionKey = "clean",
                Name = "Old"
            };

            var request = new PpeItemUpdateRequest
            {
                ActionKey = " NEW ",
                Name = "New Name"
            };

            _repoMock.GetByIdAsync(id).Returns(entity);
            _repoMock.UpdateAsync(entity).Returns(1);

            _userContextMock.UserId.Returns(Guid.NewGuid());
            _dateTimeMock.UtcNow.Returns(DateTime.UtcNow);

            var result = await _service.UpdateAsync(id, request);

            Assert.Equal("new", result.ActionKey);
            Assert.Equal("New Name", result.Name);

            await _repoMock.Received(1).UpdateAsync(entity);
        }

        // ================================
        // UPDATE - WITH IMAGE
        // ================================
        [Fact]
        public async Task UpdateAsync_WithImage_ShouldUploadAndUpdateUrl()
        {
            var id = Guid.NewGuid();

            var entity = new PpeItem
            {
                Id = id,
                ImageUrl = "old-url"
            };

            var stream = new MemoryStream();

            var request = new PpeItemUpdateRequest
            {
                ImageStream = stream,
                ImageFileName = "new.png"
            };

            _repoMock.GetByIdAsync(id).Returns(entity);
            _repoMock.UpdateAsync(entity).Returns(1);

            _fileStorageMock.UploadFileAsync(stream, Arg.Any<string>(), Arg.Any<string>())
                .Returns("new-url");

            _userContextMock.UserId.Returns(Guid.NewGuid());
            _dateTimeMock.UtcNow.Returns(DateTime.UtcNow);

            var result = await _service.UpdateAsync(id, request);

            Assert.Equal("new-url", result.ImageUrl);
        }

        // ================================
        // UPDATE - NOT FOUND
        // ================================
        [Fact]
        public async Task UpdateAsync_ShouldThrow_WhenNotFound()
        {
            var id = Guid.NewGuid();

            _repoMock.GetByIdAsync(id).Returns((PpeItem?)null);

            await Assert.ThrowsAsync<NotFoundException>(() =>
                _service.UpdateAsync(id, new PpeItemUpdateRequest()));
        }

        // ================================
        // DELETE
        // ================================
        [Fact]
        public async Task DeleteAsync_ShouldCallRepository()
        {
            var id = Guid.NewGuid();

            var entity = new PpeItem { Id = id };

            _repoMock.GetByIdAsync(id).Returns(entity);
            _repoMock.DeleteAsync(id).Returns(1);

            var result = await _service.DeleteAsync(id);

            Assert.Equal(1, result);
            await _repoMock.Received(1).DeleteAsync(id);
        }

        // ================================
        // DELETE - NOT FOUND
        // ================================
        [Fact]
        public async Task DeleteAsync_ShouldThrow_WhenNotFound()
        {
            var id = Guid.NewGuid();

            _repoMock.GetByIdAsync(id).Returns((PpeItem?)null);

            await Assert.ThrowsAsync<NotFoundException>(() =>
                _service.DeleteAsync(id));
        }

        // ================================
        // GET BY ACTION KEY
        // ================================
        [Fact]
        public async Task GetByActionKeyAsync_ShouldReturnList()
        {
            var data = new List<PpeItem>
            {
                new PpeItem { Id = Guid.NewGuid(), ActionKey = "clean" }
            };

            _repoMock.GetByActionKeyAsync("clean").Returns(data);

            var result = await _service.GetByActionKeyAsync(" CLEAN ");

            Assert.Single(result);
        }

        // ================================
        // GET ALL ACTION KEYS
        // ================================
        [Fact]
        public async Task GetAllActionKeysAsync_ShouldReturnList()
        {
            var keys = new List<string> { "clean", "wash" };

            _repoMock.GetAllActionKeysAsync().Returns(keys);

            var result = await _service.GetAllActionKeysAsync();

            Assert.Equal(2, result.Count);
        }

        // ================================
        // PAGINATION
        // ================================
        [Fact]
        public async Task GetAllPaginationAsync_ShouldReturnPagedResult()
        {
            var data = new List<PpeItem>
            {
                new PpeItem { Id = Guid.NewGuid(), Name = "A" }
            };

            _repoMock.GetAllPaginationAsync(1, 10)
                     .Returns((data, 1));

            var result = await _service.GetAllPaginationAsync(1, 10);

            Assert.Single(result.Content);
            Assert.Equal(1, result.TotalElements);
        }
    }
}
