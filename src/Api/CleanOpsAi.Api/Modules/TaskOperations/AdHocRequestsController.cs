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
    public class AdHocRequestsController : ControllerBase
    {
        private readonly IAdHocRequestService _service;

        public AdHocRequestsController(IAdHocRequestService service)
        {
            _service = service;
        }

        [HttpGet]
        [SwaggerOperation(
            Summary = "Get all adhoc requests with pagination",
            Description = "Retrieves a paginated list of all adhoc requests.",
            Tags = new[] { "AdHocRequest" }
        )]
        [ProducesResponseType(typeof(PaginatedResult<AdHocRequestDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> Gets(
            [FromQuery] PaginationRequest request,
            CancellationToken ct)
        {
            var result = await _service.Gets(request, ct);
            return Ok(result);
        }

        [HttpGet("{id:guid}")]
        [SwaggerOperation(
            Summary = "Get adhoc request by ID",
            Description = "Retrieves a single adhoc request by ID.",
            Tags = new[] { "AdHocRequest" }
        )]
        [ProducesResponseType(typeof(AdHocRequestDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
        {
            var result = await _service.GetById(id, ct);
            if (result == null) return NotFound();
            return Ok(result);
        }

        [HttpGet("worker/{workerId:guid}")]
        [SwaggerOperation(
            Summary = "Get adhoc requests by worker",
            Description = "Retrieves adhoc requests submitted by a specific worker.",
            Tags = new[] { "AdHocRequest" }
        )]
        [ProducesResponseType(typeof(PaginatedResult<AdHocRequestDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetsByWorkerId(
            Guid workerId,
            [FromQuery] PaginationRequest request,
            CancellationToken ct)
        {
            var result = await _service.GetsByWorkerId(workerId, request, ct);
            return Ok(result);
        }

        [HttpGet("task-assignment/{taskAssignmentId:guid}")]
        [SwaggerOperation(
            Summary = "Get adhoc requests by task assignment",
            Description = "Retrieves adhoc requests associated with a task assignment.",
            Tags = new[] { "AdHocRequest" }
        )]
        [ProducesResponseType(typeof(PaginatedResult<AdHocRequestDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetsByTaskAssignmentId(
            Guid taskAssignmentId,
            [FromQuery] PaginationRequest request,
            CancellationToken ct)
        {
            var result = await _service.GetsByTaskAssignmentId(taskAssignmentId, request, ct);
            return Ok(result);
        }

        [HttpGet("status/{status}")]
        [SwaggerOperation(
            Summary = "Get adhoc requests by status",
            Description = "Retrieves adhoc requests filtered by status (Pending, Approved, Rejected).",
            Tags = new[] { "AdHocRequest" }
        )]
        [ProducesResponseType(typeof(PaginatedResult<AdHocRequestDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetsByStatus(
            AdHocRequestStatus status,
            [FromQuery] PaginationRequest request,
            CancellationToken ct)
        {
            var result = await _service.GetsByStatus(status, request, ct);
            return Ok(result);
        }

        [HttpGet("type/{type}")]
        [SwaggerOperation(
            Summary = "Get adhoc requests by type",
            Description = "Retrieves adhoc requests filtered by request type.",
            Tags = new[] { "AdHocRequest" }
        )]
        [ProducesResponseType(typeof(PaginatedResult<AdHocRequestDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetsByType(
            AdHocRequestType type,
            [FromQuery] PaginationRequest request,
            CancellationToken ct)
        {
            var result = await _service.GetsByType(type, request, ct);
            return Ok(result);
        }

        [HttpPost]
        [SwaggerOperation(
            Summary = "Create adhoc request",
            Description = "Creates a new adhoc request. Status is set to Pending automatically.",
            Tags = new[] { "AdHocRequest" }
        )]
        [ProducesResponseType(typeof(AdHocRequestDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create(
            [FromBody] CreateAdHocRequestDto dto,
            CancellationToken ct)
        {
            var result = await _service.Create(dto, ct);
            return CreatedAtAction(nameof(GetById), new { id = result!.Id }, result);
        }

        [HttpPut("{id:guid}")]
        [SwaggerOperation(
            Summary = "Update adhoc request",
            Description = "Updates request type, reason or description.",
            Tags = new[] { "AdHocRequest" }
        )]
        [ProducesResponseType(typeof(AdHocRequestDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(
            Guid id,
            [FromBody] UpdateAdHocRequestDto dto,
            CancellationToken ct)
        {
            var result = await _service.Update(id, dto, ct);
            if (result == null) return NotFound();
            return Ok(result);
        }

        [HttpPatch("{id:guid}/review")]
        [SwaggerOperation(
            Summary = "Review adhoc request",
            Description = "Approve or reject adhoc request. Will trigger notification via RabbitMQ.",
            Tags = new[] { "AdHocRequest" }
        )]
        [ProducesResponseType(typeof(AdHocRequestDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Review(
            Guid id,
            [FromBody] ReviewAdHocRequestDto dto,
            CancellationToken ct)
        {
            var result = await _service.Review(id, dto, ct);
            if (result == null) return NotFound();
            return Ok(result);
        }

        [HttpDelete("{id:guid}")]
        [SwaggerOperation(
            Summary = "Delete adhoc request",
            Description = "Soft delete adhoc request.",
            Tags = new[] { "AdHocRequest" }
        )]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
        {
            var result = await _service.Delete(id, ct);
            if (!result) return NotFound();
            return Ok();
        }
    }
}
