using CleanOpsAi.BuildingBlocks.Application;
using CleanOpsAi.BuildingBlocks.Application.Exceptions;
using CleanOpsAi.BuildingBlocks.Application.Interfaces;
using CleanOpsAi.BuildingBlocks.Infrastructure.Services;
using CleanOpsAi.Modules.TaskOperations.Application.Common.Interfaces.Repositories;
using CleanOpsAi.Modules.TaskOperations.Application.Common.Interfaces.Services;
using CleanOpsAi.Modules.TaskOperations.Application.DTOs.Request;
using CleanOpsAi.Modules.TaskOperations.Application.DTOs.Response;
using CleanOpsAi.Modules.TaskOperations.Domain.Entities;
using CleanOpsAi.Modules.TaskOperations.Domain.Enums;
using Microsoft.AspNetCore.Http;
using System.Text.Json;

namespace CleanOpsAi.Modules.TaskOperations.Application.Services
{
    public class TaskStepExecutionImageService : ITaskStepExecutionImageService
    {
        private readonly ITaskAssignmentRepository _assignmentRepo;
        private readonly ITaskStepExecutionRepository _stepRepo;
        private readonly ITaskStepExecutionImageRepository _imageRepo;
        private readonly IFileStorageService _storageService;
        private readonly IUserContext _userContext;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly IIdGenerator _idGenerator;

        private const string CONTAINER = "contracts";
        private const string BEFORE_FOLDER = "before";
        private const string AFTER_FOLDER = "after";
        private const string PPE_FOLDER = "ppe";

        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        private const string AI_PPE_CHECK_BEHAVIOR = "ai-ppe-check";

        public TaskStepExecutionImageService(
            ITaskAssignmentRepository assignmentRepo,
            ITaskStepExecutionRepository stepRepo,
            ITaskStepExecutionImageRepository imageRepo,
            IFileStorageService storageService,
            IUserContext userContext,
            IDateTimeProvider dateTimeProvide,
            IIdGenerator idGenerator)
        {
            _assignmentRepo = assignmentRepo;
            _stepRepo = stepRepo;
            _imageRepo = imageRepo;
            _storageService = storageService;
            _userContext = userContext;
            _dateTimeProvider = dateTimeProvide;
            _idGenerator = idGenerator;
        }

        public async Task<UploadStepImagesResponse> UploadImagesAsync(
            Guid taskStepExecutionId,
            ImageType imageType,
            UploadStepImagesRequest request,
            CancellationToken ct = default)
        {
            var step = await _stepRepo.GetByIdAsync(taskStepExecutionId, ct)
                ?? throw new NotFoundException(nameof(TaskStepExecution), taskStepExecutionId);

            // Check status
            EnsureStepIsInProgress(step);

            if (request.Images == null || request.Images.Count == 0)
                throw new BadRequestException("No images provided.");

            var uploadedImages = await UploadFilesToStorageAsync(
                taskStepExecutionId, imageType, request.Images, ct);

            await _imageRepo.AddRangeAsync(uploadedImages, ct);

            var response = BuildResponse(imageType, request.MinPhotos, uploadedImages.Count);
            if (!ShouldPreserveExistingResult(step, imageType))
            {
                step.ResultData = JsonSerializer.Serialize(response, _jsonOptions);
            }

            await _imageRepo.SaveChangesAsync(ct);

            return response;
        }

        public async Task<UploadStepImagesResponse> ReUploadImagesAsync(
            Guid taskStepExecutionId,
            ImageType imageType,
            UploadStepImagesRequest request,
            CancellationToken ct = default)
        {
            var step = await _stepRepo.GetByIdAsync(taskStepExecutionId, ct)
                ?? throw new NotFoundException(nameof(TaskStepExecution), taskStepExecutionId);

            // Check status (thay vì chỉ check Completed)
            EnsureStepIsInProgress(step);

            if (request.Images == null || request.Images.Count == 0)
                throw new BadRequestException("No images provided.");

            // Soft-delete ảnh cũ
            var existingImages = await _imageRepo.GetActiveByExecutionIdAndTypeAsync(
                taskStepExecutionId, imageType, ct);

            foreach (var img in existingImages)
            {
                img.IsDeleted = true;
                img.LastModified = _dateTimeProvider.UtcNow;
                img.LastModifiedBy = _userContext.UserId.ToString();
            }

            // Upload ảnh mới
            var newImages = await UploadFilesToStorageAsync(
                taskStepExecutionId, imageType, request.Images, ct);

            await _imageRepo.AddRangeAsync(newImages, ct);

            var response = BuildResponse(imageType, request.MinPhotos, newImages.Count);
            if (!ShouldPreserveExistingResult(step, imageType))
            {
                step.ResultData = JsonSerializer.Serialize(response, _jsonOptions);
            }

            await _imageRepo.SaveChangesAsync(ct);

            return response;
        }

