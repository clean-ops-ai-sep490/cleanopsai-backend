using CleanOpsAi.Modules.ClientManagement.Application.Dtos.WorkareaDetails;
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
    public class WorkAreaDetailsController : ControllerBase
    {
        private readonly IWorkAreaDetailService _service;

        public WorkAreaDetailsController(IWorkAreaDetailService service)
        {
            _service = service;
        }

        [HttpGet("{id:guid}")]
        [Consumes("application/json")]
        [SwaggerOperation(
            Summary = "Get work area detail by id",
            Description = "Retrieve a specific work area detail using id.",
            Tags = new[] { "WorkAreaDetails" })]
        [SwaggerResponse(StatusCodes.Status200OK, "Found work area detail")]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Work area detail not found")]
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
            Summary = "Get all work area details",
            Description = "Retrieve all work area details with pagination.",
            Tags = new[] { "WorkAreaDetails" })]
        public async Task<IActionResult> GetAll(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            var result = await _service.GetAllPaginationAsync(pageNumber, pageSize);

            return Ok(result);
        }

        [HttpGet("work-area/{workAreaId:guid}")]
        [Consumes("application/json")]
        [SwaggerOperation(
            Summary = "Get work area details by workAreaId",
            Description = "Retrieve work area details belonging to a specific work area.",
            Tags = new[] { "WorkAreaDetails" })]
        public async Task<IActionResult> GetByWorkAreaId(
            Guid workAreaId,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            var result = await _service.GetByWorkAreaIdPaginationAsync(workAreaId, pageNumber, pageSize);

            return Ok(result);
        }

        [HttpPost]
        [Consumes("application/json")]
        [SwaggerOperation(
            Summary = "Create work area detail",
            Description = "Create a new work area detail.",
            Tags = new[] { "WorkAreaDetails" })]
        [SwaggerResponse(StatusCodes.Status200OK, "Create successfully")]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid request data")]
        public async Task<IActionResult> Create(WorkAreaDetailCreateRequest request)
        {
            var result = await _service.CreateAsync(request);

            return Ok(result);
        }

        [HttpPut("{id:guid}")]
        [Consumes("application/json")]
        [SwaggerOperation(
            Summary = "Update work area detail",
            Description = "Update name or area of a work area detail.",
            Tags = new[] { "WorkAreaDetails" })]
        [SwaggerResponse(StatusCodes.Status200OK, "Update successfully")]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Work area detail not found")]
        public async Task<IActionResult> Update(Guid id, WorkAreaDetailUpdateRequest request)
        {
            var result = await _service.UpdateAsync(id, request);

            if (result == null)
                return NotFound();

            return Ok(result);
        }

        [HttpDelete("{id:guid}")]
        [Consumes("application/json")]
        [SwaggerOperation(
            Summary = "Delete work area detail",
            Description = "Soft delete a work area detail by id.",
            Tags = new[] { "WorkAreaDetails" })]
        [SwaggerResponse(StatusCodes.Status200OK, "Delete successfully")]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Work area detail not found")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var result = await _service.DeleteAsync(id);

            if (result == 0)
                return NotFound();

            return Ok(result);
        }
    }
}
