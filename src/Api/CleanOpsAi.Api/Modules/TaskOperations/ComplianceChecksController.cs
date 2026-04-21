using CleanOpsAi.Modules.TaskOperations.Application.Common.Interfaces.Services;
using CleanOpsAi.Modules.TaskOperations.Application.DTOs.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace CleanOpsAi.Api.Modules.TaskOperations
{
    /// <summary>
    /// AI compliance check endpoints scoped to a task step execution.
    /// Route: /api/task-step-executions/{taskStepExecutionId}/compliance-check
    /// </summary>
    [Route("api/task-step-executions/{taskStepExecutionId:guid}/compliance-check")]
    [ApiController]
    public class ComplianceChecksController : ControllerBase
    {
        private readonly IComplianceCheckService _complianceCheckService;

        public ComplianceChecksController(IComplianceCheckService complianceCheckService)
        {
            _complianceCheckService = complianceCheckService;
        }

        /// <summary>
        /// Initiates an AI compliance check for the given step execution.
        /// </summary>
        /// <remarks>
        /// This endpoint is the single entry point for starting automated image scoring:
        /// <list type="number">
        ///   <item>Creates (or reuses) a <c>Pending</c> ComplianceCheck record.</item>
        ///   <item>Fetches the After-type images already uploaded for the execution.</item>
        ///   <item>Submits the images to the AI Scoring Module (async via RabbitMQ).</item>
        /// </list>
        /// When scoring completes the result is pushed to the client via SignalR
        /// on the <c>compliance-check-updated</c> method, group key = <c>taskStepExecutionId</c>.
        /// </remarks>
        [HttpPost]
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
            Guid taskStepExecutionId,
            CancellationToken ct = default)
        {
            if (taskStepExecutionId == Guid.Empty)
                return BadRequest("taskStepExecutionId cannot be empty.");

            try
            {
                var result = await _complianceCheckService
                    .InitiateAiCheckAsync(taskStepExecutionId, ct);

                return Accepted(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Returns the current AI compliance check status for the given step execution.
        /// </summary>
        /// <remarks>
        /// Polling fallback for clients that reconnect after a SignalR disconnect.
        /// Returns 404 if no AI compliance check has been initiated yet.
        /// </remarks>
        [HttpGet]
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
