using CleanOpsAi.Modules.Workforce.Application.Dtos.WorkerGps;
using CleanOpsAi.Modules.Workforce.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace CleanOpsAi.Api.Modules.Workforce
{
    [Route("api/[controller]")]
    [ApiController]
    public class WorkerGpsController : ControllerBase
    {
        private readonly IWorkerGpsService _service;

        public WorkerGpsController(IWorkerGpsService service)
        {
            _service = service;
        }

        [HttpGet("{id:guid}")]
        [Consumes("application/json")]
        [SwaggerOperation(
            Summary = "Get worker GPS by id",
            Description = "Get worker GPS record by id.",
            Tags = new[] { "WorkerGps" })]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _service.GetByIdAsync(id);
            if (result == null)
                return NotFound();
            return Ok(result);
        }

        [HttpGet]
        [Consumes("application/json")]
        [SwaggerOperation(
            Summary = "Get all worker GPS",
            Description = "Get all worker GPS records with pagination.",
            Tags = new[] { "WorkerGps" })]
        public async Task<IActionResult> GetAll(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            var result = await _service.GetAllPaginationAsync(pageNumber, pageSize);
            return Ok(result);
        }

        [HttpGet("worker/{workerId:guid}/latest")]
        [Consumes("application/json")]
        [SwaggerOperation(
            Summary = "Get latest GPS of a worker",
            Description = "Get the most recent GPS record of a specific worker.",
            Tags = new[] { "WorkerGps" })]
        public async Task<IActionResult> GetLatestByWorkerId(Guid workerId)
        {
            var result = await _service.GetLatestByWorkerIdAsync(workerId);
            if (result == null)
                return NotFound();
            return Ok(result);
        }

        [HttpGet("worker/{workerId:guid}/history")]
        [Consumes("application/json")]
        [SwaggerOperation(
            Summary = "Get GPS history of a worker",
            Description = "Get paginated GPS history of a specific worker.",
            Tags = new[] { "WorkerGps" })]
        public async Task<IActionResult> GetHistoryByWorkerId(
            Guid workerId,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            var result = await _service.GetByWorkerIdPaginationAsync(workerId, pageNumber, pageSize);
            return Ok(result);
        }

        [HttpPost("workers/latest")]
        [Consumes("application/json")]
        [SwaggerOperation(
            Summary = "Get latest GPS of multiple workers",
            Description = "Get the most recent GPS record for each worker in the provided list.",
            Tags = new[] { "WorkerGps" })]
        public async Task<IActionResult> GetLatestByWorkerIds([FromBody] List<Guid> workerIds)
        {
            if (workerIds == null || !workerIds.Any())
                return BadRequest("Worker ids are required.");

            var result = await _service.GetLatestByWorkerIdsAsync(workerIds);
            return Ok(result);
        }

        [HttpPost]
        [Consumes("application/json")]
        [SwaggerOperation(
            Summary = "Create worker GPS",
            Description = "Create new worker GPS record.",
            Tags = new[] { "WorkerGps" })]
        public async Task<IActionResult> Create([FromBody] WorkerGpsCreateRequest request)
        {
            var result = await _service.CreateAsync(request);
            return Ok(result);
        }
    }
}
