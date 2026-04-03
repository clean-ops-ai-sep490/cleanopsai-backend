using CleanOpsAi.Modules.ClientManagement.Application.Dtos.Workareas;
using CleanOpsAi.Modules.ClientManagement.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace CleanOpsAi.Api.Modules.ClientManagement
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Manager, Admin")]
    public class WorkAreasController : ControllerBase
    {
        private readonly IWorkAreaService _service;

        public WorkAreasController(IWorkAreaService service)
        {
            _service = service;
        }

        [HttpGet("{id:guid}")]
        [SwaggerOperation(
            Summary = "Get work area by id",
            Description = "Get a work area using workAreaId.",
            Tags = new[] { "WorkAreas" })]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _service.GetByIdAsync(id);

            if (result == null)
                return NotFound();

            return Ok(result);
        }

        [HttpGet]
        [SwaggerOperation(
            Summary = "Get all work areas",
            Description = "Get all work areas with pagination.",
            Tags = new[] { "WorkAreas" })]
        public async Task<IActionResult> GetAll(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            var result = await _service.GetAllPaginationAsync(pageNumber, pageSize);

            return Ok(result);
        }

        [HttpGet("zone/{zoneId:guid}")]
        [SwaggerOperation(
            Summary = "Get work areas by zoneId",
            Description = "Get work areas of a zone with pagination.",
            Tags = new[] { "WorkAreas" })]
        public async Task<IActionResult> GetByZoneId(
            Guid zoneId,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            var result = await _service.GetByZoneIdPaginationAsync(zoneId, pageNumber, pageSize);

            return Ok(result);
        }

		[Authorize]
		[HttpPost]
        [SwaggerOperation(
            Summary = "Create work area",
            Description = "Create a new work area.",
            Tags = new[] { "WorkAreas" })]
        public async Task<IActionResult> Create(WorkAreaCreateRequest request)
        {
            var result = await _service.CreateAsync(request);

            if (result != null)
                return Ok(result);

            return BadRequest();
        }

		[Authorize]
		[HttpPut("{id}")]
        [SwaggerOperation(
            Summary = "Update work area",
            Description = "Update work area name.",
            Tags = new[] { "WorkAreas" })]
        public async Task<IActionResult> Update(Guid id, WorkAreaUpdateRequest request)
        {
            var result = await _service.UpdateAsync(id, request);

            if (result != null)
                return Ok(result);

            return NotFound();
        }

		[Authorize]
		[HttpDelete("{id}")]
        [SwaggerOperation(
            Summary = "Delete work area",
            Description = "Soft delete work area.",
            Tags = new[] { "WorkAreas" })]
        public async Task<IActionResult> Delete(Guid id)
        {
            var result = await _service.DeleteAsync(id);

            if (result > 0)
                return Ok(result);

            return NotFound();
        }

    }
}
