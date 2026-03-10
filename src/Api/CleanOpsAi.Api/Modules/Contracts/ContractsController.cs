using CleanOpsAi.Api.Modules.Contracts.Dtos;
using CleanOpsAi.Modules.ClientManagement.Application.Dtos.Contracts;
using CleanOpsAi.Modules.ClientManagement.Application.Interfaces;
using CleanOpsAi.Modules.ClientManagement.Application.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CleanOpsAi.Api.Modules.Contracts
{
    [Route("api/[controller]")]
    [ApiController]
    public class ContractsController : ControllerBase
    {
        private readonly IContractService _service;

        public ContractsController(IContractService service)
        {
            _service = service;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromForm] CreateContractApiRequest request)
        {
            var command = new ContractCreateRequest
            {
                Name = request.Name,
                ClientId = request.ClientId,
                FileStream = request.File?.OpenReadStream(),
                FileName = request.File?.FileName
            };

            var result = await _service.CreateAsync(command);

            return Ok(result);
        }

        [HttpPut]
        public async Task<IActionResult> Update(Guid id, [FromForm] UpdateContractApiRequest request)
        {
            var command = new ContractUpdateRequest
            {
                Name = request.Name,
                FileStream = request.File?.OpenReadStream(),
                FileName = request.File?.FileName
            };

            var result = await _service.UpdateAsync(id, command);

            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var result = await _service.DeleteAsync(id);

            return Ok(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var data = await _service.GetAllAsync();

            return Ok(data);
        }
    }
}
