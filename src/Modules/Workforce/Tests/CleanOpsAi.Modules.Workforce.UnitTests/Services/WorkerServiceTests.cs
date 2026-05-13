using CleanOpsAi.BuildingBlocks.Application;
using CleanOpsAi.BuildingBlocks.Application.Exceptions;
using CleanOpsAi.BuildingBlocks.Application.Interfaces;
using CleanOpsAi.BuildingBlocks.Infrastructure.Events.Request;
using CleanOpsAi.BuildingBlocks.Infrastructure.Events.Response;
using CleanOpsAi.Modules.Workforce.Application.Dtos.Nlps;
using CleanOpsAi.Modules.Workforce.Application.Dtos.Workers;
using CleanOpsAi.Modules.Workforce.Application.Interfaces;
using CleanOpsAi.Modules.Workforce.Application.Services;
using CleanOpsAi.Modules.Workforce.Domain.Entities;
using MassTransit;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanOpsAi.Modules.Workforce.UnitTests.Services
{
    public class WorkerServiceTests
    {
        private readonly IWorkerRepository _repoMock;
        private readonly IFileStorageService _fileMock;
        private readonly IUserContext _userContextMock;
        private readonly IDateTimeProvider _dateTimeMock;
        private readonly IGoongMapService _goongMock;
        private readonly IRequestClient<GetBusyWorkerIdsRequest> _busMock;
        private readonly IGeminiService _geminiMock;
        private readonly ISkillRepository _skillRepoMock;
        private readonly ICertificationRepository _certificationRepoMock;

        private readonly WorkerService _service;

        public WorkerServiceTests()
        {
            _repoMock = Substitute.For<IWorkerRepository>();
            _fileMock = Substitute.For<IFileStorageService>();
            _userContextMock = Substitute.For<IUserContext>();
            _dateTimeMock = Substitute.For<IDateTimeProvider>();
            _goongMock = Substitute.For<IGoongMapService>();
            _busMock = Substitute.For<IRequestClient<GetBusyWorkerIdsRequest>>();
            _geminiMock = Substitute.For<IGeminiService>();
            _skillRepoMock = Substitute.For<ISkillRepository>();
            _certificationRepoMock = Substitute.For<ICertificationRepository>();

            _service = new WorkerService(
                _repoMock,
                _fileMock,
                _userContextMock,
                _dateTimeMock,
                _goongMock,
                _busMock,
                _geminiMock,
                _skillRepoMock,
                _certificationRepoMock
            );
        }

        // ================================
        // CREATE
        // ================================
        [Fact]
        public async Task CreateAsync_ShouldReturnWorker()
        {
            var request = new WorkerCreateRequest
            {
                UserId = Guid.NewGuid(),
                FullName = "Test"
            };

            _userContextMock.UserId.Returns(Guid.NewGuid());
            _dateTimeMock.UtcNow.Returns(DateTime.UtcNow);

            _repoMock.CreateAsync(Arg.Any<Worker>()).Returns(1);

            var result = await _service.CreateAsync(request);

            Assert.NotNull(result);
            Assert.Equal(request.FullName, result.FullName);
        }

        // ================================
        // UPDATE - NOT FOUND
        // ================================
        [Fact]
        public async Task UpdateAsync_ShouldThrow_WhenNotFound()
        {
            var id = Guid.NewGuid();

            _repoMock.GetByIdAsync(id).Returns((Worker?)null);

            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _service.UpdateAsync(id, new WorkerUpdateRequest()));
        }

        // ================================
        // UPDATE - GEO + AVATAR
        // ================================
        [Fact]
        public async Task UpdateAsync_ShouldUpdateGeoAndAvatar()
        {
            var id = Guid.NewGuid();

            var worker = new Worker
            {
                Id = id,
                WorkerSkills = new List<WorkerSkill>(),
                WorkerCertifications = new List<WorkerCertification>()
            };

            var request = new WorkerUpdateRequest
            {
                DisplayAddress = "HCM",
                AvatarStream = new MemoryStream(),
                AvatarFileName = "a.png"
            };

            _repoMock.GetByIdAsync(id).Returns(worker);

            _goongMock.GetCoordinatesAsync("HCM")
                .Returns((10.0, 20.0));

            _fileMock.UploadFileAsync(
                Arg.Any<Stream>(),
                Arg.Any<string>(),
                Arg.Any<string>())
                .Returns("url");

            _userContextMock.UserId.Returns(Guid.NewGuid());
            _dateTimeMock.UtcNow.Returns(DateTime.UtcNow);

            var result = await _service.UpdateAsync(id, request);

            Assert.Equal(10.0, result!.Latitude);
            Assert.Equal(20.0, result.Longitude);
            Assert.Equal("url", result.AvatarUrl);
        }

        // ================================
        // DELETE
        // ================================
        [Fact]
        public async Task DeleteAsync_ShouldDelete()
        {
            var id = Guid.NewGuid();

            _repoMock.GetByIdAsync(id).Returns(new Worker { Id = id });
            _repoMock.DeleteAsync(id).Returns(1);

            var result = await _service.DeleteAsync(id);

            Assert.Equal(1, result);
        }

        // ================================
        // FILTER - INVALID TIME
        // ================================
        [Fact]
        public async Task FilterAsync_ShouldThrow_WhenInvalidTime()
        {
            var request = new WorkerFilterRequest
            {
                StartAt = DateTime.UtcNow,
                EndAt = DateTime.UtcNow.AddDays(-1)
            };

            await Assert.ThrowsAsync<BadRequestException>(() =>
                _service.FilterAsync(request));
        }

        // ================================
        // FILTER - REMOVE BUSY
        // ================================
        [Fact]
        public async Task FilterAsync_ShouldExcludeBusyWorkers()
        {
            var workerId = Guid.NewGuid();

            var workers = new List<Worker>
            {
                new Worker
                {
                    Id = workerId,
                    WorkerSkills = new List<WorkerSkill>(),
                    WorkerCertifications = new List<WorkerCertification>()
                }
            };

            var request = new WorkerFilterRequest
            {
                StartAt = DateTime.UtcNow,
                EndAt = DateTime.UtcNow.AddDays(1)
            };

            // mock bus
            var response = Substitute.For<Response<GetBusyWorkerIdsResponse>>();
            response.Message.Returns(new GetBusyWorkerIdsResponse
            {
                BusyWorkerIds = new List<Guid> { workerId }
            });

            _busMock.GetResponse<GetBusyWorkerIdsResponse>(Arg.Any<GetBusyWorkerIdsRequest>())
                .Returns(Task.FromResult(response));

            _repoMock.FilterAsync(request).Returns(workers);

            var result = await _service.FilterAsync(request);

            Assert.Empty(result); // bị loại vì busy
        }

        // ================================
        // NLP FILTER
        // ================================
        //[Fact]
        //public async Task NlpFilterAsync_ShouldResolveSkillAndCertificationNames()
        //{
        //    var skillId = Guid.NewGuid();
        //    var certificationId = Guid.NewGuid();

        //    _geminiMock.ParseWorkerFilterAsync(Arg.Any<string>())
        //        .Returns(new WorkerFilterNlpResult
        //        {
        //            Address = "Phường Ba Đình",
        //            SkillCategories = new List<string> { "chemical" },
        //            CertificateCategories = new List<string> { "management" }
        //        });

        //    _skillRepoMock.GetAllAsync()
        //        .Returns(new List<Skill>
        //        {
        //            new Skill
        //            {
        //                Id = skillId,
        //                Name = "Chemical Cleaning",
        //                Category = "Chemical"
        //            }
        //        });

        //    _certificationRepoMock.GetAllAsync()
        //        .Returns(new List<Certification>
        //        {
        //            new Certification
        //            {
        //                Id = certificationId,
        //                Name = "Safety Management",
        //                Category = "Management",
        //                IssuingOrganization = "CleanOps"
        //            }
        //        });

        //    _goongMock.GetCoordinatesAsync("phường ba đình")
        //        .Returns((10.0, 20.0));

        //    _repoMock.FilterAsync(Arg.Any<WorkerFilterRequest>())
        //        .Returns(new List<Worker>());

        //    await _service.NlpFilterAsync("Tìm người có skill chemical có chứng chỉ management ở Phường Ba Đình");

        //    await _repoMock.Received(1).FilterAsync(Arg.Is<WorkerFilterRequest>(request =>
        //        request.SkillIds != null &&
        //        request.SkillIds.Contains(skillId) &&
        //        request.CertificateIds != null &&
        //        request.CertificateIds.Contains(certificationId) &&
        //        request.Latitude == 10.0 &&
        //        request.Longitude == 20.0));
        //}

        //[Fact]
        //public async Task NlpFilterAsync_ShouldInferSkillFromNaturalSentenceWithoutSkillKeyword()
        //{
        //    var skillId = Guid.NewGuid();

        //    _geminiMock.ParseWorkerFilterAsync(Arg.Any<string>())
        //        .Returns(new WorkerFilterNlpResult());

        //    _skillRepoMock.GetAllAsync()
        //        .Returns(new List<Skill>
        //        {
        //            new Skill
        //            {
        //                Id = skillId,
        //                Name = "Chemical Cleaning",
        //                Category = "Chemical"
        //            }
        //        });

        //    _certificationRepoMock.GetAllAsync()
        //        .Returns(new List<Certification>());

        //    _repoMock.FilterAsync(Arg.Any<WorkerFilterRequest>())
        //        .Returns(new List<Worker>());

        //    await _service.NlpFilterAsync("Ai lam duoc chemical cleaning");

        //    await _repoMock.Received(1).FilterAsync(Arg.Is<WorkerFilterRequest>(request =>
        //        request.SkillIds != null &&
        //        request.SkillIds.Contains(skillId)));
        //}

        // ================================
        // GET BY IDS
        // ================================
        [Fact]
        public async Task GetWorkersByIds_ShouldReturnList()
        {
            var data = new List<Worker>
            {
                new Worker { Id = Guid.NewGuid(), FullName = "A" }
            };

            _repoMock.GetWorkersByIds(Arg.Any<List<Guid>>())
                     .Returns(data);

            var result = await _service.GetWorkersByIds(new List<Guid> { Guid.NewGuid() });

            Assert.Single(result);
            Assert.Equal("A", result[0].FullName);
        }

        [Fact]
        public async Task GetWorkersByUserIds_ShouldReturnWorkerSummaries()
        {
            var userId = Guid.NewGuid();
            var workerId = Guid.NewGuid();

            _repoMock.GetWorkersByUserIds(Arg.Any<List<Guid>>())
                .Returns(new List<Worker>
                {
                    new Worker
                    {
                        Id = workerId,
                        UserId = userId,
                        FullName = "Worker Lookup"
                    }
                });

            var result = await _service.GetWorkersByUserIds(new List<Guid> { userId });

            Assert.Single(result);
            Assert.Equal(userId, result[0].UserId);
            Assert.Equal(workerId, result[0].WorkerId);
            Assert.Equal("Worker Lookup", result[0].FullName);
        }

        // ================================
        // QUALIFIED
        // ================================
        [Fact]
        public async Task IsWorkerQualifiedAsync_ShouldReturnTrue()
        {
            _repoMock.IsWorkerQualifiedAsync(
                Arg.Any<Guid>(),
                Arg.Any<List<Guid>>(),
                Arg.Any<List<Guid>>(),
                Arg.Any<CancellationToken>())
                .Returns(true);

            var result = await _service.IsWorkerQualifiedAsync(
                Guid.NewGuid(),
                new List<Guid>(),
                new List<Guid>());

            Assert.True(result);
        }
    }
}
