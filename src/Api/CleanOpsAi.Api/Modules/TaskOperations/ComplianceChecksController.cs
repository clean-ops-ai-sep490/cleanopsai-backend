using CleanOpsAi.BuildingBlocks.Application.Pagination;
using CleanOpsAi.Modules.TaskOperations.Application.Common.Interfaces.Services;
using CleanOpsAi.Modules.TaskOperations.Application.DTOs.Request;
using CleanOpsAi.Modules.TaskOperations.Application.DTOs.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace CleanOpsAi.Api.Modules.TaskOperations
{
    [Route("api/[controller]")]
    [ApiController]
    public class ComplianceChecksController : ControllerBase
    {
        private readonly IComplianceCheckService _complianceCheckService;

        public ComplianceChecksController(IComplianceCheckService complianceCheckService)
        {
            _complianceCheckService = complianceCheckService;
        }

		[HttpGet("pending-supervisor")]
		[Authorize(Roles = "Supervisor")]
		[SwaggerOperation(
	        Summary = "Get pending supervisor compliance checks",
	        Description =
		        "Returns a paginated list of ComplianceChecks that are awaiting supervisor review. " +
		        "Supervisors can see all pending cases regardless of who submitted them. " +
		        "Images are not included in this response — call GET /{id}/detail to load images for a specific check.",
	        Tags = new[] { "ComplianceChecks" }
        )]
		[ProducesResponseType(typeof(PaginatedResult<PendingSupervisorCheckDto>), StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status401Unauthorized)]
		[ProducesResponseType(StatusCodes.Status403Forbidden)]
		public async Task<IActionResult> GetPendingSupervisorChecks(
        [FromQuery] PaginationRequest request,
        CancellationToken ct)
        {
            var result = await _complianceCheckService.GetPendingSupervisorChecksAsync(request, ct);
            return Ok(result);
        }


        [HttpPost("initiate")]
        [Authorize(Roles = "Supervisor,Worker")]
        [SwaggerOperation(
            Summary = "Initiate AI compliance check",
            Description =
                "Creates a pending ComplianceCheck and submits After images to the Scoring Module. " +
                "Result is delivered asynchronously via SignalR (compliance-check-updated). " +
                "Use GET to poll if SignalR is unavailable.",
            Tags = new[] { "ComplianceChecks" }
        )]
        [ProducesResponseType(typeof(InitiateAiCheckResult), StatusCodes.Status202Accepted)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> InitiateAiCheck(
		InitiateAiCheckRequest request,
        CancellationToken ct = default)
        {
            if (request.TaskStepExecutionId == Guid.Empty)
                return BadRequest("taskStepExecutionId cannot be empty.");

            try
            {
                var result = await _complianceCheckService
                    .InitiateAiCheckAsync(request.TaskStepExecutionId, ct);

                return Accepted(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }


		[HttpGet("task-step-executions/{taskStepExecutionId:guid}")]
		[Authorize(Roles = "Supervisor,Worker")]
        [SwaggerOperation(
            Summary = "Get current compliance check status",
            Description =
                "Polling fallback for the AI compliance check result. " +
                "The primary notification channel is SignalR (compliance-check-updated). " +
                "Returns 404 if no AI check has been initiated for this step execution.",
            Tags = new[] { "ComplianceChecks" }
        )]
        [ProducesResponseType(typeof(ComplianceCheckStatusResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetCurrentStatus(
            Guid taskStepExecutionId,
            CancellationToken ct = default)
        {
            var result = await _complianceCheckService
                .GetCurrentStatusAsync(taskStepExecutionId, ct);

            if (result is null)
                return NotFound(
                    $"No AI compliance check found for step execution {taskStepExecutionId}.");

            return Ok(result);
        }
 
	}
}
