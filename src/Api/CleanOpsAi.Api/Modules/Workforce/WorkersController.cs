using CleanOpsAi.Api.Modules.Workforce.Dtos;
using CleanOpsAi.Modules.Workforce.Application.Dtos.Workers;
using CleanOpsAi.Modules.Workforce.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace CleanOpsAi.Api.Modules.Workforce
{
    [Route("api/[controller]")]
    [ApiController]
    public class WorkersController : ControllerBase
    {
        private readonly IWorkerService _service;

        public WorkersController(IWorkerService service)
        {
            _service = service;
        }

        [HttpGet("{id:guid}")]
        [Consumes("application/json")]
        [SwaggerOperation(
            Summary = "Get Worker by id",
            Description = "Get worker information using workerId.",
            Tags = new[] { "Workers" })]
        public async Task<IActionResult> GetById(Guid id)
        {
            var worker = await _service.GetByIdAsync(id);

            if (worker == null)
                return NotFound();

            return Ok(worker);
        }

        [HttpGet("user/{userId}")]
        [Consumes("application/json")]
        [SwaggerOperation(
            Summary = "Get Worker by user id",
            Description = "Get worker information using userId.",
            Tags = new[] { "Workers" })]
        public async Task<IActionResult> GetByUserId(string userId)
        {
            var worker = await _service.GetByUserIdAsync(userId);

            if (worker == null)
                return NotFound();

            return Ok(worker);
        }

        [HttpGet("me")]
        [Consumes("application/json")]
        [SwaggerOperation(
            Summary = "Get current Worker",
            Description = "Get current worker information.",
            Tags = new[] { "Workers" })]
        public async Task<IActionResult> GetInfor()
        {
            var worker = await _service.GetInforAsync();

            if (worker == null)
                return NotFound();

            return Ok(worker);
        }

        [HttpGet]
        [Consumes("application/json")]
        [SwaggerOperation(
            Summary = "Get all workers",
            Description = "Get all workers with pagination.",
            Tags = new[] { "Workers" })]
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
            Summary = "Create worker",
            Description = "Create new worker.",
            Tags = new[] { "Workers" })]
        [SwaggerResponse(StatusCodes.Status200OK, "Create successfully")]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid request data")]
        public async Task<IActionResult> Create(WorkerCreateRequest request)
        {
            var result = await _service.CreateAsync(request);

            if (result != null)
                return Ok(result);

            return BadRequest();
        }

        [HttpPut("{id}")]
        [Consumes("multipart/form-data")]
        [SwaggerOperation(
    Summary = "Update worker",
    Description = "Update worker information and avatar.",
    Tags = new[] { "Workers" })]
        [SwaggerResponse(StatusCodes.Status200OK, "Update successfully")]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Worker not found")]
        public async Task<IActionResult> Update(Guid id, [FromForm] UpdateWorkerApiRequest request)
        {
            var command = new WorkerUpdateRequest
            {
                DisplayAddress = request.DisplayAddress,
                Latitude = request.Latitude,
                Longitude = request.Longitude,
                AvatarStream = request.Avatar?.OpenReadStream(),
                AvatarFileName = request.Avatar?.FileName
            };

            var result = await _service.UpdateAsync(id, command);

            if (result != null)
                return Ok(result);

            return NotFound();
        }

        [HttpDelete("{id}")]
        [Consumes("application/json")]
        [SwaggerOperation(
            Summary = "Delete worker",
            Description = "Delete worker by id.",
            Tags = new[] { "Workers" })]
        [SwaggerResponse(StatusCodes.Status200OK, "Delete successfully")]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Worker not found")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var result = await _service.DeleteAsync(id);

            if (result > 0)
                return Ok(result);

            return NotFound();
        }
    }
}
