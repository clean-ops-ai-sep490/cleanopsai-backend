using CleanOpsAi.Modules.ClientManagement.Application.Dtos;
using CleanOpsAi.Modules.ClientManagement.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CleanOpsAi.Api.Modules.Clients
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

        [HttpGet("clientId/{id}")]
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
        public async Task<IActionResult> GetAll(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            var clients = await _clientService.GetAllPaginationAsync(pageNumber, pageSize);
            return Ok(clients);
        }

        [HttpPost]
        public async Task<IActionResult> Create(ClientCreateRequest request)
        {
            var result = await _clientService.CreateAsync(request);
            if (result > 0)
            {
                return Ok(result);
            }
            return BadRequest();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, ClientUpdateRequest request)
        {
            var result = await _clientService.UpdateAsync(id, request);
            if (result > 0)
            {
                return Ok(result);
            }
            return NotFound();
        }

        [HttpDelete("{id}")]
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
