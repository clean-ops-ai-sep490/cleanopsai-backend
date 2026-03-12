using CleanOpsAi.Api.Modules.UserAccess.Dtos;
using CleanOpsAi.Modules.ClientManagement.Application.Dtos.Clients;
using CleanOpsAi.Modules.ClientManagement.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace CleanOpsAi.Api.Modules.ClientManagement
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClientsController : ControllerBase
    {
        private readonly IClientService _clientService;
        public ClientsController(IClientService clientService)
        {
            _clientService = clientService;
        }

        [HttpGet("{id:guid}")]
        [Consumes("application/json")]
        [SwaggerOperation(
    Summary = "Get client by clientId",
    Description = "Get a client using clientId.",
    Tags = new[] { "Clients" })]
        public async Task<IActionResult> GetById(Guid id)
        {
            var client = await _clientService.GetByIdAsync(id);
            if (client == null)
            {
                return NotFound();
            }
            return Ok(client);
        }

        [HttpGet]
        [Consumes("application/json")]
        [SwaggerOperation(
    Summary = "Get all clients",
    Description = "Get all clients and pagination",
    Tags = new[] { "Clients" })]
        public async Task<IActionResult> GetAll(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            var clients = await _clientService.GetAllPaginationAsync(pageNumber, pageSize);
            return Ok(clients);
        }

        [HttpPost]
        [Consumes("application/json")]
        [SwaggerOperation(
    Summary = "Create new user",
    Description = "Create a new client by name and email.",
    Tags = new[] { "Clients" })]
        [SwaggerResponse(StatusCodes.Status200OK, "Create successfully")]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid request data")]
        public async Task<IActionResult> Create(ClientCreateRequest request)
        {
            var result = await _clientService.CreateAsync(request);
            if (result != null)
            {
                return Ok(result);
            }
            return BadRequest();
        }

        [HttpPut("{id}")]
        [Consumes("application/json")]
        [SwaggerOperation(
    Summary = "Update user",
    Description = "Update a client throw ClientId and Name, Email (Name and Email can empty if not want to change info",
    Tags = new[] { "Clients" })]
        [SwaggerResponse(StatusCodes.Status200OK, "Update successfully")]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid request data")]
        public async Task<IActionResult> Update(Guid id, ClientUpdateRequest request)
        {
            var result = await _clientService.UpdateAsync(id, request);
            if (result != null)
            {
                return Ok(result);
            }
            return NotFound();
        }

        [HttpDelete("{id}")]
        [Consumes("application/json")]
        [SwaggerOperation(
    Summary = "Delete user",
    Description = "Delete a client throw ClientId (soft delete) ",
    Tags = new[] { "Clients" })]
        [SwaggerResponse(StatusCodes.Status200OK, "Delete successfully")]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid request data")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var result = await _clientService.DeleteAsync(id);
            if (result > 0)
            {
                return Ok(result);
            }
            return NotFound();
        }
    }
}
