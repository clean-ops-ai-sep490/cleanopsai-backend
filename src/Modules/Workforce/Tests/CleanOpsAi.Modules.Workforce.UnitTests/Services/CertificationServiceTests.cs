using CleanOpsAi.BuildingBlocks.Application;
using CleanOpsAi.BuildingBlocks.Application.Interfaces;
using CleanOpsAi.Modules.Workforce.Application.Dtos.Certifications;
using CleanOpsAi.Modules.Workforce.Application.Interfaces;
using CleanOpsAi.Modules.Workforce.Application.Services;
using CleanOpsAi.Modules.Workforce.Domain.Entities;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

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
                     .Returns(Task.FromResult(1));

            var result = await _service.CreateAsync(request);

            Assert.NotNull(result);
            Assert.Equal(request.Name, result.Name);

            await _repoMock.Received(1).CreateAsync(Arg.Any<Certification>());
        }

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

            _repoMock.GetByIdAsync(id)
                     .Returns(Task.FromResult(cert));

            var result = await _service.GetByIdAsync(id);

            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal(id, result[0].Id);
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnList()
        {
            var data = new List<Certification>
    {
        new Certification { Id = Guid.NewGuid(), Name = "A" },
        new Certification { Id = Guid.NewGuid(), Name = "B" }
    };

            _repoMock.GetAllAsync()
                     .Returns(Task.FromResult(data));

            var result = await _service.GetAllAsync();

            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
        }

        [Fact]
        public async Task UpdateAsync_ShouldUpdateCertification()
        {
            var id = Guid.NewGuid();

            var cert = new Certification
            {
                Id = id,
                Name = "Old",
                Category = "Old",
                IssuingOrganization = "Old"
            };

            var request = new CertificationUpdateRequest
            {
                Name = "New",
                Category = "New",
                IssuingOrganization = "New"
            };

            _repoMock.GetByIdAsync(id)
                     .Returns(Task.FromResult(cert));

            _repoMock.UpdateAsync(cert)
                     .Returns(Task.FromResult(1));

            _userContextMock.UserId.Returns(Guid.NewGuid());
            _dateTimeMock.UtcNow.Returns(DateTime.UtcNow);

            var result = await _service.UpdateAsync(id, request);

            Assert.NotNull(result);
            Assert.Equal("New", result.Name);
            Assert.Equal("New", result.Category);

            await _repoMock.Received(1).UpdateAsync(cert);
        }

        [Fact]
        public async Task DeleteAsync_ShouldCallRepository()
        {
            var id = Guid.NewGuid();

            var cert = new Certification { Id = id };

            _repoMock.GetByIdAsync(id)
                     .Returns(Task.FromResult(cert));

            _repoMock.DeleteAsync(id)
                     .Returns(Task.FromResult(1));

            var result = await _service.DeleteAsync(id);

            Assert.Equal(1, result);

            await _repoMock.Received(1).DeleteAsync(id);
        }

        [Fact]
        public async Task GetAllPaginationAsync_ShouldReturnPaged()
        {
            var data = new List<Certification>
            {
                new Certification { Id = Guid.NewGuid() }
            };

            _repoMock.GetAllPaginationAsync(1, 10)
                     .Returns((data, 1));

            var result = await _service.GetAllPaginationAsync(1, 10);

            Assert.Equal(1, result.TotalElements);
        }

        [Fact]
        public async Task GetByCategoryAsync_ShouldReturnList()
        {
            _repoMock.GetByCategoryAsync("Cloud")
                     .Returns(new List<Certification>
                     {
                         new Certification { Id = Guid.NewGuid(), Category = "Cloud" }
                     });

            var result = await _service.GetByCategoryAsync("Cloud");

            Assert.Single(result);
        }

        [Fact]
        public async Task GetAllCategoriesAsync_ShouldReturnList()
        {
            var categories = new List<string> { "Cloud", "DevOps" };

            _repoMock.GetAllCategoriesAsync()
                     .Returns(Task.FromResult(categories));

            var result = await _service.GetAllCategoriesAsync();

            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
        }
    }
}