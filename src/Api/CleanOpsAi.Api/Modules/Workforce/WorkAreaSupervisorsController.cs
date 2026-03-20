using CleanOpsAi.Modules.Workforce.Application.Dtos.WorkAreaSupervisors;
using CleanOpsAi.Modules.Workforce.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace CleanOpsAi.Api.Modules.Workforce
{
    [Route("api/[controller]")]
    [ApiController]
    public class WorkAreaSupervisorsController : ControllerBase
    {
        private readonly IWorkAreaSupervisorService _service;

        public WorkAreaSupervisorsController(IWorkAreaSupervisorService service)
        {
            _service = service;
        }

        [HttpGet("{id:guid}")]
        [Consumes("application/json")]
        [SwaggerOperation(
            Summary = "Get WorkAreaSupervisor by id",
            Description = "Get supervisor information using id.",
            Tags = new[] { "WorkAreaSupervisors" })]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _service.GetByIdAsync(id);

            if (result == null)
                return NotFound();

            return Ok(result);
        }

        [HttpGet("user/{userId}")]
        [Consumes("application/json")]
        [SwaggerOperation(
            Summary = "Get WorkAreaSupervisor by user id",
            Description = "Get supervisor using userId.",
            Tags = new[] { "WorkAreaSupervisors" })]
        public async Task<IActionResult> GetByUserId(string userId)
        {
            var result = await _service.GetByUserIdAsync(userId);

            if (result == null)
                return NotFound();

            return Ok(result);
        }

        [HttpGet("worker/{workerId:guid}")]
        [Consumes("application/json")]
        [SwaggerOperation(
            Summary = "Get WorkAreaSupervisor by worker id",
            Description = "Get supervisor using workerId.",
            Tags = new[] { "WorkAreaSupervisors" })]
        public async Task<IActionResult> GetByWorkerId(Guid workerId)
        {
            var result = await _service.GetByWorkerIdAsync(workerId);

            if (result == null)
                return NotFound();

            return Ok(result);
        }

        [HttpGet]
        [Consumes("application/json")]
        [SwaggerOperation(
            Summary = "Get all WorkAreaSupervisors",
            Description = "Get all supervisors with pagination.",
            Tags = new[] { "WorkAreaSupervisors" })]
        public async Task<IActionResult> GetAll(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            var result = await _service.GetAllPaginationAsync(pageNumber, pageSize);

            return Ok(result);
        }

        [HttpGet("workarea/{workAreaId:guid}")]
        [Consumes("application/json")]
        [SwaggerOperation(
            Summary = "Get supervisors by work area",
            Description = "Get all supervisors in a work area.",
            Tags = new[] { "WorkAreaSupervisors" })]
        public async Task<IActionResult> GetByWorkAreaId(Guid workAreaId)
        {
            var result = await _service.GetByWorkAreaIdAsync(workAreaId);

            return Ok(result);
        }

        [HttpPost]
        [Consumes("application/json")]
        [SwaggerOperation(
            Summary = "Create WorkAreaSupervisor",
            Description = "Create new supervisor.",
            Tags = new[] { "WorkAreaSupervisors" })]
        [SwaggerResponse(StatusCodes.Status200OK, "Create successfully")]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid request data")]
        public async Task<IActionResult> Create(WorkAreaSupervisorCreateRequest request)
        {
            var result = await _service.CreateAsync(request);

            if (result != null)
                return Ok(result);

            return BadRequest();
        }

        [HttpPut("{id:guid}")]
        [Consumes("application/json")]
        [SwaggerOperation(
            Summary = "Update WorkAreaSupervisor",
            Description = "Update supervisor information.",
            Tags = new[] { "WorkAreaSupervisors" })]
        [SwaggerResponse(StatusCodes.Status200OK, "Update successfully")]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Supervisor not found")]
        public async Task<IActionResult> Update(Guid id, WorkAreaSupervisorUpdateRequest request)
        {
            var result = await _service.UpdateAsync(id, request);

            if (result != null)
                return Ok(result);

            return NotFound();
        }

        [HttpDelete("{id:guid}")]
        [Consumes("application/json")]
        [SwaggerOperation(
            Summary = "Delete WorkAreaSupervisor",
            Description = "Delete supervisor by id.",
            Tags = new[] { "WorkAreaSupervisors" })]
        [SwaggerResponse(StatusCodes.Status200OK, "Delete successfully")]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Supervisor not found")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var result = await _service.DeleteAsync(id);

            if (result > 0)
                return Ok(result);

            return NotFound();
        }

        // Gps

        [HttpGet("workarea/{workAreaId:guid}/workers/gps")]
        [Consumes("application/json")]
        [SwaggerOperation(
            Summary = "Get latest GPS of workers in work area",
            Description = "Get latest GPS of all workers in a work area.",
            Tags = new[] { "WorkAreaSupervisors" })]
        public async Task<IActionResult> GetWorkersLatestGps(Guid workAreaId)
        {
            var result = await _service.GetWorkersLatestGpsByWorkAreaIdAsync(workAreaId);

            return Ok(result);
        }
    }
}
