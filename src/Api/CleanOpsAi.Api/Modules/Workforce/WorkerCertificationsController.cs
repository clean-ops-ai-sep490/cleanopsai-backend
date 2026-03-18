using CleanOpsAi.Modules.Workforce.Application.Dtos.WorkerCertifications;
using CleanOpsAi.Modules.Workforce.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace CleanOpsAi.Api.Modules.Workforce
{
    [Route("api/[controller]")]
    [ApiController]
    public class WorkerCertificationsController : ControllerBase
    {
        private readonly IWorkerCertificationService _service;

        public WorkerCertificationsController(IWorkerCertificationService service)
        {
            _service = service;
        }

        [HttpGet("{workerId:guid}/{certificationId:guid}")]
        [Consumes("application/json")]
        [SwaggerOperation(
            Summary = "Get worker certification",
            Description = "Get worker certification using workerId and certificationId.",
            Tags = new[] { "WorkerCertifications" })]
        public async Task<IActionResult> GetById(Guid workerId, Guid certificationId)
        {
            var result = await _service.GetByIdAsync(workerId, certificationId);

            if (result == null)
                return NotFound();

            return Ok(result);
        }

        [HttpGet]
        [Consumes("application/json")]
        [SwaggerOperation(
            Summary = "Get all worker certifications",
            Description = "Get all worker certifications with pagination.",
            Tags = new[] { "WorkerCertifications" })]
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
            Summary = "Assign certification to worker",
            Description = "Create a worker certification.",
            Tags = new[] { "WorkerCertifications" })]
        [SwaggerResponse(StatusCodes.Status200OK, "Create successfully")]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid request data")]
        public async Task<IActionResult> Create(WorkerCertificationCreateRequest request)
        {
            var result = await _service.CreateAsync(request);

            if (result != null)
                return Ok(result);

            return BadRequest();
        }

        [HttpPut("{workerId:guid}/{certificationId:guid}")]
        [Consumes("application/json")]
        [SwaggerOperation(
            Summary = "Update worker certification",
            Description = "Update worker certification information.",
            Tags = new[] { "WorkerCertifications" })]
        [SwaggerResponse(StatusCodes.Status200OK, "Update successfully")]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Worker certification not found")]
        public async Task<IActionResult> Update(
            Guid workerId,
            Guid certificationId,
            WorkerCertificationUpdateRequest request)
        {
            var result = await _service.UpdateAsync(workerId, certificationId, request);

            if (result != null)
                return Ok(result);

            return NotFound();
        }

        [HttpDelete("{workerId:guid}/{certificationId:guid}")]
        [Consumes("application/json")]
        [SwaggerOperation(
            Summary = "Delete worker certification",
            Description = "Remove certification from worker.",
            Tags = new[] { "WorkerCertifications" })]
        [SwaggerResponse(StatusCodes.Status200OK, "Delete successfully")]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Worker certification not found")]
        public async Task<IActionResult> Delete(Guid workerId, Guid certificationId)
        {
            var result = await _service.DeleteAsync(workerId, certificationId);

            if (result > 0)
                return Ok(result);

            return NotFound();
        }
    }
}
