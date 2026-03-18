using CleanOpsAi.Modules.ClientManagement.Application.Dtos.SlaShifts;
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
    public class SlaShiftsController : ControllerBase
    {
        private readonly ISlaShiftService _service;

        public SlaShiftsController(ISlaShiftService service)
        {
            _service = service;
        }

        [HttpGet("{id:guid}")]
        [Consumes("application/json")]
        [SwaggerOperation(
            Summary = "Get SLA Shift by id",
            Description = "Get SLA Shift using shiftId.",
            Tags = new[] { "SlaShifts" })]
        public async Task<IActionResult> GetById(Guid id)
        {
            var shift = await _service.GetByIdAsync(id);

            if (shift == null)
                return NotFound();

            return Ok(shift);
        }

        [HttpGet]
        [Consumes("application/json")]
        [SwaggerOperation(
            Summary = "Get all SLA Shifts",
            Description = "Get all SLA Shifts with pagination.",
            Tags = new[] { "SlaShifts" })]
        public async Task<IActionResult> GetAll(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            var result = await _service.GetAllPaginationAsync(pageNumber, pageSize);

            return Ok(result);
        }

        [HttpGet("sla/{slaId:guid}")]
        [Consumes("application/json")]
        [SwaggerOperation(
            Summary = "Get shifts by SLA",
            Description = "Get all shifts belonging to a specific SLA.",
            Tags = new[] { "SlaShifts" })]
        public async Task<IActionResult> GetBySla(Guid slaId)
        {
            var result = await _service.GetBySlaIdAsync(slaId);

            return Ok(result);
        }

        [HttpPost]
        [Consumes("application/json")]
        [SwaggerOperation(
            Summary = "Create SLA Shift",
            Description = "Create a new shift for an SLA.",
            Tags = new[] { "SlaShifts" })]
        [SwaggerResponse(StatusCodes.Status200OK, "Create successfully")]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid request data")]
        public async Task<IActionResult> Create(SlaShiftCreateRequest request)
        {
            var result = await _service.CreateAsync(request);

            if (result != null)
                return Ok(result);

            return BadRequest();
        }

        [HttpPut("{id}")]
        [Consumes("application/json")]
        [SwaggerOperation(
            Summary = "Update SLA Shift",
            Description = "Update shift information.",
            Tags = new[] { "SlaShifts" })]
        [SwaggerResponse(StatusCodes.Status200OK, "Update successfully")]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Shift not found")]
        public async Task<IActionResult> Update(Guid id, SlaShiftUpdateRequest request)
        {
            var result = await _service.UpdateAsync(id, request);

            if (result != null)
                return Ok(result);

            return NotFound();
        }

        [HttpDelete("{id}")]
        [Consumes("application/json")]
        [SwaggerOperation(
            Summary = "Delete SLA Shift",
            Description = "Delete SLA Shift by id.",
            Tags = new[] { "SlaShifts" })]
        [SwaggerResponse(StatusCodes.Status200OK, "Delete successfully")]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Shift not found")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var result = await _service.DeleteAsync(id);

            if (result > 0)
                return Ok(result);

            return NotFound();
        }
    }
}
