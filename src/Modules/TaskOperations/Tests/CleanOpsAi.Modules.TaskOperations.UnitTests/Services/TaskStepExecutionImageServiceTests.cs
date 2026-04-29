using CleanOpsAi.BuildingBlocks.Application;
using CleanOpsAi.BuildingBlocks.Application.Exceptions;
using CleanOpsAi.BuildingBlocks.Application.Interfaces;
using CleanOpsAi.Modules.TaskOperations.Application.Common.Interfaces.Repositories;
using CleanOpsAi.Modules.TaskOperations.Application.Common.Interfaces.Services;
using CleanOpsAi.Modules.TaskOperations.Application.DTOs.Request;
using CleanOpsAi.Modules.TaskOperations.Application.DTOs.Response;
using CleanOpsAi.Modules.TaskOperations.Application.Services;
using CleanOpsAi.Modules.TaskOperations.Domain.Entities;
using CleanOpsAi.Modules.TaskOperations.Domain.Enums;
using Microsoft.AspNetCore.Http;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace CleanOpsAi.Modules.TaskOperations.UnitTests.Services
{
    public class TaskStepExecutionImageServiceTests
    {
        private readonly ITaskAssignmentRepository _assignmentRepo;
        private readonly ITaskStepExecutionRepository _stepRepo;
        private readonly ITaskStepExecutionImageRepository _imageRepo;
        private readonly IFileStorageService _storageService;

        private readonly TaskStepExecutionImageService _service;
        private readonly IUserContext _userContext;
        private readonly IDateTimeProvider _dateTime;
		private readonly IIdGenerator _idGenerator;

		public TaskStepExecutionImageServiceTests()
        {
            _assignmentRepo = Substitute.For<ITaskAssignmentRepository>();
            _stepRepo = Substitute.For<ITaskStepExecutionRepository>();
            _imageRepo = Substitute.For<ITaskStepExecutionImageRepository>();
            _storageService = Substitute.For<IFileStorageService>();
            _userContext = Substitute.For<IUserContext>();
            _dateTime = Substitute.For<IDateTimeProvider>();
			_idGenerator = Substitute.For<IIdGenerator>();

            _service = new TaskStepExecutionImageService(
                _assignmentRepo,
                _stepRepo,
                _imageRepo,
                _storageService,
                _userContext,
                _dateTime,
				_idGenerator
			);
        }

        // =========================
        // UPLOAD IMAGES SUCCESS
        // =========================
        [Fact]
        public async Task UploadImagesAsync_ShouldUploadImages_WhenValid()
        {
            var stepId = Guid.NewGuid();
            var imageType = ImageType.Before;

            var step = new TaskStepExecution
            {
                Id = stepId,
                Status = TaskStepExecutionStatus.InProgress,
                ResultData = "{}"
            };

            _stepRepo.GetByIdAsync(stepId, default)
                .Returns(step);

            var mockFile1 = Substitute.For<IFormFile>();
            mockFile1.FileName.Returns("image1.jpg");
            mockFile1.OpenReadStream().Returns(new MemoryStream(new byte[] { 1, 2, 3 }));

            var mockFile2 = Substitute.For<IFormFile>();
            mockFile2.FileName.Returns("image2.jpg");
            mockFile2.OpenReadStream().Returns(new MemoryStream(new byte[] { 4, 5, 6 }));

            var request = new UploadStepImagesRequest
            {
                Images = new List<IFormFile> { mockFile1, mockFile2 },
                MinPhotos = 2
            };

            var uploadedUrls = new List<string> 
            { 
                "https://storage.com/before/image1.jpg",
                "https://storage.com/before/image2.jpg"
            };

            _storageService.UploadFilesAsync(Arg.Any<List<(Stream, string)>>(), "contracts", default)
                .Returns(uploadedUrls);

            var result = await _service.UploadImagesAsync(stepId, imageType, request);

            Assert.NotNull(result);
            Assert.Equal("before", result.Phase);
            Assert.Equal(2, result.MinPhotos);
            Assert.Equal(2, result.ActualPhotos);
            await _imageRepo.Received(1).AddRangeAsync(Arg.Any<List<TaskStepExecutionImage>>(), default);
            await _imageRepo.Received(1).SaveChangesAsync(default);
        }

        // =========================
        // UPLOAD IMAGES - AFTER PHASE
        // =========================
        [Fact]
        public async Task UploadImagesAsync_ShouldHandleAfterPhase()
        {
            var stepId = Guid.NewGuid();
            var imageType = ImageType.After;

            var step = new TaskStepExecution
            {
                Id = stepId,
                Status = TaskStepExecutionStatus.InProgress,
                ResultData = "{}"
            };

            _stepRepo.GetByIdAsync(stepId, default)
                .Returns(step);

            var mockFile = Substitute.For<IFormFile>();
            mockFile.FileName.Returns("after.jpg");
            mockFile.OpenReadStream().Returns(new MemoryStream(new byte[] { 1, 2, 3 }));

            var request = new UploadStepImagesRequest
            {
                Images = new List<IFormFile> { mockFile },
                MinPhotos = 1
            };

            var uploadedUrls = new List<string> { "https://storage.com/after/after.jpg" };

            _storageService.UploadFilesAsync(Arg.Any<List<(Stream, string)>>(), "contracts", default)
                .Returns(uploadedUrls);

            var result = await _service.UploadImagesAsync(stepId, imageType, request);

            Assert.Equal("after", result.Phase);
        }

        // =========================
        // UPLOAD IMAGES - PPE PHASE
        // =========================
        [Fact]
        public async Task UploadImagesAsync_ShouldHandlePpePhase()
        {
            var stepId = Guid.NewGuid();
            var imageType = ImageType.Ppe;

            var step = new TaskStepExecution
            {
                Id = stepId,
                Status = TaskStepExecutionStatus.InProgress,
                ResultData = "{}"
            };

            _stepRepo.GetByIdAsync(stepId, default)
                .Returns(step);

            var mockFile = Substitute.For<IFormFile>();
            mockFile.FileName.Returns("ppe.jpg");
            mockFile.OpenReadStream().Returns(new MemoryStream(new byte[] { 1, 2, 3 }));

            var request = new UploadStepImagesRequest
            {
                Images = new List<IFormFile> { mockFile },
                MinPhotos = 1
            };

            var uploadedUrls = new List<string> { "https://storage.com/ppe/ppe.jpg" };

            _storageService.UploadFilesAsync(Arg.Any<List<(Stream, string)>>(), "contracts", default)
                .Returns(uploadedUrls);

            var result = await _service.UploadImagesAsync(stepId, imageType, request);

            Assert.Equal("ppe", result.Phase);
        }

        // =========================
        // UPLOAD IMAGES FAIL - STEP NOT FOUND
        // =========================
        [Fact]
        public async Task UploadImagesAsync_ShouldThrow_WhenStepNotFound()
        {
            var stepId = Guid.NewGuid();

            _stepRepo.GetByIdAsync(stepId, default)
                .Returns((TaskStepExecution)null);

            var request = new UploadStepImagesRequest
            {
                Images = new List<IFormFile>()
            };

            await Assert.ThrowsAsync<NotFoundException>(() =>
                _service.UploadImagesAsync(stepId, ImageType.Before, request));
        }

        // =========================
        // UPLOAD IMAGES FAIL - STEP NOT IN PROGRESS
        // =========================
        [Fact]
        public async Task UploadImagesAsync_ShouldThrow_WhenStepNotInProgress()
        {
            var stepId = Guid.NewGuid();

            var step = new TaskStepExecution
            {
                Id = stepId,
                Status = TaskStepExecutionStatus.Completed
            };

            _stepRepo.GetByIdAsync(stepId, default)
                .Returns(step);

            var request = new UploadStepImagesRequest
            {
                Images = new List<IFormFile>()
            };

            await Assert.ThrowsAsync<BadRequestException>(() =>
                _service.UploadImagesAsync(stepId, ImageType.Before, request));
        }

        // =========================
        // UPLOAD IMAGES FAIL - NO IMAGES PROVIDED
        // =========================
        [Fact]
        public async Task UploadImagesAsync_ShouldThrow_WhenNoImagesProvided()
        {
            var stepId = Guid.NewGuid();

            var step = new TaskStepExecution
            {
                Id = stepId,
                Status = TaskStepExecutionStatus.InProgress
            };

            _stepRepo.GetByIdAsync(stepId, default)
                .Returns(step);

            var request = new UploadStepImagesRequest
            {
                Images = new List<IFormFile>()
            };

            await Assert.ThrowsAsync<BadRequestException>(() =>
                _service.UploadImagesAsync(stepId, ImageType.Before, request));
        }

        // =========================
        // REUPLOAD IMAGES SUCCESS
        // =========================
        [Fact]
        public async Task ReUploadImagesAsync_ShouldSoftDeleteOldAndUploadNew()
        {
            var stepId = Guid.NewGuid();
            var imageType = ImageType.Before;

            var step = new TaskStepExecution
            {
                Id = stepId,
                Status = TaskStepExecutionStatus.InProgress,
                ResultData = "{}"
            };

            _stepRepo.GetByIdAsync(stepId, default)
                .Returns(step);

            var oldImage1 = new TaskStepExecutionImage
            {
                Id = Guid.NewGuid(),
                TaskStepExecutionId = stepId,
                ImageUrl = "https://storage.com/before/old1.jpg",
                IsDeleted = false
            };

            var oldImage2 = new TaskStepExecutionImage
            {
                Id = Guid.NewGuid(),
                TaskStepExecutionId = stepId,
                ImageUrl = "https://storage.com/before/old2.jpg",
                IsDeleted = false
            };

            var existingImages = new List<TaskStepExecutionImage> { oldImage1, oldImage2 };

            _imageRepo.GetActiveByExecutionIdAndTypeAsync(stepId, default)
                .Returns(existingImages);

            var mockFile = Substitute.For<IFormFile>();
            mockFile.FileName.Returns("new_image.jpg");
            mockFile.OpenReadStream().Returns(new MemoryStream(new byte[] { 1, 2, 3 }));

            var request = new UploadStepImagesRequest
            {
                Images = new List<IFormFile> { mockFile },
                MinPhotos = 1
            };

            var uploadedUrls = new List<string> { "https://storage.com/before/new_image.jpg" };

            _storageService.UploadFilesAsync(Arg.Any<List<(Stream, string)>>(), "contracts", default)
                .Returns(uploadedUrls);

            var result = await _service.ReUploadImagesAsync(stepId, imageType, request);

            Assert.NotNull(result);
            Assert.Equal(1, result.ActualPhotos);
            Assert.True(oldImage1.IsDeleted);
            Assert.True(oldImage2.IsDeleted);
            await _imageRepo.Received(1).AddRangeAsync(Arg.Any<List<TaskStepExecutionImage>>(), default);
            await _imageRepo.Received(1).SaveChangesAsync(default);
        }

        // =========================
        // REUPLOAD IMAGES FAIL - STEP NOT FOUND
        // =========================
        [Fact]
        public async Task ReUploadImagesAsync_ShouldThrow_WhenStepNotFound()
        {
            var stepId = Guid.NewGuid();

            _stepRepo.GetByIdAsync(stepId, default)
                .Returns((TaskStepExecution)null);

            var request = new UploadStepImagesRequest
            {
                Images = new List<IFormFile>()
            };

            await Assert.ThrowsAsync<NotFoundException>(() =>
                _service.ReUploadImagesAsync(stepId, ImageType.Before, request));
        }

        // =========================
        // REUPLOAD IMAGES FAIL - NO IMAGES PROVIDED
        // =========================
        [Fact]
        public async Task ReUploadImagesAsync_ShouldThrow_WhenNoImagesProvided()
        {
            var stepId = Guid.NewGuid();

            var step = new TaskStepExecution
            {
                Id = stepId,
                Status = TaskStepExecutionStatus.InProgress
            };

            _stepRepo.GetByIdAsync(stepId, default)
                .Returns(step);

            var request = new UploadStepImagesRequest
            {
                Images = new List<IFormFile>()
            };

            await Assert.ThrowsAsync<BadRequestException>(() =>
                _service.ReUploadImagesAsync(stepId, ImageType.Before, request));
        }

        // =========================
        // GET IMAGES BY ASSIGNMENT ID SUCCESS
        // =========================
        [Fact]
        public async Task GetImagesByAssignmentIdAsync_ShouldReturnImages()
        {
            var assignmentId = Guid.NewGuid();

            var assignment = new TaskAssignment
            {
                Id = assignmentId
            };

            _assignmentRepo.GetByIdAsync(assignmentId, default)
                .Returns(assignment);

            var step1 = new TaskStepExecution
            {
                Id = Guid.NewGuid(),
                StepOrder = 1,
                Status = TaskStepExecutionStatus.Completed,
                TaskStepExecutionImages = new List<TaskStepExecutionImage>
                {
                    new TaskStepExecutionImage
                    {
                        Id = Guid.NewGuid(),
                        ImageUrl = "https://storage.com/before/image1.jpg",
                        ImageType = ImageType.Before
                    }
                }
            };

            var step2 = new TaskStepExecution
            {
                Id = Guid.NewGuid(),
                StepOrder = 2,
                Status = TaskStepExecutionStatus.InProgress,
                TaskStepExecutionImages = new List<TaskStepExecutionImage>
                {
                    new TaskStepExecutionImage
                    {
                        Id = Guid.NewGuid(),
                        ImageUrl = "https://storage.com/after/image2.jpg",
                        ImageType = ImageType.After
                    }
                }
            };

            var steps = new List<TaskStepExecution> { step1, step2 };

            _stepRepo.GetStepsWithImagesByAssignmentIdAsync(assignmentId, default)
                .Returns(steps);

            var result = await _service.GetImagesByAssignmentIdAsync(assignmentId);

            Assert.NotNull(result);
            Assert.Equal(assignmentId, result.TaskAssignmentId);
            Assert.Equal(2, result.Steps.Count);
            Assert.Single(result.Steps[0].Images);
            Assert.Single(result.Steps[1].Images);
        }

        // =========================
        // GET IMAGES BY ASSIGNMENT ID - NO IMAGES
        // =========================
        [Fact]
        public async Task GetImagesByAssignmentIdAsync_ShouldReturnEmpty_WhenNoImages()
        {
            var assignmentId = Guid.NewGuid();

            var assignment = new TaskAssignment
            {
                Id = assignmentId
            };

            _assignmentRepo.GetByIdAsync(assignmentId, default)
                .Returns(assignment);

            var steps = new List<TaskStepExecution>();

            _stepRepo.GetStepsWithImagesByAssignmentIdAsync(assignmentId, default)
                .Returns(steps);

            var result = await _service.GetImagesByAssignmentIdAsync(assignmentId);

            Assert.NotNull(result);
            Assert.Equal(assignmentId, result.TaskAssignmentId);
            Assert.Empty(result.Steps);
        }

        // =========================
        // GET IMAGES BY ASSIGNMENT ID FAIL - ASSIGNMENT NOT FOUND
        // =========================
        [Fact]
        public async Task GetImagesByAssignmentIdAsync_ShouldThrow_WhenAssignmentNotFound()
        {
            var assignmentId = Guid.NewGuid();

            _assignmentRepo.GetByIdAsync(assignmentId, default)
                .Returns((TaskAssignment)null);

            await Assert.ThrowsAsync<NotFoundException>(() =>
                _service.GetImagesByAssignmentIdAsync(assignmentId));
        }

        // =========================
        // MULTIPLE IMAGE TYPES SUCCESS
        // =========================
        [Fact]
        public async Task UploadImagesAsync_ShouldHandleMultipleImages_AllTypes()
        {
            var stepId = Guid.NewGuid();

            // Test Before
            var step = new TaskStepExecution
            {
                Id = stepId,
                Status = TaskStepExecutionStatus.InProgress,
                ResultData = "{}"
            };

            _stepRepo.GetByIdAsync(stepId, default)
                .Returns(step);

            var mockFiles = new List<IFormFile>();
            for (int i = 0; i < 3; i++)
            {
                var mockFile = Substitute.For<IFormFile>();
                mockFile.FileName.Returns($"image{i}.jpg");
                mockFile.OpenReadStream().Returns(new MemoryStream(new byte[] { (byte)i }));
                mockFiles.Add(mockFile);
            }

            var request = new UploadStepImagesRequest
            {
                Images = mockFiles,
                MinPhotos = 3
            };

            var urls = Enumerable.Range(0, 3)
                .Select(i => $"https://storage.com/before/image{i}.jpg")
                .ToList();

            _storageService.UploadFilesAsync(Arg.Any<List<(Stream, string)>>(), "contracts", default)
                .Returns(urls);

            var result = await _service.UploadImagesAsync(stepId, ImageType.Before, request);

            Assert.Equal(3, result.ActualPhotos);
            Assert.Equal(3, result.MinPhotos);
        }

        // =========================
        // UPLOAD IMAGES - VERIFY STORAGE SERVICE CALL
        // =========================
        [Fact]
        public async Task UploadImagesAsync_ShouldCallStorageServiceWithCorrectParameters()
        {
            var stepId = Guid.NewGuid();

            var step = new TaskStepExecution
            {
                Id = stepId,
                Status = TaskStepExecutionStatus.InProgress,
                ResultData = "{}"
            };

            _stepRepo.GetByIdAsync(stepId, default)
                .Returns(step);

            var mockFile = Substitute.For<IFormFile>();
            mockFile.FileName.Returns("test.jpg");
            mockFile.OpenReadStream().Returns(new MemoryStream(new byte[] { 1, 2, 3 }));

            var request = new UploadStepImagesRequest
            {
                Images = new List<IFormFile> { mockFile },
                MinPhotos = 1
            };

            _storageService.UploadFilesAsync(Arg.Any<List<(Stream, string)>>(), "contracts", default)
                .Returns(new List<string> { "https://storage.com/before/test.jpg" });

            await _service.UploadImagesAsync(stepId, ImageType.Before, request);

            await _storageService.Received(1).UploadFilesAsync(
                Arg.Is<List<(Stream, string)>>(x => x.Count == 1 && x[0].Item2.Contains("before")),
                "contracts",
                default);
        }
    }
}
