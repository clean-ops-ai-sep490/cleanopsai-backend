using CleanOpsAi.Modules.TaskOperations.Application.DTOs.Request;
using CleanOpsAi.Modules.TaskOperations.Application.DTOs.Response;
using CleanOpsAi.Modules.TaskOperations.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanOpsAi.Modules.TaskOperations.Application.Common.Interfaces.Services
{
    public interface ITaskStepExecutionImageService
    {
        Task<UploadStepImagesResponse> UploadImagesAsync(
            Guid taskStepExecutionId,
            ImageType imageType,
            UploadStepImagesRequest request,
            CancellationToken ct = default);

        Task<UploadStepImagesResponse> ReUploadImagesAsync(
            Guid taskStepExecutionId,
            ImageType imageType,
            UploadStepImagesRequest request,
            CancellationToken ct = default);

        Task<TaskAssignmentImagesResponse> GetImagesByAssignmentIdAsync(
        Guid taskAssignmentId, CancellationToken ct = default);

        Task<bool> DeleteImagesByStepExecutionIdAsync(Guid taskStepExecutionId, CancellationToken ct = default);
    }
}
