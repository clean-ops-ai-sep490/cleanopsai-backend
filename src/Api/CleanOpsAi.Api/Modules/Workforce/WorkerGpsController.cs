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
            Description = "Get worker GPS record using id.",
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

        [HttpPost]
        [Consumes("application/json")]
        [SwaggerOperation(
            Summary = "Create worker GPS",
            Description = "Create new worker GPS record.",
            Tags = new[] { "WorkerGps" })]
        public async Task<IActionResult> Create(WorkerGpsCreateRequest request)
        {
            var result = await _service.CreateAsync(request);

            if (result != null)
                return Ok(result);

            return BadRequest();
        }

        [HttpPut("{id}")]
        [Consumes("application/json")]
        [SwaggerOperation(
            Summary = "Update worker GPS",
            Description = "Update worker GPS location.",
            Tags = new[] { "WorkerGps" })]
        public async Task<IActionResult> Update(Guid id, WorkerGpsUpdateRequest request)
        {
            var result = await _service.UpdateAsync(id, request);

            if (result != null)
                return Ok(result);

            return NotFound();
        }

        [HttpDelete("{id}")]
        [Consumes("application/json")]
        [SwaggerOperation(
            Summary = "Delete worker GPS",
            Description = "Delete worker GPS record.",
            Tags = new[] { "WorkerGps" })]
        public async Task<IActionResult> Delete(Guid id)
        {
            var result = await _service.DeleteAsync(id);

            if (result > 0)
                return Ok(result);

            return NotFound();
        }
    }
}
