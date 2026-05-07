using CleanOpsAi.Api.Modules.Workforce.Dtos;
using CleanOpsAi.Modules.Workforce.Application.Dtos.Workers;
using CleanOpsAi.Modules.Workforce.Application.Interfaces;
using CleanOpsAi.Modules.Workforce.Application.Services;
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
        public async Task<IActionResult> GetByUserId(Guid userId)
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
                FullName = request.FullName,
                DisplayAddress = request.Address,
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

        [HttpGet("filter")]
        [SwaggerOperation(
            Summary = "Filter worker",
            Description = "Filter worker by certi, skill, location, and start date end date to find who not busy",
            Tags = new[] { "Workers" })]
        public async Task<IActionResult> Filter(
            [FromQuery] string? address,
            [FromQuery] List<Guid>? skillIds,
            [FromQuery] List<Guid>? certificateIds,
            [FromQuery] DateTimeOffset? startAt,
            [FromQuery] DateTimeOffset? endAt)
        {
            var request = new WorkerFilterRequest
            {
                Address = address,
                SkillIds = skillIds,
                CertificateIds = certificateIds,
                StartAt = startAt?.UtcDateTime,
                EndAt = endAt?.UtcDateTime
            };
            var result = await _service.FilterAsync(request);
            return Ok(result);
        }

        [HttpGet("nlp-filter")]
        [SwaggerOperation(
            Summary = "NLP search worker",
            Description = "Search worker bằng ngôn ngữ tự nhiên. VD: 'Tìm người  có skill chemical co chứng chỉ management ở Phường Ba Đình'. Warning khi có khoảng  thời gian thì phải có thời gian bắt đầu và thời gian kết thúc. Ghi chú: rất hay bị 400 do Gemini bị nghẽn do sử dụng key free ",
            Tags = new[] { "Workers" })]
        [SwaggerResponse(StatusCodes.Status200OK, "Danh sách worker phù hợp")]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Query không hợp lệ")]
        public async Task<IActionResult> NlpFilter([FromQuery] string? search)
        {
            Console.WriteLine(">>> CONTROLLER HIT");
            var result = await _service.NlpFilterAsync(search);
            Console.WriteLine(">>> CONTROLLER DONE");
            return Ok(result);
        }

    }
}
