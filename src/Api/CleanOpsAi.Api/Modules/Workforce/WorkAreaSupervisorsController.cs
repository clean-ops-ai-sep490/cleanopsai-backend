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

        //[HttpGet("{id:guid}")]
        //[Consumes("application/json")]
        //[SwaggerOperation(
        //    Summary = "Get WorkAreaSupervisor by id",
        //    Description = "Get supervisor information using id.",
        //    Tags = new[] { "WorkAreaSupervisors" })]
        //public async Task<IActionResult> GetById(Guid id)
        //{
        //    var result = await _service.GetByIdAsync(id);

        //    if (result == null)
        //        return NotFound();

        //    return Ok(result);
        //}

        //[HttpGet("user/{userId}")]
        //[Consumes("application/json")]
        //[SwaggerOperation(
        //    Summary = "Get WorkAreaSupervisor by user id",
        //    Description = "Get WorkAreaSupervisor using userId.",
        //    Tags = new[] { "WorkAreaSupervisors" })]
        //public async Task<IActionResult> GetByUserId(Guid userId)
        //{
        //    var result = await _service.GetByUserIdAsync(userId);

        //    return Ok(result);
        //}

        //[HttpGet("worker/{workerId:guid}")]
        //[Consumes("application/json")]
        //[SwaggerOperation(
        //    Summary = "Get WorkAreaSupervisor by worker id",
        //    Description = "Get supervisor using workerId.",
        //    Tags = new[] { "WorkAreaSupervisors" })]
        //public async Task<IActionResult> GetByWorkerIdPaging(
        //    Guid workerId,
        //    int pageNumber = 1,
        //    int pageSize = 10)
        //{
        //    var result = await _service.GetByWorkerIdPaginationAsync(workerId, pageNumber, pageSize);
        //    return Ok(result);
        //}

        //[HttpGet]
        //[Consumes("application/json")]
        //[SwaggerOperation(
        //    Summary = "Get all WorkAreaSupervisors",
        //    Description = "Get all WorkAreaSupervisors with pagination.",
        //    Tags = new[] { "WorkAreaSupervisors" })]
        //public async Task<IActionResult> GetAll(
        //    [FromQuery] int pageNumber = 1,
        //    [FromQuery] int pageSize = 10)
        //{
        //    var result = await _service.GetAllPaginationAsync(pageNumber, pageSize);

        //    return Ok(result);
        //}

        //[HttpGet("workarea/{workAreaId:guid}")]
        //[Consumes("application/json")]
        //[SwaggerOperation(
        //    Summary = "Get supervisors by work area",
        //    Description = "Get all supervisors in a work area.",
        //    Tags = new[] { "WorkAreaSupervisors" })]
        //public async Task<IActionResult> GetByWorkAreaId(Guid workAreaId)
        //{
        //    var result = await _service.GetByWorkAreaIdAsync(workAreaId);

        //    return Ok(result);
        //}



        [HttpPut("update-assignments")]
        [Consumes("application/json")]
        [SwaggerOperation(
            Summary = "Update worker assignments of supervisor in work area",
            Description = "Replace all current worker assignments of a supervisor in a work area with the new list.",
            Tags = new[] { "WorkAreaSupervisors" })]
        [SwaggerResponse(StatusCodes.Status200OK, "Update successfully")]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid request data")]
        public async Task<IActionResult> UpdateAssignments([FromBody] WorkAreaSupervisorUpdateRequest request)
        {
            var result = await _service.UpdateAsync(request);
            return Ok(result);
        }

        [HttpDelete("{id:guid}")]
        [Consumes("application/json")]
        [SwaggerOperation(
            Summary = "Delete WorkAreaSupervisor",
            Description = "Delete WorkAreaSupervisor by id.",
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
            Summary = "Get latest GPS of workers in work area Có GPS trong 10 phút gần nhất offlineThresholdMinutes là tính online",
            Description = "Get latest GPS of all workers in a work area with paging.",
            Tags = new[] { "WorkAreaSupervisors" })]
        public async Task<IActionResult> GetWorkersLatestGps(
            Guid workAreaId,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] int offlineThresholdMinutes = 10)
        {
            var result = await _service.GetWorkersLiveStatusByWorkAreaPagingAsync(
                workAreaId,
                pageNumber,
                pageSize,
                offlineThresholdMinutes);

            return Ok(result);
        }

        [HttpPost("assign")]
        [Consumes("application/json")]
        [SwaggerOperation(
            Summary = "Assign workers to supervisor in work area",
            Description = "Assign a list of workers to a supervisor in a specific work area.",
            Tags = new[] { "WorkAreaSupervisors" })]
        public async Task<IActionResult> AssignWorkers(
            [FromBody] WorkAreaSupervisorAssignRequest request)
        {
            var result = await _service.AssignWorkersAsync(request);
            return Ok(result);
        }

        [HttpDelete("unassign")]
        [Consumes("application/json")]
        [SwaggerOperation(
            Summary = "Unassign a worker from supervisor in work area",
            Description = "Remove a specific worker assignment from a supervisor in a work area.",
            Tags = new[] { "WorkAreaSupervisors" })]
        public async Task<IActionResult> UnassignWorker(
            [FromQuery] Guid workAreaId,
            [FromQuery] Guid userId,
            [FromQuery] Guid workerId)
        {
            var result = await _service.UnassignWorkerAsync(workAreaId, userId, workerId);
            if (result > 0)
                return Ok();
            return NotFound();
        }

        //[HttpGet("by-workarea-worker")]
        //[SwaggerOperation(
        //    Summary = "Get supervisors by work area",
        //    Description = "Get all supervisors in a work area.",
        //    Tags = new[] { "WorkAreaSupervisors" })]
        //public async Task<IActionResult> GetByWorkAreaAndWorker(Guid workAreaId, Guid workerId)
        //{
        //    var result = await _service.GetSupervisorByWorkAreaAndWorkerAsync(workAreaId, workerId);

        //    if (result == null)
        //        return NotFound("Không tìm thấy supervisor.");

        //    return Ok(result);
        //}

        [HttpGet("workareas/{supervisorId}")]
        [SwaggerOperation(
            Summary = "Get work areas by supervisor",
            Description = "Get all work area of supervisor.",
            Tags = new[] { "WorkAreaSupervisors" })]
        public async Task<IActionResult> GetPaging(
            Guid supervisorId,
            int pageNumber = 1,
            int pageSize = 10)
        {
            var result = await _service
                .GetWorkAreasBySupervisorPaginationAsync(supervisorId, pageNumber, pageSize);

            return Ok(result);
        }

        [HttpGet("supervisors/{supervisorId}/workers")]
        [SwaggerOperation(
            Summary = "Get worker by supervisor",
            Description = "Get all worker of supervisor.",
            Tags = new[] { "WorkAreaSupervisors" })]
        public async Task<IActionResult> GetWorkersPaging(
            Guid supervisorId,
            int pageNumber = 1,
            int pageSize = 10)
        {
            var result = await _service.GetUniqueWorkersBySupervisorPagingAsync(
                supervisorId, pageNumber, pageSize);

            return Ok(result);
        }

        [HttpGet("workareas/{workAreaId}/workers")]
        [SwaggerOperation(
            Summary = "Get worker by workAreaId",
            Description = "Get all worker of workAreaId.",
            Tags = new[] { "WorkAreaSupervisors" })]
        public async Task<IActionResult> GetWorkersOfWorkareaUserContextPaging(
            Guid workAreaId,
            int pageNumber = 1,
            int pageSize = 10)
        {
            var result = await _service.GetWorkersByWorkAreaPagingAsync(
                workAreaId,
                pageNumber,
                pageSize);

            return Ok(result);
        }

    }
}
