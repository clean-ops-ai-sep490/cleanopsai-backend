using CleanOpsAi.BuildingBlocks.Application.Pagination;
using CleanOpsAi.Modules.TaskOperations.Application.Common.Interfaces.Services;
using CleanOpsAi.Modules.TaskOperations.Application.DTOs.Request;
using CleanOpsAi.Modules.TaskOperations.Application.DTOs.Response;
using CleanOpsAi.Modules.TaskOperations.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace CleanOpsAi.Api.Modules.TaskOperations
{
	[Authorize]
	[Route("api/[controller]")]
    [ApiController]
    public class EmergencyLeaveRequestsController : ControllerBase
    {
        private readonly IEmergencyLeaveRequestService _emergencyLeaveRequestService;

        public EmergencyLeaveRequestsController(IEmergencyLeaveRequestService emergencyLeaveRequestService)
        {
            _emergencyLeaveRequestService = emergencyLeaveRequestService;
        }

        [HttpGet]
        [SwaggerOperation(
            Summary = "Get all emergency leave requests with pagination",
            Description = "Retrieves a paginated list of all emergency leave requests across all workers and task assignments.",
            Tags = new[] { "EmergencyLeaveRequest" }
        )]
        [ProducesResponseType(typeof(PaginatedResult<EmergencyLeaveRequestDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> Gets(
            [FromQuery] PaginationRequest request,
            CancellationToken ct)
        {
            var result = await _emergencyLeaveRequestService.Gets(request, ct);
            return Ok(result);
        }

        [HttpGet("{id:guid}")]
        [SwaggerOperation(
            Summary = "Get emergency leave request by ID",
            Description = "Retrieves a single emergency leave request by its ID. Returns 404 if not found or soft-deleted.",
            Tags = new[] { "EmergencyLeaveRequest" }
        )]
        [ProducesResponseType(typeof(EmergencyLeaveRequestDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
        {
            var result = await _emergencyLeaveRequestService.GetById(id, ct);
            if (result == null) return NotFound();
            return Ok(result);
        }

        [HttpGet("worker/{workerId:guid}")]
        [SwaggerOperation(
            Summary = "Get emergency leave requests by worker with pagination",
            Description = "Retrieves a paginated list of emergency leave requests submitted by a specific worker.",
            Tags = new[] { "EmergencyLeaveRequest" }
        )]
        [ProducesResponseType(typeof(PaginatedResult<EmergencyLeaveRequestDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetsByWorkerId(
            Guid workerId,
            [FromQuery] PaginationRequest request,
            CancellationToken ct)
        {
            var result = await _emergencyLeaveRequestService.GetsByWorkerId(workerId, request, ct);
            return Ok(result);
        }

        [HttpGet("task-assignment/{taskAssignmentId:guid}")]
        [SwaggerOperation(
            Summary = "Get emergency leave requests by task assignment with pagination",
            Description = "Retrieves a paginated list of emergency leave requests associated with a specific task assignment.",
            Tags = new[] { "EmergencyLeaveRequest" }
        )]
        [ProducesResponseType(typeof(PaginatedResult<EmergencyLeaveRequestDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetsByTaskAssignmentId(
            Guid taskAssignmentId,
            [FromQuery] PaginationRequest request,
            CancellationToken ct)
        {
            var result = await _emergencyLeaveRequestService.GetsByTaskAssignmentId(taskAssignmentId, request, ct);
            return Ok(result);
        }

        [HttpGet("status/{status}")]
        [SwaggerOperation(
            Summary = "Get emergency leave requests by status with pagination",
            Description = "Retrieves a paginated list of emergency leave requests filtered by status (e.g. Pending, Approved, Rejected).",
            Tags = new[] { "EmergencyLeaveRequest" }
        )]
        [ProducesResponseType(typeof(PaginatedResult<EmergencyLeaveRequestDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetsByStatus(
            RequestStatus status,
            [FromQuery] PaginationRequest request,
            CancellationToken ct)
        {
            var result = await _emergencyLeaveRequestService.GetsByStatus(status, request, ct);
            return Ok(result);
        }

        [HttpPost]
        [RequestSizeLimit(104857600)]
        [RequestFormLimits(MultipartBodyLengthLimit = 104857600)]
        [SwaggerOperation(
            Summary = "Create a new emergency leave request",
            Description = "Creates a new emergency leave request. Worker co the gui audio, tu go transcription, hoac ca hai. " +
                          "Neu upload audio thi chi dc duoi 10mb" +
                          "Status is automatically set to Pending upon creation.",
            Tags = new[] { "EmergencyLeaveRequest" }
        )]
        [ProducesResponseType(typeof(EmergencyLeaveRequestDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create(
            [FromForm] Guid workerId,
            [FromForm] Guid? taskAssignmentId,
            [FromForm] DateTime? leaveDateFrom,
            [FromForm] DateTime? leaveDateTo,
            [FromForm] string? transcription,
            IFormFile? audioFile,
            CancellationToken ct)
        {
            var dto = new CreateEmergencyLeaveRequestDto
            {
                WorkerId = workerId,
                TaskAssignmentId = taskAssignmentId,
                LeaveDateFrom = leaveDateFrom,
                LeaveDateTo = leaveDateTo,
                Transcription = transcription,
                AudioStream = audioFile?.OpenReadStream(),
                AudioFileName = audioFile?.FileName  // lay thang tu IFormFile
            };

            var result = await _emergencyLeaveRequestService.Create(dto, ct);
            return CreatedAtAction(nameof(GetById), new { id = result!.Id }, result);
        }

        [HttpPut("{id:guid}")]
        [RequestSizeLimit(104857600)]
        [RequestFormLimits(MultipartBodyLengthLimit = 104857600)]
        [SwaggerOperation(
            Summary = "Update an emergency leave request",
            Description = "Updates audio hoac transcription cua emergency leave request. " +
                          "Neu upload audio thi chi dc duoi 10mb" +
                          "Automatically updates LastModified and LastModifiedBy fields.",
            Tags = new[] { "EmergencyLeaveRequest" }
        )]
        [ProducesResponseType(typeof(EmergencyLeaveRequestDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Update(
            Guid id,
            [FromForm] DateTime? leaveDateFrom,
            [FromForm] DateTime? leaveDateTo,
            [FromForm] string? transcription,
            IFormFile? audioFile,
            CancellationToken ct)
        {
            var dto = new UpdateEmergencyLeaveRequestDto
            {
                LeaveDateFrom = leaveDateFrom,
                LeaveDateTo = leaveDateTo,
                Transcription = transcription,
                AudioStream = audioFile?.OpenReadStream(),
                AudioFileName = audioFile?.FileName  // lay thang tu IFormFile
            };

            var result = await _emergencyLeaveRequestService.Update(id, dto, ct);
            if (result == null) return NotFound();
            return Ok(result);
        }

        [HttpPatch("{id:guid}/review")]
        [SwaggerOperation(
            Summary = "Review an emergency leave request",
            Description = "Approves or rejects an emergency leave request. Sets ReviewedByUserId, updates ApprovedAt if approved, " +
                          "and triggers a notification email to the worker via RabbitMQ.",
            Tags = new[] { "EmergencyLeaveRequest" }
        )]
        [ProducesResponseType(typeof(EmergencyLeaveRequestDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Review(
            Guid id,
            [FromBody] ReviewEmergencyLeaveRequestDto dto,
            CancellationToken ct)
        {
            var result = await _emergencyLeaveRequestService.Review(id, dto, ct);
            if (result == null) return NotFound();
            return Ok(result);
        }

        [HttpDelete("{id:guid}")]
        [SwaggerOperation(
            Summary = "Delete an emergency leave request",
            Description = "Soft deletes an emergency leave request by its ID. The record is marked as deleted (IsDeleted = true) " +
                          "and will no longer appear in queries.",
            Tags = new[] { "EmergencyLeaveRequest" }
        )]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
        {
            var result = await _emergencyLeaveRequestService.Delete(id, ct);
            if (!result) return NotFound();
            return Ok();
        }

        [HttpGet("worker/{workerId:guid}/current-month")]
        [SwaggerOperation(
            Summary = "Get emergency leave requests of a worker in current month",
            Description = "Retrieves a paginated list of emergency leave requests of a specific worker within the current month.",
            Tags = new[] { "EmergencyLeaveRequest" }
        )]
        [ProducesResponseType(typeof(PaginatedResult<EmergencyLeaveRequestDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetsByWorkerCurrentMonth(
            Guid workerId,
            [FromQuery] PaginationRequest request,
            CancellationToken ct)
        {
            var result = await _emergencyLeaveRequestService
                .GetsByWorkerCurrentMonth(workerId, request, ct);

            return Ok(result);
        }
    }
}
