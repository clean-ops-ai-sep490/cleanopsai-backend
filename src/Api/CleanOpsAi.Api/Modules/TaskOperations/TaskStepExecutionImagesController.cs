using CleanOpsAi.Modules.TaskOperations.Application.Common.Interfaces.Services;
using CleanOpsAi.Modules.TaskOperations.Application.DTOs.Request;
using CleanOpsAi.Modules.TaskOperations.Application.DTOs.Response;
using CleanOpsAi.Modules.TaskOperations.Domain.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace CleanOpsAi.Api.Modules.TaskOperations
{
    [Route("api/[controller]")]
    [ApiController]
    public class TaskStepExecutionImagesController : ControllerBase
    {
        private readonly ITaskStepExecutionImageService _service;

        public TaskStepExecutionImagesController(ITaskStepExecutionImageService service)
        {
            _service = service;
        }

        [HttpGet]
        [SwaggerOperation(
            Summary = "Get all images by task assignment",
            Description = "Retrieves all images grouped by step execution for a given task assignment ID.",
            Tags = new[] { "TaskStepExecutionImage" }
        )]
        [ProducesResponseType(typeof(TaskAssignmentImagesResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetImages(
            [FromQuery] Guid taskAssignmentId,
            CancellationToken ct)
        {
            var result = await _service.GetImagesByAssignmentIdAsync(taskAssignmentId, ct);
            return Ok(result);
        }

        [HttpPost("{taskStepExecutionId:guid}/{imageType}")]
        [Consumes("multipart/form-data")]
        [SwaggerOperation(
            Summary = "Upload images for a step execution",
            Description = "Upload one or more images for a step execution. imageType: Before = 0, After = 1, Ppe = 2.",
            Tags = new[] { "TaskStepExecutionImage" }
        )]
        [ProducesResponseType(typeof(UploadStepImagesResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Upload(
            Guid taskStepExecutionId,
            ImageType imageType,
            [FromForm] UploadStepImagesRequest request,
            CancellationToken ct)
        {
            var result = await _service.UploadImagesAsync(
                taskStepExecutionId, imageType, request, ct);
            return Ok(result);
        }

        [HttpPut("{taskStepExecutionId:guid}/{imageType}")]
        [Consumes("multipart/form-data")]
        [SwaggerOperation(
            Summary = "Reupload images for a step execution",
            Description = "Soft-deletes existing images and uploads new ones. Not allowed if step is Completed.",
            Tags = new[] { "TaskStepExecutionImage" }
        )]
        [ProducesResponseType(typeof(UploadStepImagesResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ReUpload(
            Guid taskStepExecutionId,
            ImageType imageType,
            [FromForm] UploadStepImagesRequest request,
            CancellationToken ct)
        {
            var result = await _service.ReUploadImagesAsync(
                taskStepExecutionId, imageType, request, ct);
            return Ok(result);
        }
    }
}