using CleanOpsAi.Modules.ClientManagement.Application.Dtos.Slas;
using CleanOpsAi.Modules.ClientManagement.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace CleanOpsAi.Api.Modules.ClientManagement
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Manager")]
    public class SlasController : ControllerBase
    {
        private readonly ISlaService _service;

        public SlasController(ISlaService service)
        {
            _service = service;
        }

        [HttpGet("{id:guid}")]
        [Consumes("application/json")]
        [SwaggerOperation(
            Summary = "Get SLA by id",
            Description = "Get SLA using slaId.",
            Tags = new[] { "Slas" })]
        public async Task<IActionResult> GetById(Guid id)
        {
            var sla = await _service.GetByIdAsync(id);

            if (sla == null)
                return NotFound();

            return Ok(sla);
        }

        [HttpGet]
        [Consumes("application/json")]
        [SwaggerOperation(
            Summary = "Get all SLAs",
            Description = "Get all SLAs with pagination.",
            Tags = new[] { "Slas" })]
        public async Task<IActionResult> GetAll(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            var result = await _service.GetAllPaginationAsync(pageNumber, pageSize);

            return Ok(result);
        }

        [HttpGet("filter")]
        [Consumes("application/json")]
        [SwaggerOperation(
            Summary = "Filter SLA",
            Description = "Filter SLA by workAreaId, contractId, or both.",
            Tags = new[] { "Slas" })]
        public async Task<IActionResult> Filter(
            [FromQuery] Guid? workAreaId,
            [FromQuery] Guid? contractId,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            var result = await _service
                .FilterAsync(workAreaId, contractId, pageNumber, pageSize);

            return Ok(result);
        }

        [HttpPost]
        [Consumes("application/json")]
        [SwaggerOperation(
            Summary = "Create SLA",
            Description = "Create a new SLA. Enviroment Type: 0.Hospital 1.Office 2.School. Service Type: 0.Cleaning 1.Maintain",
            Tags = new[] { "Slas" })]
        [SwaggerResponse(StatusCodes.Status200OK, "Create successfully")]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid request data")]
        public async Task<IActionResult> Create(SlaCreateRequest request)
        {
            var result = await _service.CreateAsync(request);

            if (result != null)
                return Ok(result);

            return BadRequest();
        }

        [HttpPut("{id}")]
        [Consumes("application/json")]
        [SwaggerOperation(
            Summary = "Update SLA",
            Description = "Update SLA information. Enviroment Type: 0.Hospital 1.Office 2.School. Service Type: 0.Cleaning 1.Maintain",
            Tags = new[] { "Slas" })]
        [SwaggerResponse(StatusCodes.Status200OK, "Update successfully")]
        [SwaggerResponse(StatusCodes.Status404NotFound, "SLA not found")]
        public async Task<IActionResult> Update(Guid id, SlaUpdateRequest request)
        {
            var result = await _service.UpdateAsync(id, request);

            if (result != null)
                return Ok(result);

            return NotFound();
        }

        [HttpDelete("{id}")]
        [Consumes("application/json")]
        [SwaggerOperation(
            Summary = "Delete SLA",
            Description = "Delete SLA by id.",
            Tags = new[] { "Slas" })]
        [SwaggerResponse(StatusCodes.Status200OK, "Delete successfully")]
        [SwaggerResponse(StatusCodes.Status404NotFound, "SLA not found")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var result = await _service.DeleteAsync(id);

            if (result > 0)
                return Ok(result);

            return NotFound();
        }
    }
}

