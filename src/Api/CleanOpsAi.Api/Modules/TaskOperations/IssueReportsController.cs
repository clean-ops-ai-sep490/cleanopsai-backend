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
    public class IssueReportsController : ControllerBase
    {
        private readonly IIssueReportService _issueReportService;

        public IssueReportsController(IIssueReportService issueReportService)
        {
            _issueReportService = issueReportService;
        }

        [HttpGet]
        [SwaggerOperation(
            Summary = "Get all issue reports with pagination",
            Description = "Retrieves a paginated list of all issue reports across all workers and task assignments.",
            Tags = new[] { "IssueReport" }
        )]
        [ProducesResponseType(typeof(PaginatedResult<IssueReportDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> Gets(
            [FromQuery] PaginationRequest request,
            CancellationToken ct)
        {
            var result = await _issueReportService.Gets(request, ct);
            return Ok(result);
        }

        [HttpGet("{id:guid}")]
        [SwaggerOperation(
            Summary = "Get issue report by ID",
            Description = "Retrieves a single issue report by its ID. Returns 404 if not found or soft-deleted.",
            Tags = new[] { "IssueReport" }
        )]
        [ProducesResponseType(typeof(IssueReportDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
        {
            var result = await _issueReportService.GetById(id, ct);
            if (result == null) return NotFound();
            return Ok(result);
        }

        [HttpGet("worker/{workerId:guid}")]
        [SwaggerOperation(
            Summary = "Get issue reports by worker with pagination",
            Description = "Retrieves a paginated list of issue reports submitted by a specific worker.",
            Tags = new[] { "IssueReport" }
        )]
        [ProducesResponseType(typeof(PaginatedResult<IssueReportDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetsByWorkerId(
            Guid workerId,
            [FromQuery] PaginationRequest request,
            CancellationToken ct)
        {
            var result = await _issueReportService.GetsByWorkerId(workerId, request, ct);
            return Ok(result);
        }

        [HttpGet("task-assignment/{taskAssignmentId:guid}")]
        [SwaggerOperation(
            Summary = "Get issue reports by task assignment with pagination",
            Description = "Retrieves a paginated list of issue reports associated with a specific task assignment.",
            Tags = new[] { "IssueReport" }
        )]
        [ProducesResponseType(typeof(PaginatedResult<IssueReportDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetsByTaskAssignmentId(
            Guid taskAssignmentId,
            [FromQuery] PaginationRequest request,
            CancellationToken ct)
        {
            var result = await _issueReportService.GetsByTaskAssignmentId(taskAssignmentId, request, ct);
            return Ok(result);
        }

        [HttpGet("status/{status}")]
        [SwaggerOperation(
            Summary = "Get issue reports by status with pagination",
            Description = "Retrieves a paginated list of issue reports filtered by status (e.g. Approved, Rejected).",
            Tags = new[] { "IssueReport" }
        )]
        [ProducesResponseType(typeof(PaginatedResult<IssueReportDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetsByStatus(
            IssueStatus status,
            [FromQuery] PaginationRequest request,
            CancellationToken ct)
        {
            var result = await _issueReportService.GetsByStatus(status, request, ct);
            return Ok(result);
        }

        [HttpPost]
        [SwaggerOperation(
            Summary = "Create a new issue report",
            Description = "Creates a new issue report submitted by a worker on a specific task assignment. " +
                          "Status is automatically set to Open upon creation.",
            Tags = new[] { "IssueReport" }
        )]
        [ProducesResponseType(typeof(IssueReportDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create(
            [FromBody] CreateIssueReportDto dto,
            CancellationToken ct)
        {
            var result = await _issueReportService.Create(dto, ct);
            return CreatedAtAction(nameof(GetById), new { id = result!.Id }, result);
        }

        [HttpPut("{id:guid}")]
        [SwaggerOperation(
            Summary = "Update an issue report",
            Description = "Updates the description of an existing issue report. " +
                          "Automatically updates LastModified and LastModifiedBy fields.",
            Tags = new[] { "IssueReport" }
        )]
        [ProducesResponseType(typeof(IssueReportDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Update(
            Guid id,
            [FromBody] UpdateIssueReportDto dto,
            CancellationToken ct)
        {
            var result = await _issueReportService.Update(id, dto, ct);
            if (result == null) return NotFound();
            return Ok(result);
        }

        [HttpPatch("{id:guid}/resolve")]
        [SwaggerOperation(
            Summary = "Resolve an issue report",
            Description = "Approves or rejects an issue report. Sets ResolvedByUserId, updates ResolvedAt if approved, " +
                          "and triggers a notification email to the worker via RabbitMQ.",
            Tags = new[] { "IssueReport" }
        )]
        [ProducesResponseType(typeof(IssueReportDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Resolve(
            Guid id,
            [FromBody] ResolveIssueReportDto dto,
            CancellationToken ct)
        {
            var result = await _issueReportService.Resolve(id, dto, ct);
            if (result == null) return NotFound();
            return Ok(result);
        }

        [HttpDelete("{id:guid}")]
        [SwaggerOperation(
            Summary = "Delete an issue report",
            Description = "Soft deletes an issue report by its ID. The record is marked as deleted (IsDeleted = true) " +
                          "and will no longer appear in queries.",
            Tags = new[] { "IssueReport" }
        )]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
        {
            var result = await _issueReportService.Delete(id, ct);
            if (!result) return NotFound();
            return Ok();
        }
    }
}
