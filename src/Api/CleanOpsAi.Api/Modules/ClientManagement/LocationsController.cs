using CleanOpsAi.Modules.ClientManagement.Application.Dtos.Locations;
using CleanOpsAi.Modules.ClientManagement.Application.Interfaces;
using CleanOpsAi.Modules.ClientManagement.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace CleanOpsAi.Api.Modules.ClientManagement
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Manager, Admin")]
    public class LocationsController : ControllerBase
    {
        private readonly ILocationService _locationService;

        public LocationsController(ILocationService locationService)
        {
            _locationService = locationService;
        }

        [HttpGet("{id:guid}")]
        [Consumes("application/json")]
        [SwaggerOperation(
            Summary = "Get location by locationId",
            Description = "Get a location using locationId.",
            Tags = new[] { "Locations" })]
        public async Task<IActionResult> GetById(Guid id)
        {
            var location = await _locationService.GetByIdAsync(id);

            if (location == null)
            {
                return NotFound();
            }

            return Ok(location);
        }

        [HttpGet]
        [Consumes("application/json")]
        [SwaggerOperation(
            Summary = "Get all locations",
            Description = "Get all locations with pagination",
            Tags = new[] { "Locations" })]
        public async Task<IActionResult> GetAll(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            var locations = await _locationService.GetAllPaginationAsync(pageNumber, pageSize);

            return Ok(locations);
        }

        [HttpPost]
        [Consumes("application/json")]
        [SwaggerOperation(
            Summary = "Create new location",
            Description = "Create a new location.",
            Tags = new[] { "Locations" })]
        [SwaggerResponse(StatusCodes.Status200OK, "Create successfully")]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid request data")]
        public async Task<IActionResult> Create(LocationCreateRequest request)
        {
            var result = await _locationService.CreateAsync(request);

            if (result != null)
            {
                return Ok(result);
            }

            return BadRequest();
        }

        [HttpPut("{id}")]
        [Consumes("application/json")]
        [SwaggerOperation(
            Summary = "Update location",
            Description = "Update a location using LocationId.",
            Tags = new[] { "Locations" })]
        [SwaggerResponse(StatusCodes.Status200OK, "Update successfully")]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid request data")]
        public async Task<IActionResult> Update(Guid id, LocationUpdateRequest request)
        {
            var result = await _locationService.UpdateAsync(id, request);

            if (result != null)
            {
                return Ok(result);
            }

            return NotFound();
        }

        [HttpDelete("{id}")]
        [Consumes("application/json")]
        [SwaggerOperation(
            Summary = "Delete location",
            Description = "Delete a location by LocationId (soft delete).",
            Tags = new[] { "Locations" })]
        [SwaggerResponse(StatusCodes.Status200OK, "Delete successfully")]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid request data")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var result = await _locationService.DeleteAsync(id);

            if (result > 0)
            {
                return Ok(result);
            }

            return NotFound();
        }

        //        [HttpGet("client/{clientId:guid}")]
        //        [SwaggerOperation(
        //Summary = "Get locations by clientId",
        //Description = "Get all locations belonging to a specific client",
        //Tags = new[] { "Locations" })]
        //        public async Task<IActionResult> GetByClientId(Guid clientId)
        //        {
        //            var locations = await _locationService.GetByClientIdAsync(clientId);

        //            if (locations == null || !locations.Any())
        //            {
        //                return NotFound();
        //            }

        //            return Ok(locations);
        //        }

        [HttpGet("client/{clientId:guid}")]
        [SwaggerOperation(
Summary = "Get locations by clientId",
Description = "Get locations of a client with pagination",
Tags = new[] { "Locations" })]
        public async Task<IActionResult> GetByClientId(
    Guid clientId,
    [FromQuery] int pageNumber = 1,
    [FromQuery] int pageSize = 10)
        {
            var result = await _locationService
                .GetByClientIdPaginationAsync(clientId, pageNumber, pageSize);

            return Ok(result);
        }
    }
}
