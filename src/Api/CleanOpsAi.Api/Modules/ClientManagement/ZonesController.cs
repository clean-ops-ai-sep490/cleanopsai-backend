using CleanOpsAi.Modules.ClientManagement.Application.Dtos.Zones;
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
    public class ZonesController : ControllerBase
    {
        private readonly IZoneService _zoneService;

        public ZonesController(IZoneService zoneService)
        {
            _zoneService = zoneService;
        }

        [HttpGet("{id:guid}")]
        [Consumes("application/json")]
        [SwaggerOperation(
            Summary = "Get zone by id",
            Description = "Get a zone using zoneId.",
            Tags = new[] { "Zones" })]
        public async Task<IActionResult> GetById(Guid id)
        {
            var zone = await _zoneService.GetByIdAsync(id);

            if (zone == null)
                return NotFound();

            return Ok(zone);
        }

        [HttpGet]
        [Consumes("application/json")]
        [SwaggerOperation(
            Summary = "Get all zones",
            Description = "Get all zones with pagination.",
            Tags = new[] { "Zones" })]
        public async Task<IActionResult> GetAll(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            var zones = await _zoneService.GetAllPaginationAsync(pageNumber, pageSize);

            return Ok(zones);
        }

        [HttpGet("location/{locationId:guid}")]
        [Consumes("application/json")]
        [SwaggerOperation(
            Summary = "Get zones by locationId",
            Description = "Get all zones belonging to a location with pagination.",
            Tags = new[] { "Zones" })]
        public async Task<IActionResult> GetByLocationId(
            Guid locationId,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            var zones = await _zoneService
                .GetByLocationIdPaginationAsync(locationId, pageNumber, pageSize);

            return Ok(zones);
        }

        [HttpPost]
        [Consumes("application/json")]
        [SwaggerOperation(
            Summary = "Create zone",
            Description = "Create a new zone.",
            Tags = new[] { "Zones" })]
        [SwaggerResponse(StatusCodes.Status200OK, "Create successfully")]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid request data")]
        public async Task<IActionResult> Create(ZoneCreateRequest request)
        {
            var result = await _zoneService.CreateAsync(request);

            if (result != null)
                return Ok(result);

            return BadRequest();
        }

        [HttpPut("{id}")]
        [Consumes("application/json")]
        [SwaggerOperation(
            Summary = "Update zone",
            Description = "Update zone name or description.",
            Tags = new[] { "Zones" })]
        [SwaggerResponse(StatusCodes.Status200OK, "Update successfully")]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Zone not found")]
        public async Task<IActionResult> Update(Guid id, ZoneUpdateRequest request)
        {
            var result = await _zoneService.UpdateAsync(id, request);

            if (result != null)
                return Ok(result);

            return NotFound();
        }

        [HttpDelete("{id}")]
        [Consumes("application/json")]
        [SwaggerOperation(
            Summary = "Delete zone",
            Description = "Soft delete a zone by id.",
            Tags = new[] { "Zones" })]
        [SwaggerResponse(StatusCodes.Status200OK, "Delete successfully")]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Zone not found")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var result = await _zoneService.DeleteAsync(id);

            if (result > 0)
                return Ok(result);

            return NotFound();
        }

    }
}
