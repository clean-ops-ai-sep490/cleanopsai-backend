using CleanOpsAi.BuildingBlocks.Application;
using CleanOpsAi.BuildingBlocks.Application.Interfaces;
using CleanOpsAi.Modules.Workforce.Application.Dtos.Certifications;
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
    public class CertificationServiceTests
    {
        private readonly ICertificationRepository _repoMock;
        private readonly IUserContext _userContextMock;
        private readonly IDateTimeProvider _dateTimeMock;
		private readonly ISkillRepository _skillRepoMock;

		private readonly CertificationService _service;

        public CertificationServiceTests()
        {
            _repoMock = Substitute.For<ICertificationRepository>();
            _userContextMock = Substitute.For<IUserContext>();
            _dateTimeMock = Substitute.For<IDateTimeProvider>();
            _skillRepoMock = Substitute.For<ISkillRepository>();

			_service = new CertificationService(
                _repoMock,
                _userContextMock,
                _dateTimeMock,
				_skillRepoMock
			);
        }

        // ================================
        // CREATE
        // ================================
        [Fact]
        public async Task CreateAsync_ShouldReturnCertificationResponse()
        {
            var request = new CertificationCreateRequest
            {
                Name = "AWS",
                Category = "Cloud",
                IssuingOrganization = "Amazon"
            };

            _userContextMock.UserId.Returns(Guid.NewGuid());
            _dateTimeMock.UtcNow.Returns(DateTime.UtcNow);

            _repoMock.CreateAsync(Arg.Any<Certification>())
                     .Returns(1);

            var result = await _service.CreateAsync(request);

            Assert.NotNull(result);
            Assert.Equal(request.Name, result.Name);
            Assert.Equal(request.Category, result.Category);

            await _repoMock.Received(1).CreateAsync(Arg.Any<Certification>());
        }

        // ================================
        // GET BY ID
        // ================================
        [Fact]
        public async Task GetByIdAsync_ShouldReturnCertification()
        {
            var id = Guid.NewGuid();

            var cert = new Certification
            {
                Id = id,
                Name = "AWS",
                Category = "Cloud",
                IssuingOrganization = "Amazon"
            };

            _repoMock.GetByIdAsync(id).Returns(cert);

            var result = await _service.GetByIdAsync(id);

            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal(id, result[0].Id);
        }

        // ================================
        // GET ALL
        // ================================
        [Fact]
        public async Task GetAllAsync_ShouldReturnList()
        {
            var data = new List<Certification>
            {
                new Certification { Id = Guid.NewGuid(), Name = "A" },
                new Certification { Id = Guid.NewGuid(), Name = "B" }
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
        public async Task UpdateAsync_ShouldUpdateCertification()
        {
            var id = Guid.NewGuid();

            var cert = new Certification
            {
                Id = id,
                Name = "Old",
                Category = "OldCat",
                IssuingOrganization = "OldOrg"
            };

            var request = new CertificationUpdateRequest
            {
                Name = "New",
                Category = "NewCat",
                IssuingOrganization = "NewOrg"
            };

            _repoMock.GetByIdAsync(id).Returns(cert);
            _repoMock.UpdateAsync(cert).Returns(1);

            _userContextMock.UserId.Returns(Guid.NewGuid());
            _dateTimeMock.UtcNow.Returns(DateTime.UtcNow);

            var result = await _service.UpdateAsync(id, request);

            Assert.NotNull(result);
            Assert.Equal("New", result.Name);
            Assert.Equal("NewCat", result.Category);

            await _repoMock.Received(1).UpdateAsync(cert);
        }

        // ================================
        // DELETE
        // ================================
        [Fact]
        public async Task DeleteAsync_ShouldCallRepository()
        {
            var id = Guid.NewGuid();

            var cert = new Certification { Id = id };

            _repoMock.GetByIdAsync(id).Returns(cert);
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
            var data = new List<Certification>
            {
                new Certification { Id = Guid.NewGuid(), Name = "A" },
                new Certification { Id = Guid.NewGuid(), Name = "B" }
            };

            _repoMock.GetAllPaginationAsync(1, 10)
                     .Returns((data, 2));

            var result = await _service.GetAllPaginationAsync(1, 10);

            Assert.NotNull(result);
            Assert.Equal(2, result.Content.Count);
            Assert.Equal(1, result.PageNumber);
        }

        // ================================
        // GET BY CATEGORY
        // ================================
        [Fact]
        public async Task GetByCategoryAsync_ShouldReturnList()
        {
            var data = new List<Certification>
            {
                new Certification { Id = Guid.NewGuid(), Category = "Cloud" }
            };

            _repoMock.GetByCategoryAsync("Cloud").Returns(data);

            var result = await _service.GetByCategoryAsync("Cloud");

            Assert.NotNull(result);
            Assert.Single(result);
        }

        // ================================
        // GET ALL CATEGORIES
        // ================================
        [Fact]
        public async Task GetAllCategoriesAsync_ShouldReturnList()
        {
            var categories = new List<string> { "Cloud", "DevOps" };

            _repoMock.GetAllCategoriesAsync().Returns(categories);

            var result = await _service.GetAllCategoriesAsync();

            Assert.Equal(2, result.Count);
        }
    }
}
