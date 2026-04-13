using CleanOpsAi.BuildingBlocks.Application;
using CleanOpsAi.BuildingBlocks.Application.Interfaces;
using CleanOpsAi.Modules.Workforce.Application.Dtos.Skills;
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
    public class SkillServiceTests
    {
        private readonly ISkillRepository _repoMock;
        private readonly IUserContext _userContextMock;
        private readonly IDateTimeProvider _dateTimeMock;

        private readonly SkillService _service;

        public SkillServiceTests()
        {
            _repoMock = Substitute.For<ISkillRepository>();
            _userContextMock = Substitute.For<IUserContext>();
            _dateTimeMock = Substitute.For<IDateTimeProvider>();

            _service = new SkillService(
                _repoMock,
                _userContextMock,
                _dateTimeMock
            );
        }

        // ================================
        // CREATE
        // ================================
        [Fact]
        public async Task CreateAsync_ShouldReturnSkillResponse()
        {
            var request = new SkillCreateRequest
            {
                Name = "Cleaning",
                Category = "General",
                Description = "Basic cleaning"
            };

            _userContextMock.UserId.Returns(Guid.NewGuid());
            _dateTimeMock.UtcNow.Returns(DateTime.UtcNow);

            _repoMock.CreateAsync(Arg.Any<Skill>()).Returns(1);

            var result = await _service.CreateAsync(request);

            Assert.NotNull(result);
            Assert.Equal(request.Name, result!.Name);
            Assert.Equal(request.Category, result.Category);

            await _repoMock.Received(1).CreateAsync(Arg.Any<Skill>());
        }

        // ================================
        // GET BY ID
        // ================================
        [Fact]
        public async Task GetByIdAsync_ShouldReturnSkill()
        {
            var id = Guid.NewGuid();

            var skill = new Skill
            {
                Id = id,
                Name = "Cleaning",
                Category = "General"
            };

            _repoMock.GetByIdAsync(id).Returns(skill);

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
            var data = new List<Skill>
            {
                new Skill { Id = Guid.NewGuid(), Name = "A" },
                new Skill { Id = Guid.NewGuid(), Name = "B" }
            };

            _repoMock.GetAllAsync().Returns(data);

            var result = await _service.GetAllAsync();

            Assert.Equal(2, result.Count);
        }

        // ================================
        // UPDATE
        // ================================
        [Fact]
        public async Task UpdateAsync_ShouldUpdateSkill()
        {
            var id = Guid.NewGuid();

            var skill = new Skill
            {
                Id = id,
                Name = "Old",
                Category = "OldCat"
            };

            var request = new SkillUpdateRequest
            {
                Name = "New",
                Category = "NewCat"
            };

            _repoMock.GetByIdAsync(id).Returns(skill);
            _repoMock.UpdateAsync(skill).Returns(1);

            _userContextMock.UserId.Returns(Guid.NewGuid());
            _dateTimeMock.UtcNow.Returns(DateTime.UtcNow);

            var result = await _service.UpdateAsync(id, request);

            Assert.NotNull(result);
            Assert.Equal("New", result!.Name);
            Assert.Equal("NewCat", result.Category);

            await _repoMock.Received(1).UpdateAsync(skill);
        }

        // ================================
        // UPDATE - NOT FOUND
        // ================================
        [Fact]
        public async Task UpdateAsync_ShouldReturnNull_WhenNotFound()
        {
            var id = Guid.NewGuid();

            _repoMock.GetByIdAsync(id).Returns((Skill?)null);

            var result = await _service.UpdateAsync(id, new SkillUpdateRequest());

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
        // PAGINATION
        // ================================
        [Fact]
        public async Task GetAllPaginationAsync_ShouldReturnPagedResult()
        {
            var data = new List<Skill>
            {
                new Skill { Id = Guid.NewGuid(), Name = "A" }
            };

            _repoMock.GetAllPaginationAsync(1, 10)
                     .Returns((data, 1));

            var result = await _service.GetAllPaginationAsync(1, 10);

            Assert.Single(result.Content);
            Assert.Equal(1, result.TotalElements);
        }

        // ================================
        // GET ALL CATEGORIES
        // ================================
        [Fact]
        public async Task GetAllCategoriesAsync_ShouldReturnList()
        {
            var categories = new List<string> { "General", "Advanced" };

            _repoMock.GetAllCategoriesAsync().Returns(categories);

            var result = await _service.GetAllCategoriesAsync();

            Assert.Equal(2, result.Count);
        }

        // ================================
        // GET BY CATEGORY
        // ================================
        [Fact]
        public async Task GetSkillsByCategoryAsync_ShouldReturnList()
        {
            var data = new List<Skill>
            {
                new Skill { Id = Guid.NewGuid(), Category = "General" }
            };

            _repoMock.GetByCategoryAsync("General").Returns(data);

            var result = await _service.GetSkillsByCategoryAsync("General");

            Assert.Single(result);
        }

        // ================================
        // GET BY CATEGORY - EMPTY INPUT
        // ================================
        [Fact]
        public async Task GetSkillsByCategoryAsync_ShouldReturnEmpty_WhenCategoryNull()
        {
            var result = await _service.GetSkillsByCategoryAsync("");

            Assert.Empty(result);
        }

        // ================================
        // GET SKILLS BY WORKER
        // ================================
        [Fact]
        public async Task GetSkillsByWorkerIdAsync_ShouldReturnList()
        {
            var workerId = Guid.NewGuid();

            var data = new List<WorkerSkill>
            {
                new WorkerSkill
                {
                    SkillId = Guid.NewGuid(),
                    Skill = new Skill { Name = "Cleaning", Category = "General" },
                    SkillLevel = SkillLevelType.Beginner,
                }
            };

            _repoMock.GetSkillsByWorkerIdAsync(workerId).Returns(data);

            var result = await _service.GetSkillsByWorkerIdAsync(workerId);

            Assert.Single(result);
            Assert.Equal("Cleaning", result[0].Name);
        }

        // ================================
        // GET SKILLS BY WORKER - EMPTY ID
        // ================================
        [Fact]
        public async Task GetSkillsByWorkerIdAsync_ShouldReturnEmpty_WhenIdEmpty()
        {
            var result = await _service.GetSkillsByWorkerIdAsync(Guid.Empty);

            Assert.Empty(result);
        }
    }
}
