using CleanOpsAi.Modules.Workforce.Application.Dtos.Certifications;
using CleanOpsAi.Modules.Workforce.Application.Interfaces;
using CleanOpsAi.Modules.Workforce.Application.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace CleanOpsAi.Api.Modules.Workforce
{
    [Route("api/[controller]")]
    [ApiController]
    public class CertificationsController : ControllerBase
    {
        private readonly ICertificationService _service;

        public CertificationsController(ICertificationService service)
        {
            _service = service;
        }

        [HttpGet("{id:guid}")]
        [Consumes("application/json")]
        [SwaggerOperation(
            Summary = "Get certification by id",
            Description = "Get certification information using certificationId.",
            Tags = new[] { "Certifications" })]
        public async Task<IActionResult> GetById(Guid id)
        {
            var certification = await _service.GetByIdAsync(id);

            if (certification == null)
                return NotFound();

            return Ok(certification);
        }

        [HttpGet]
        [Consumes("application/json")]
        [SwaggerOperation(
            Summary = "Get all certifications",
            Description = "Get all certifications with pagination.",
            Tags = new[] { "Certifications" })]
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
            Summary = "Create certification",
            Description = "Create new certification.",
            Tags = new[] { "Certifications" })]
        [SwaggerResponse(StatusCodes.Status200OK, "Create successfully")]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid request data")]
        public async Task<IActionResult> Create(CertificationCreateRequest request)
        {
            var result = await _service.CreateAsync(request);

            if (result != null)
                return Ok(result);

            return BadRequest();
        }

        [HttpPut("{id}")]
        [Consumes("application/json")]
        [SwaggerOperation(
            Summary = "Update certification",
            Description = "Update certification information.",
            Tags = new[] { "Certifications" })]
        [SwaggerResponse(StatusCodes.Status200OK, "Update successfully")]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Certification not found")]
        public async Task<IActionResult> Update(Guid id, CertificationUpdateRequest request)
        {
            var result = await _service.UpdateAsync(id, request);

            if (result != null)
                return Ok(result);

            return NotFound();
        }

        [HttpDelete("{id}")]
        [Consumes("application/json")]
        [SwaggerOperation(
            Summary = "Delete certification",
            Description = "Delete certification by id.",
            Tags = new[] { "Certifications" })]
        [SwaggerResponse(StatusCodes.Status200OK, "Delete successfully")]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Certification not found")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var result = await _service.DeleteAsync(id);

            if (result > 0)
                return Ok(result);

            return NotFound();
        }

        [HttpGet("categories")]
        [Consumes("application/json")]
        [SwaggerOperation(
            Summary = "Categories certification",
            Description = "list all category ",
            Tags = new[] { "Certifications" })]
        public async Task<IActionResult> GetCategories()
        {
            var result = await _service.GetAllCategoriesAsync();
            return Ok(result);
        }

        [HttpGet("by-category")]
        [Consumes("application/json")]
        [SwaggerOperation(
            Summary = "List all certificate of Category",
            Description = "list all certificate of category ",
            Tags = new[] { "Certifications" })]
        public async Task<IActionResult> GetByCategory([FromQuery] string category)
        {
            var result = await _service.GetByCategoryAsync(category);
            return Ok(result);
        }

        [HttpGet("worker/{workerId}/certifications")]
        [Consumes("application/json")]
        [SwaggerOperation(
            Summary = "List all certificate of worker",
            Description = "list all certificate of worker ",
            Tags = new[] { "Certifications" })]
        public async Task<IActionResult> GetCertificationsByWorker(Guid workerId)
        {
            var result = await _service.GetCertificationsByWorkerIdAsync(workerId);
            return Ok(result);
        }

    }
}
