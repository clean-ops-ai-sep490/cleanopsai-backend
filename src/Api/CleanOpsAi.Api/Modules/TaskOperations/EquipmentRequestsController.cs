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
            Description = "Retrieves a single equipment request by its ID.",
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

        [HttpPost]
        [SwaggerOperation(
            Summary = "Create a new equipment request",
            Description = "Creates a new equipment request (batch items). Status = Pending.",
            Tags = new[] { "EquipmentRequest" }
        )]
        [ProducesResponseType(typeof(EquipmentRequestDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create(
            [FromBody] CreateEquipmentRequestBatchDto dto,
            CancellationToken ct)
        {
            var result = await _equipmentRequestService.CreateBatch(dto, ct);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        [HttpPut("{id:guid}")]
        [SwaggerOperation(
            Summary = "Update an equipment request",
            Description = "Updates reason + items",
            Tags = new[] { "EquipmentRequest" }
        )]
        [ProducesResponseType(typeof(EquipmentRequestDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
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
            Description = "Approve / Reject request",
            Tags = new[] { "EquipmentRequest" }
        )]
        [ProducesResponseType(typeof(EquipmentRequestDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
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
            Description = "Soft delete",
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
