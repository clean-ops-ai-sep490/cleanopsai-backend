using CleanOpsAi.Modules.Workforce.Application.Dtos.WorkerSkills;
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
    public class WorkerSkillServiceTests
    {
        private readonly IWorkerSkillRepository _repoMock;
        private readonly WorkerSkillService _service;

        public WorkerSkillServiceTests()
        {
            _repoMock = Substitute.For<IWorkerSkillRepository>();
            _service = new WorkerSkillService(_repoMock);
        }

        // =========================
        // GET BY ID
        // =========================
        [Fact]
        public async Task GetByIdAsync_ShouldReturnData()
        {
            var workerId = Guid.NewGuid();
            var skillId = Guid.NewGuid();

            var entity = new WorkerSkill
            {
                WorkerId = workerId,
                SkillId = skillId,
                SkillLevel = SkillLevelType.Beginner,
                Worker = new Worker { FullName = "Worker A" },
                Skill = new Skill { Name = "Cleaning" }
            };

            _repoMock.GetByIdAsync(workerId, skillId).Returns(entity);

            var result = await _service.GetByIdAsync(workerId, skillId);

            Assert.NotNull(result);
            Assert.Equal(workerId, result.WorkerId);
            Assert.Equal(skillId, result.SkillId);
            Assert.Equal("Worker A", result.WorkerName);
            Assert.Equal("Cleaning", result.SkillName);
        }

        // =========================
        // GET ALL
        // =========================
        [Fact]
        public async Task GetAllAsync_ShouldReturnList()
        {
            var list = new List<WorkerSkill>
            {
                new WorkerSkill
                {
                    WorkerId = Guid.NewGuid(),
                    SkillId = Guid.NewGuid(),
                    SkillLevel = SkillLevelType.Beginner,
                    Worker = new Worker { FullName = "A" },
                    Skill = new Skill { Name = "Skill A" }
                },
                new WorkerSkill
                {
                    WorkerId = Guid.NewGuid(),
                    SkillId = Guid.NewGuid(),
                    SkillLevel = SkillLevelType.Beginner,
                    Worker = new Worker { FullName = "B" },
                    Skill = new Skill { Name = "Skill B" }
                }
            };

            _repoMock.GetAllAsync().Returns(list);

            var result = await _service.GetAllAsync();

            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
        }

        // =========================
        // PAGINATION
        // =========================
        [Fact]
        public async Task GetAllPaginationAsync_ShouldReturnPagedData()
        {
            var data = new List<WorkerSkill>
            {
                new WorkerSkill
                {
                    WorkerId = Guid.NewGuid(),
                    SkillId = Guid.NewGuid(),
                    SkillLevel = SkillLevelType.Beginner,
                    Worker = new Worker { FullName = "A" },
                    Skill = new Skill { Name = "Skill A" }
                }
            };

            _repoMock.GetAllPaginationAsync(1, 10)
                .Returns((data, 1));

            var result = await _service.GetAllPaginationAsync(1, 10);

            Assert.NotNull(result);
            Assert.Single(result.Content);
            Assert.Equal(1, result.TotalElements);
        }

        // =========================
        // CREATE
        // =========================
        [Fact]
        public async Task CreateAsync_ShouldCreateSuccessfully()
        {
            var request = new WorkerSkillCreateRequest
            {
                WorkerId = Guid.NewGuid(),
                SkillId = Guid.NewGuid(),
                SkillLevel = SkillLevelType.Beginner,
            };

            _repoMock.CreateAsync(Arg.Any<WorkerSkill>())
                .Returns(callInfo =>
                {
                    var entity = callInfo.Arg<WorkerSkill>();

                    // 🔥 inject navigation để tránh null
                    entity.Worker = new Worker { FullName = "Test Worker" };
                    entity.Skill = new Skill { Name = "Test Skill" };

                    return Task.FromResult(1);
                });

            var result = await _service.CreateAsync(request);

            Assert.NotNull(result);
            Assert.Equal(request.WorkerId, result.WorkerId);
            Assert.Equal(request.SkillId, result.SkillId);
            Assert.Equal(request.SkillLevel, result.SkillLevel);
            Assert.Equal("Test Worker", result.WorkerName);
            Assert.Equal("Test Skill", result.SkillName);

            await _repoMock.Received(1).CreateAsync(Arg.Any<WorkerSkill>());
        }

        // =========================
        // UPDATE
        // =========================
        [Fact]
        public async Task UpdateAsync_ShouldUpdateSkillLevel()
        {
            var workerId = Guid.NewGuid();
            var skillId = Guid.NewGuid();

            var entity = new WorkerSkill
            {
                WorkerId = workerId,
                SkillId = skillId,
                SkillLevel = SkillLevelType.Beginner,
                Worker = new Worker { FullName = "A" },
                Skill = new Skill { Name = "Skill A" }
            };

            var request = new WorkerSkillUpdateRequest
            {
                SkillLevel = SkillLevelType.Beginner
            };

            _repoMock.GetByIdAsync(workerId, skillId).Returns(entity);

            _repoMock.UpdateAsync(entity)
                .Returns(Task.FromResult(1)); // 🔥 FIX

            var result = await _service.UpdateAsync(workerId, skillId, request);

            Assert.NotNull(result);
            Assert.Equal(SkillLevelType.Beginner, result.SkillLevel);

            await _repoMock.Received(1).UpdateAsync(entity);
        }

        // =========================
        // DELETE
        // =========================
        [Fact]
        public async Task DeleteAsync_ShouldDeleteSuccessfully()
        {
            var workerId = Guid.NewGuid();
            var skillId = Guid.NewGuid();

            var entity = new WorkerSkill
            {
                WorkerId = workerId,
                SkillId = skillId
            };

            _repoMock.GetByIdAsync(workerId, skillId).Returns(entity);
            _repoMock.DeleteAsync(workerId, skillId).Returns(1);

            var result = await _service.DeleteAsync(workerId, skillId);

            Assert.Equal(1, result);
            await _repoMock.Received(1).DeleteAsync(workerId, skillId);
        }

        // =========================
        // DELETE NOT FOUND
        // =========================
        [Fact]
        public async Task DeleteAsync_NotFound_ShouldReturnZero()
        {
            var workerId = Guid.NewGuid();
            var skillId = Guid.NewGuid();

            _repoMock.GetByIdAsync(workerId, skillId).Returns((WorkerSkill)null);

            var result = await _service.DeleteAsync(workerId, skillId);

            Assert.Equal(0, result);
        }
    }
}
