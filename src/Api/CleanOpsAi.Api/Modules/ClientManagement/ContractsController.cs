using CleanOpsAi.Api.Modules.ClientManagement.Dtos;
using CleanOpsAi.Modules.ClientManagement.Application.Dtos.Contracts;
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
    public class ContractsController : ControllerBase
    {
        private readonly IContractService _service;
        private readonly IContractScanService _scanService;

        public ContractsController(IContractService service, IContractScanService scanService)
        {
            _service = service;
            _scanService = scanService;
        }

        [HttpGet("{id:guid}")]
        [Consumes("application/json")]
        [SwaggerOperation(
            Summary = "Get contract by id",
            Description = "Get a contract using contractId.",
            Tags = new[] { "Contracts" })]
        public async Task<IActionResult> GetById(Guid id)
        {
            var contract = await _service.GetByIdAsync(id);

            if (contract == null)
                return NotFound();

            return Ok(contract);
        }

        [HttpGet]
        [Consumes("application/json")]
        [SwaggerOperation(
            Summary = "Get all contracts",
            Description = "Get all contracts with pagination.",
            Tags = new[] { "Contracts" })]
        public async Task<IActionResult> GetAll(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            var contracts = await _service.GetAllPaginationAsync(pageNumber, pageSize);

            return Ok(contracts);
        }

		[Authorize]
		[HttpPost]
        [Consumes("multipart/form-data")]
        [SwaggerOperation(
            Summary = "Create contract",
            Description = "Create a contract with file upload.",
            Tags = new[] { "Contracts" })]
        [SwaggerResponse(StatusCodes.Status200OK, "Create successfully")]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid request data")]
        public async Task<IActionResult> Create([FromForm] CreateContractApiRequest request)
        {
            var command = new ContractCreateRequest
            {
                Name = request.Name,
                ClientId = request.ClientId,
                ContractStartDate = request.ContractStartDate,
                ContractEndDate = request.ContractEndDate,
                FileStream = request.File?.OpenReadStream(),
                FileName = request.File?.FileName
            };

            var result = await _service.CreateAsync(command);

            if (result !=null)
                return Ok(result);

            return BadRequest();
        }

		[Authorize]
		[HttpPut("{id}")]
        [Consumes("multipart/form-data")]
        [SwaggerOperation(
            Summary = "Update contract",
            Description = "Update contract name or file.",
            Tags = new[] { "Contracts" })]
        [SwaggerResponse(StatusCodes.Status200OK, "Update successfully")]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Contract not found")]
        public async Task<IActionResult> Update(Guid id, [FromForm] UpdateContractApiRequest request)
        {
            var command = new ContractUpdateRequest
            {
                Name = request.Name,
                ContractStartDate = request.ContractStartDate,
                ContractEndDate = request.ContractEndDate,
                FileStream = request.File?.OpenReadStream(),
                FileName = request.File?.FileName
            };

            var result = await _service.UpdateAsync(id, command);

            if (result != null)
                return Ok(result);

            return NotFound();
        }


		[Authorize]
		[HttpDelete("{id}")]
        [Consumes("application/json")]
        [SwaggerOperation(
            Summary = "Delete contract",
            Description = "Delete contract by id.",
            Tags = new[] { "Contracts" })]
        [SwaggerResponse(StatusCodes.Status200OK, "Delete successfully")]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Contract not found")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var result = await _service.DeleteAsync(id);

            if (result > 0)
                return Ok(result);

            return NotFound();
        }

        //    [HttpGet("client/{clientId:guid}")]
        //    [Consumes("application/json")]
        //    [SwaggerOperation(
        //Summary = "Get contracts by clientId",
        //Description = "Get all contracts belonging to a client.",
        //Tags = new[] { "Contracts" })]
        //    public async Task<IActionResult> GetByClientId(Guid clientId)
        //    {
        //        var contracts = await _service.GetByClientIdAsync(clientId);

        //        if (contracts == null || !contracts.Any())
        //            return NotFound();

        //        return Ok(contracts);
        //    }

        [HttpGet("client/{clientId:guid}")]
        [SwaggerOperation(
Summary = "Get contracts by clientId",
Description = "Get contracts of a client with pagination",
Tags = new[] { "Contracts" })]
        public async Task<IActionResult> GetByClientId(
    Guid clientId,
    [FromQuery] int pageNumber = 1,
    [FromQuery] int pageSize = 10)
        {
            var result = await _service
                .GetByClientIdPaginationAsync(clientId, pageNumber, pageSize);

            return Ok(result);
        }

        //[HttpPost("{id:guid}/scan")]
        //[SwaggerOperation(
        //    Summary = "Scan contract using AI",
        //    Description = "Extracts SLA, Shifts, and Tasks from the uploaded contract document using AI.",
        //    Tags = new[] { "Contracts" })]
        //[SwaggerResponse(StatusCodes.Status200OK, "Scan completed successfully")]
        //[SwaggerResponse(StatusCodes.Status404NotFound, "Contract not found")]
        //[SwaggerResponse(StatusCodes.Status400BadRequest, "Failed to scan contract")]
        //public async Task<IActionResult> ScanContract(Guid id, CancellationToken cancellationToken)
        //{
        //    try
        //    {
        //        var result = await _scanService.ScanContractAsync(id, cancellationToken);
        //        return Ok(result);
        //    }
        //    catch (Exception ex)
        //    {
        //        // In a real app, this should be handled by a global exception filter, but we return a generic error for now
        //        return BadRequest(new { Error = ex.Message });
        //    }
        //}
    }
}