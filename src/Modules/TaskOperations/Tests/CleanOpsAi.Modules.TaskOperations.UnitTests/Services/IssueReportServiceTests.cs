using AutoMapper;
using CleanOpsAi.BuildingBlocks.Application;
using CleanOpsAi.BuildingBlocks.Application.Interfaces;
using CleanOpsAi.BuildingBlocks.Application.Pagination;
using CleanOpsAi.Modules.TaskOperations.Application.Common.Interfaces.Repositories;
using CleanOpsAi.Modules.TaskOperations.Application.Common.Interfaces.Services;
using CleanOpsAi.Modules.TaskOperations.Application.DTOs.Request;
using CleanOpsAi.Modules.TaskOperations.Application.DTOs.Response;
using CleanOpsAi.Modules.TaskOperations.Application.Services;
using CleanOpsAi.Modules.TaskOperations.Domain.Entities;
using CleanOpsAi.Modules.TaskOperations.Domain.Enums;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanOpsAi.Modules.TaskOperations.UnitTests.Services
{
    public class IssueReportServiceTests
    {
        private readonly IIssueReportRepository _repo;
        private readonly IMapper _mapper;
        private readonly IUserContext _userContext;
        private readonly IDateTimeProvider _dateTime;
        private readonly IWorkerQueryService _workerService;

        private readonly IssueReportService _service;

        public IssueReportServiceTests()
        {
            _repo = Substitute.For<IIssueReportRepository>();
            _mapper = Substitute.For<IMapper>();
            _userContext = Substitute.For<IUserContext>();
            _dateTime = Substitute.For<IDateTimeProvider>();
            _workerService = Substitute.For<IWorkerQueryService>();

            _service = new IssueReportService(
                _repo,
                _mapper,
                _userContext,
                _dateTime,
                _workerService
            );
        }

        // =========================
        // GET BY ID
        // =========================
        [Fact]
        public async Task GetById_ShouldReturnDto_WithWorkerName()
        {
            var workerId = Guid.NewGuid();

            var entity = new IssueReport
            {
                Id = Guid.NewGuid(),
                ReportedByWorkerId = workerId
            };

            _repo.GetByIdExistAsync(entity.Id, default).Returns(entity);

            var dto = new IssueReportDto
            {
                Id = entity.Id,
                ReportedByWorkerId = workerId
            };

            _mapper.Map<IssueReportDto>(entity).Returns(dto);

            _workerService.GetUserNames(Arg.Any<List<Guid>>())
                .Returns(new Dictionary<Guid, string>
                {
                    { workerId, "Worker A" }
                });

            var result = await _service.GetById(entity.Id);

            Assert.NotNull(result);
            Assert.Equal("Worker A", result.ReportedByWorkerName);
        }

        // =========================
        // CREATE
        // =========================
        [Fact]
        public async Task Create_ShouldReturnDto()
        {
            var userId = Guid.NewGuid();
            _userContext.UserId.Returns(userId);
            _dateTime.UtcNow.Returns(DateTime.UtcNow);

            var workerId = Guid.NewGuid();

            var request = new CreateIssueReportDto
            {
                ReportedByWorkerId = workerId,
                Description = "Test issue"
            };

            var entity = new IssueReport
            {
                Id = Guid.NewGuid(),
                ReportedByWorkerId = workerId
            };

            _mapper.Map<IssueReport>(request).Returns(entity);

            var dto = new IssueReportDto
            {
                Id = entity.Id,
                ReportedByWorkerId = workerId
            };

            _mapper.Map<IssueReportDto>(entity).Returns(dto);

            _workerService.GetUserNames(Arg.Any<List<Guid>>())
                .Returns(new Dictionary<Guid, string>
                {
                    { workerId, "Worker A" }
                });

            var result = await _service.Create(request);

            Assert.NotNull(result);
            Assert.Equal("Worker A", result.ReportedByWorkerName);

            await _repo.Received(1).AddAsync(entity, default);
        }

        // =========================
        // UPDATE
        // =========================
        [Fact]
        public async Task Update_ShouldUpdateEntity()
        {
            var workerId = Guid.NewGuid();

            var entity = new IssueReport
            {
                Id = Guid.NewGuid(),
                ReportedByWorkerId = workerId
            };

            _repo.GetByIdExistAsync(entity.Id, default).Returns(entity);

            _userContext.UserId.Returns(Guid.NewGuid());
            _dateTime.UtcNow.Returns(DateTime.UtcNow);

            var request = new UpdateIssueReportDto
            {
                Description = "Updated"
            };

            _mapper.Map(request, entity);

            _mapper.Map<IssueReportDto>(entity)
                .Returns(new IssueReportDto
                {
                    Id = entity.Id,
                    ReportedByWorkerId = workerId
                });

            _workerService.GetUserNames(Arg.Any<List<Guid>>())
                .Returns(new Dictionary<Guid, string>());

            var result = await _service.Update(entity.Id, request);

            Assert.NotNull(result);

            await _repo.Received(1).UpdateAsync(entity, default);
        }

        // =========================
        // RESOLVE
        // =========================
        [Fact]
        public async Task Resolve_ShouldSetApproved()
        {
            var workerId = Guid.NewGuid();

            var entity = new IssueReport
            {
                Id = Guid.NewGuid(),
                ReportedByWorkerId = workerId
            };

            _repo.GetByIdExistAsync(entity.Id, default).Returns(entity);

            _userContext.UserId.Returns(Guid.NewGuid());
            _userContext.FullName.Returns("Supervisor A");
            _dateTime.UtcNow.Returns(DateTime.UtcNow);

            var request = new ResolveIssueReportDto
            {
                Status = IssueStatus.Approved
            };

            _mapper.Map<IssueReportDto>(entity)
                .Returns(new IssueReportDto
                {
                    Id = entity.Id,
                    ReportedByWorkerId = workerId
                });

            _workerService.GetUserNames(Arg.Any<List<Guid>>())
                .Returns(new Dictionary<Guid, string>());

            var result = await _service.Resolve(entity.Id, request);

            Assert.NotNull(result);
            Assert.Equal(IssueStatus.Approved, entity.Status);
            Assert.Equal("Supervisor A", result.ResolvedByUserName);

            await _repo.Received(1).UpdateAsync(entity, default);
        }

        // =========================
        // DELETE
        // =========================
        [Fact]
        public async Task Delete_ShouldReturnTrue()
        {
            var entity = new IssueReport
            {
                Id = Guid.NewGuid()
            };

            _repo.GetByIdExistAsync(entity.Id, default).Returns(entity);

            var result = await _service.Delete(entity.Id);

            Assert.True(result);

            await _repo.Received(1).DeleteAsync(entity, default);
        }

        // =========================
        // GET LIST (PAGING + ENRICH)
        // =========================
        [Fact]
        public async Task Gets_ShouldReturnPagedResult()
        {
            var workerId = Guid.NewGuid();

            var entities = new List<IssueReport>
            {
                new IssueReport { Id = Guid.NewGuid(), ReportedByWorkerId = workerId }
            };

            var paged = new PaginatedResult<IssueReport>(
                1, 10, 1, entities
            );

            _repo.GetsPagingAsync(Arg.Any<PaginationRequest>(), default)
                .Returns(paged);

            //  dùng Arg.Any để tránh null mapper
            _mapper.Map<List<IssueReportDto>>(Arg.Any<List<IssueReport>>())
                .Returns(new List<IssueReportDto>
                {
                    new IssueReportDto { ReportedByWorkerId = workerId }
                });

            _workerService.GetUserNames(Arg.Any<List<Guid>>())
                .Returns(new Dictionary<Guid, string>
                {
                    { workerId, "Worker A" }
                });

            var result = await _service.Gets(new PaginationRequest());

            Assert.NotNull(result);
            Assert.Single(result.Content);
            Assert.Equal("Worker A", result.Content[0].ReportedByWorkerName);
        }
    }
}