        public async Task<TaskAssignmentImagesResponse> GetImagesByAssignmentIdAsync(
            Guid taskAssignmentId, CancellationToken ct = default)
        {
            var assignment = await _assignmentRepo.GetByIdAsync(taskAssignmentId, ct)
                ?? throw new NotFoundException(nameof(TaskAssignment), taskAssignmentId);

            var steps = await _stepRepo.GetStepsWithImagesByAssignmentIdAsync(taskAssignmentId, ct);

            return new TaskAssignmentImagesResponse
            {
                TaskAssignmentId = taskAssignmentId,
                Steps = steps.Select(s => new StepImagesDto
                {
                    StepExecutionId = s.Id,
                    StepOrder = s.StepOrder,
                    Status = s.Status,
                    Images = s.TaskStepExecutionImages.Select(img => new StepImageDto
                    {
                        Id = img.Id,
                        ImageUrl = img.ImageUrl,
                        ImageType = img.ImageType
                    }).ToList()
                }).ToList()
            };
        }

        public async Task<bool> DeleteImagesByStepExecutionIdAsync(
            Guid taskStepExecutionId,
            CancellationToken ct = default)
        {
            var step = await _stepRepo.GetByIdAsync(taskStepExecutionId, ct)
                ?? throw new NotFoundException(nameof(TaskStepExecution), taskStepExecutionId);

            // Chỉ cho xoá khi đang InProgress
            EnsureStepIsInProgress(step);

            var images = await _imageRepo.GetActiveByExecutionIdAsync(taskStepExecutionId, ct);

            if (images == null || images.Count == 0)
                throw new BadRequestException("No images to delete.");

            foreach (var img in images)
            {
                img.IsDeleted = true;
            }

            await _imageRepo.SaveChangesAsync(ct);

            return true;
        }

        public async Task<bool> DeleteImageByIdAsync(Guid imageId, CancellationToken ct = default)
        {
            var image = await _imageRepo.GetByIdAsync(imageId, ct)
                ?? throw new NotFoundException(nameof(TaskStepExecutionImage), imageId);

            // lấy step để check status
            var step = await _stepRepo.GetByIdAsync(image.TaskStepExecutionId, ct)
                ?? throw new NotFoundException(nameof(TaskStepExecution), image.TaskStepExecutionId);

            // chỉ cho xoá khi InProgress
            EnsureStepIsInProgress(step);

            image.IsDeleted = true;
            image.LastModified = _dateTimeProvider.UtcNow;
            image.LastModifiedBy = _userContext.UserId.ToString();

            await _imageRepo.SaveChangesAsync(ct);

            return true;
        }

        // ---------- Helpers ----------

        private static void EnsureStepIsInProgress(TaskStepExecution step)
        {
            if (step.Status != TaskStepExecutionStatus.InProgress)
                throw new BadRequestException("Step must be in progress to upload images.");
        }

        private static string GetFolder(ImageType imageType) => imageType switch
        {
            ImageType.Before => BEFORE_FOLDER,
            ImageType.After => AFTER_FOLDER,
            ImageType.Ppe => PPE_FOLDER,
            _ => imageType.ToString().ToLower()
        };

        private async Task<List<TaskStepExecutionImage>> UploadFilesToStorageAsync(
            Guid taskStepExecutionId,
            ImageType imageType,
            List<IFormFile> files,
            CancellationToken ct)
        {
            var folder = GetFolder(imageType);

            var fileInputs = files.Select(f =>
            {
                var stream = f.OpenReadStream();
                var fileName = $"{folder}/{f.FileName}";
                return (Stream: stream, FileName: fileName);
            }).ToList();

            var urls = await _storageService.UploadFilesAsync(fileInputs, CONTAINER, ct);

            foreach (var (stream, _) in fileInputs)
                await stream.DisposeAsync();

            return urls.Select(url => new TaskStepExecutionImage
            {
                Id = _idGenerator.Generate(),
                TaskStepExecutionId = taskStepExecutionId,
                ImageUrl = url,
                ImageType = imageType,
                Created = _dateTimeProvider.UtcNow,
                CreatedBy = _userContext.UserId.ToString()
            }).ToList();
        }

        private static UploadStepImagesResponse BuildResponse(
            ImageType imageType, int minPhotos, int actualPhotos)
        {
            return new UploadStepImagesResponse
            {
                Phase = imageType.ToString().ToLower(),
                MinPhotos = minPhotos,
                ActualPhotos = actualPhotos
            };
        }

        private static bool ShouldPreserveExistingResult(TaskStepExecution step, ImageType imageType)
        {
            return imageType == ImageType.Ppe && IsAiPpeCheckStep(step.ConfigSnapshot);
        }

        private static bool IsAiPpeCheckStep(string configSnapshot)
        {
            if (string.IsNullOrWhiteSpace(configSnapshot))
            {
                return false;
            }

            try
            {
                using var document = JsonDocument.Parse(configSnapshot);
                if (!document.RootElement.TryGetProperty("schema", out var schema))
                {
                    return false;
                }

                if (!schema.TryGetProperty("x-behavior", out var behavior))
                {
                    return false;
                }

                return string.Equals(
                    behavior.GetString(),
                    AI_PPE_CHECK_BEHAVIOR,
                    StringComparison.OrdinalIgnoreCase);
            }
            catch (JsonException)
            {
                return false;
            }
        }
    }
}