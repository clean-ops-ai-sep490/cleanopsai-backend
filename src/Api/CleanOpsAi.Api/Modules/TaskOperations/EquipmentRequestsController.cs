using CleanOpsAi.BuildingBlocks.Application.Pagination;
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
    public class EquipmentRequestsController : ControllerBase
    {
        private readonly IEquipmentRequestService _equipmentRequestService;

        public EquipmentRequestsController(IEquipmentRequestService equipmentRequestService)
        {
            _equipmentRequestService = equipmentRequestService;
        }

        [HttpGet]
        [SwaggerOperation(
            Summary = "Get all equipment requests with pagination",
            Description = "Retrieves a paginated list of all equipment requests across all workers and task assignments.",
            Tags = new[] { "EquipmentRequest" }
        )]
        [ProducesResponseType(typeof(PaginatedResult<EquipmentRequestDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> Gets(
            [FromQuery] PaginationRequest request,
            CancellationToken ct)
        {
            var result = await _equipmentRequestService.Gets(request, ct);
            return Ok(result);
        }

        [HttpGet("{id:guid}")]
        [SwaggerOperation(
            Summary = "Get equipment request by ID",
            Description = "Retrieves a single equipment request by its ID. Returns 404 if not found or soft-deleted.",
            Tags = new[] { "EquipmentRequest" }
        )]
        [ProducesResponseType(typeof(EquipmentRequestDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
        {
            var result = await _equipmentRequestService.GetById(id, ct);
            if (result == null) return NotFound();
            return Ok(result);
        }

        [HttpGet("worker/{workerId:guid}")]
        [SwaggerOperation(
            Summary = "Get equipment requests by worker with pagination",
            Description = "Retrieves a paginated list of equipment requests submitted by a specific worker.",
            Tags = new[] { "EquipmentRequest" }
        )]
        [ProducesResponseType(typeof(PaginatedResult<EquipmentRequestDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetsByWorkerId(
            Guid workerId,
            [FromQuery] PaginationRequest request,
            CancellationToken ct)
        {
            var result = await _equipmentRequestService.GetsByWorkerId(workerId, request, ct);
            return Ok(result);
        }

        [HttpGet("task-assignment/{taskAssignmentId:guid}")]
        [SwaggerOperation(
            Summary = "Get equipment requests by task assignment with pagination",
            Description = "Retrieves a paginated list of equipment requests associated with a specific task assignment.",
            Tags = new[] { "EquipmentRequest" }
        )]
        [ProducesResponseType(typeof(PaginatedResult<EquipmentRequestDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetsByTaskAssignmentId(
            Guid taskAssignmentId,
            [FromQuery] PaginationRequest request,
            CancellationToken ct)
        {
            var result = await _equipmentRequestService.GetsByTaskAssignmentId(taskAssignmentId, request, ct);
            return Ok(result);
        }

        [HttpGet("equipment/{equipmentId:guid}")]
        [SwaggerOperation(
            Summary = "Get equipment requests by equipment with pagination",
            Description = "Retrieves a paginated list of equipment requests for a specific piece of equipment.",
            Tags = new[] { "EquipmentRequest" }
        )]
        [ProducesResponseType(typeof(PaginatedResult<EquipmentRequestDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetsByEquipmentId(
            Guid equipmentId,
            [FromQuery] PaginationRequest request,
            CancellationToken ct)
        {
            var result = await _equipmentRequestService.GetsByEquipmentId(equipmentId, request, ct);
            return Ok(result);
        }

        [HttpGet("status/{status}")]
        [SwaggerOperation(
            Summary = "Get equipment requests by status with pagination",
            Description = "Retrieves a paginated list of equipment requests filtered by status (e.g. Pending, Approved, Rejected).",
            Tags = new[] { "EquipmentRequest" }
        )]
        [ProducesResponseType(typeof(PaginatedResult<EquipmentRequestDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetsByStatus(
            EquipmentRequestStatus status,
            [FromQuery] PaginationRequest request,
            CancellationToken ct)
        {
            var result = await _equipmentRequestService.GetsByStatus(status, request, ct);
            return Ok(result);
        }

        [HttpPost]
        [SwaggerOperation(
            Summary = "Create a new equipment request",
            Description = "Creates a new equipment request for a worker on a specific task assignment. " +
                          "Status is automatically set to Pending upon creation.",
            Tags = new[] { "EquipmentRequest" }
        )]
        [ProducesResponseType(typeof(EquipmentRequestDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create(
            [FromBody] CreateEquipmentRequestDto dto,
            CancellationToken ct)
        {
            var result = await _equipmentRequestService.Create(dto, ct);
            return CreatedAtAction(nameof(GetById), new { id = result!.Id }, result);
        }

        [HttpPut("{id:guid}")]
        [SwaggerOperation(
            Summary = "Update an equipment request",
            Description = "Updates the quantity and reason of an existing equipment request. " +
                          "Automatically updates LastModified and LastModifiedBy fields.",
            Tags = new[] { "EquipmentRequest" }
        )]
        [ProducesResponseType(typeof(EquipmentRequestDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Update(
            Guid id,
            [FromBody] UpdateEquipmentRequestDto dto,
            CancellationToken ct)
        {
            var result = await _equipmentRequestService.Update(id, dto, ct);
            if (result == null) return NotFound();
            return Ok(result);
        }

        [HttpPatch("{id:guid}/review")]
        [SwaggerOperation(
            Summary = "Review an equipment request",
            Description = "Approves or rejects an equipment request. Sets ReviewedByUserId, updates ApprovedAt if approved, " +
                          "and triggers a notification email to the worker via RabbitMQ.",
            Tags = new[] { "EquipmentRequest" }
        )]
        [ProducesResponseType(typeof(EquipmentRequestDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Review(
            Guid id,
            [FromBody] ReviewEquipmentRequestDto dto,
            CancellationToken ct)
        {
            var result = await _equipmentRequestService.Review(id, dto, ct);
            if (result == null) return NotFound();
            return Ok(result);
        }

        [HttpDelete("{id:guid}")]
        [SwaggerOperation(
            Summary = "Delete an equipment request",
            Description = "Soft deletes an equipment request by its ID. The record is marked as deleted (IsDeleted = true) " +
                          "and will no longer appear in queries.",
            Tags = new[] { "EquipmentRequest" }
        )]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
        {
            var result = await _equipmentRequestService.Delete(id, ct);
            if (!result) return NotFound();
            return Ok();
        }
    }
}
