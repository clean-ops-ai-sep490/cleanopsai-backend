using CleanOpsAi.Modules.Workforce.Application.Dtos.Dashboard;
using CleanOpsAi.Modules.Workforce.Application.Dtos.WorkAreaSupervisors;
using CleanOpsAi.Modules.Workforce.Application.Dtos;
using CleanOpsAi.Modules.Workforce.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace CleanOpsAi.Api.Modules.Workforce
{
    [Route("api/dashboard")]
    [ApiController]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService _dashboardService;

        public DashboardController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        [HttpGet("worker-total")]
        [SwaggerOperation(
            Summary = "Get total workers",
            Description = "Returns total active workers in the system.",
            Tags = new[] { "Dashboard" })]
        [ProducesResponseType(typeof(DashboardWorkerTotalResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetWorkerTotal(CancellationToken ct)
        {
            var result = await _dashboardService.GetWorkerTotalAsync(ct);
            return Ok(result);
        }

        [HttpGet("workers-by-supervisor/{supervisorId:guid}")]
        [SwaggerOperation(
            Summary = "Get workers by supervisor",
            Description = "Returns paged unique workers managed by a supervisor.",
            Tags = new[] { "Dashboard" })]
        [ProducesResponseType(typeof(PagedResponse<WorkerGroupResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetWorkersBySupervisor(
            Guid supervisorId,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            CancellationToken ct = default)
        {
            var result = await _dashboardService.GetWorkersBySupervisorAsync(
                supervisorId,
                pageNumber,
                pageSize,
                ct);
            return Ok(result);
        }

        [HttpGet("task-summary")]
        [SwaggerOperation(
            Summary = "Get task summary",
            Description = "Returns total tasks to current time, passed tasks to current time, and non-passed tasks to current time.",
            Tags = new[] { "Dashboard" })]
        [ProducesResponseType(typeof(DashboardTaskSummaryResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetTaskSummary(CancellationToken ct)
        {
            var result = await _dashboardService.GetTaskSummaryAsync(ct);
            return Ok(result);
        }

        [HttpGet("task-status-counts")]
        [SwaggerOperation(
            Summary = "Get task status counts",
            Description = "Returns task counts grouped by status.",
            Tags = new[] { "Dashboard" })]
        [ProducesResponseType(typeof(List<DashboardStatusCountResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetTaskStatusCounts(CancellationToken ct)
        {
            var result = await _dashboardService.GetTaskStatusCountsAsync(ct);
            return Ok(result);
        }

        [HttpGet("top-workers")]
        [SwaggerOperation(
            Summary = "Get top workers",
            Description = "Returns top 5 workers with the most passed tasks in the current month.",
            Tags = new[] { "Dashboard" })]
        [ProducesResponseType(typeof(List<DashboardWorkerTaskResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetTopWorkers(CancellationToken ct)
        {
            var result = await _dashboardService.GetTopWorkersAsync(ct);
            return Ok(result);
        }

        [HttpGet("low-workers")]
        [SwaggerOperation(
            Summary = "Get low workers",
            Description = "Returns top 5 workers with the fewest passed tasks in the current month.",
            Tags = new[] { "Dashboard" })]
        [ProducesResponseType(typeof(List<DashboardWorkerTaskResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetLowWorkers(CancellationToken ct)
        {
            var result = await _dashboardService.GetLowWorkersAsync(ct);
            return Ok(result);
        }

        [HttpGet("work-area-stats")]
        [SwaggerOperation(
            Summary = "Get work area stats",
            Description = "Returns number of workers and tasks in each work area.",
            Tags = new[] { "Dashboard" })]
        [ProducesResponseType(typeof(List<DashboardWorkAreaResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetWorkAreaStats(CancellationToken ct)
        {
            var result = await _dashboardService.GetWorkAreaStatsAsync(ct);
            return Ok(result);
        }

        [HttpGet("ai-compliance-rate")]
        [SwaggerOperation(
            Summary = "Get AI compliance pass fail rate",
            Description = "Returns AI automated compliance check pass and fail counts with percentages.",
            Tags = new[] { "Dashboard" })]
        [ProducesResponseType(typeof(DashboardAiComplianceRateResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAiComplianceRate(CancellationToken ct)
        {
            var result = await _dashboardService.GetAiComplianceRateAsync(ct);
            return Ok(result);
        }
    }
}
