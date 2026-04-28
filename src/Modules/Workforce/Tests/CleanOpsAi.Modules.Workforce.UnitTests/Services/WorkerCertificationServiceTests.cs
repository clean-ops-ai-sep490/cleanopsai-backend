using CleanOpsAi.BuildingBlocks.Application;
using CleanOpsAi.Modules.Workforce.Application.Dtos.WorkerCertifications;
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
    public class WorkerCertificationServiceTests
    {
        private readonly IWorkerCertificationRepository _repoMock;
        private readonly IUserContext _userContextMock;

        private readonly WorkerCertificationService _service;

        public WorkerCertificationServiceTests()
        {
            _repoMock = Substitute.For<IWorkerCertificationRepository>();
            _userContextMock = Substitute.For<IUserContext>();

            _service = new WorkerCertificationService(
                _repoMock,
                _userContextMock
            );
        }

        // ================================
        // GET BY ID
        // ================================
        [Fact]
        public async Task GetByIdAsync_ShouldReturnData()
        {
            var workerId = Guid.NewGuid();
            var certId = Guid.NewGuid();

            var entity = new WorkerCertification
            {
                WorkerId = workerId,
                CertificationId = certId,
                Worker = new Worker { FullName = "John" },
                Certification = new Certification { Name = "AWS" }
            };

            _repoMock.GetByIdAsync(workerId, certId).Returns(entity);

            var result = await _service.GetByIdAsync(workerId, certId);

            Assert.NotNull(result);
            Assert.Equal("John", result!.WorkerName);
            Assert.Equal("AWS", result.CertificationName);
        }

        // ================================
        // GET ALL
        // ================================
        [Fact]
        public async Task GetAllAsync_ShouldReturnList()
        {
            var data = new List<WorkerCertification>
            {
                new WorkerCertification
                {
                    WorkerId = Guid.NewGuid(),
                    CertificationId = Guid.NewGuid(),
                    Worker = new Worker { FullName = "A" },
                    Certification = new Certification { Name = "CertA" }
                }
            };

            _repoMock.GetAllAsync().Returns(data);

            var result = await _service.GetAllAsync();

            Assert.Single(result);
        }

        // ================================
        // PAGINATION
        // ================================
        [Fact]
        public async Task GetAllPaginationAsync_ShouldReturnPaged()
        {
            var data = new List<WorkerCertification>
            {
                new WorkerCertification
                {
                    WorkerId = Guid.NewGuid(),
                    CertificationId = Guid.NewGuid(),
                    Worker = new Worker { FullName = "A" },
                    Certification = new Certification { Name = "CertA" }
                }
            };

            _repoMock.GetAllPaginationAsync(1, 10)
                     .Returns((data, 1));

            var result = await _service.GetAllPaginationAsync(1, 10);

            Assert.Single(result.Content);
            Assert.Equal(1, result.TotalElements);
        }

        // ================================
        // CREATE
        // ================================
        [Fact]
        public async Task CreateAsync_ShouldCreateSuccessfully()
        {
            var request = new WorkerCertificationCreateRequest
            {
                WorkerId = Guid.NewGuid(),
                CertificationId = Guid.NewGuid(),
                IssuedDate = DateTime.UtcNow
            };

            _repoMock.CreateAsync(Arg.Any<WorkerCertification>())
                     .Returns(1);

            var result = await _service.CreateAsync(request);

            Assert.NotNull(result);
            Assert.Equal(request.WorkerId, result!.WorkerId);
            Assert.Equal(request.CertificationId, result.CertificationId);

            await _repoMock.Received(1).CreateAsync(Arg.Any<WorkerCertification>());
        }

        // ================================
        // UPDATE
        // ================================
        [Fact]
        public async Task UpdateAsync_ShouldUpdateSuccessfully()
        {
            var workerId = Guid.NewGuid();
            var certId = Guid.NewGuid();

            var entity = new WorkerCertification
            {
                WorkerId = workerId,
                CertificationId = certId,
                Worker = new Worker { FullName = "John" },
                Certification = new Certification { Name = "AWS" }
            };

            var request = new WorkerCertificationUpdateRequest
            {
                IssuedDate = DateTime.UtcNow,
                ExpiredAt = DateTime.UtcNow.AddYears(1)
            };

            _repoMock.GetByIdAsync(workerId, certId).Returns(entity);
            _repoMock.UpdateAsync(entity).Returns(1);

            var result = await _service.UpdateAsync(workerId, certId, request);

            Assert.NotNull(result);
            Assert.Equal("John", result!.WorkerName);

            await _repoMock.Received(1).UpdateAsync(entity);
        }

        // ================================
        // UPDATE - NOT FOUND
        // ================================
        [Fact]
        public async Task UpdateAsync_ShouldReturnNull_WhenNotFound()
        {
            var workerId = Guid.NewGuid();
            var certId = Guid.NewGuid();

            _repoMock.GetByIdAsync(workerId, certId)
                     .Returns((WorkerCertification?)null);

            var result = await _service.UpdateAsync(workerId, certId, new WorkerCertificationUpdateRequest());

            Assert.Null(result);
        }

        // ================================
        // DELETE
        // ================================
        [Fact]
        public async Task DeleteAsync_ShouldDelete()
        {
            var workerId = Guid.NewGuid();
            var certId = Guid.NewGuid();

            var entity = new WorkerCertification
            {
                WorkerId = workerId,
                CertificationId = certId
            };

            _repoMock.GetByIdAsync(workerId, certId).Returns(entity);
            _repoMock.DeleteAsync(workerId, certId).Returns(1);

            var result = await _service.DeleteAsync(workerId, certId);

            Assert.Equal(1, result);
        }

        // ================================
        // DELETE - NOT FOUND
        // ================================
        [Fact]
        public async Task DeleteAsync_ShouldReturnZero_WhenNotFound()
        {
            var workerId = Guid.NewGuid();
            var certId = Guid.NewGuid();

            _repoMock.GetByIdAsync(workerId, certId)
                     .Returns((WorkerCertification?)null);

            var result = await _service.DeleteAsync(workerId, certId);

            Assert.Equal(0, result);
        }
    }
}
