using CleanOpsAi.Api.Modules.Workforce.Dtos;
using CleanOpsAi.Modules.Workforce.Application.Dtos.PpeItems;
using CleanOpsAi.Modules.Workforce.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace CleanOpsAi.Api.Modules.Workforce
{
    [Route("api/[controller]")]
    [ApiController]
    public class PpeItemsController : ControllerBase
    {
        private readonly IPpeItemService _service;

        public PpeItemsController(IPpeItemService service)
        {
            _service = service;
        }

        [HttpGet("{id:guid}")]
        [Consumes("application/json")]
        [SwaggerOperation(
            Summary = "Get PPE item by id",
            Description = "Get PPE item information using id.",
            Tags = new[] { "PpeItems" })]
        public async Task<IActionResult> GetById(Guid id)
        {
            var item = await _service.GetByIdAsync(id);

            if (item == null)
                return NotFound();

            return Ok(item);
        }

        [HttpGet]
        [Consumes("application/json")]
        [SwaggerOperation(
            Summary = "Get all PPE items",
            Description = "Get all PPE items with pagination.",
            Tags = new[] { "PpeItems" })]
        public async Task<IActionResult> GetAll(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            var result = await _service.GetAllPaginationAsync(pageNumber, pageSize);

            return Ok(result);
        }

        [HttpPost]
        [Consumes("multipart/form-data")]
        [SwaggerOperation(
            Summary = "Create PPE item",
            Description = "Create new PPE item.",
            Tags = new[] { "PpeItems" })]
        [SwaggerResponse(StatusCodes.Status200OK, "Create successfully")]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid request data")]
        public async Task<IActionResult> Create([FromForm] PpeItemCreateFormRequest form)
        {
            var request = new PpeItemCreateRequest
            {
                ActionKey = form.ActionKey,
                Name = form.Name,
                Description = form.Description,
                ImageStream = form.ImageFile?.OpenReadStream(),
                ImageFileName = form.ImageFile?.FileName
            };

            var result = await _service.CreateAsync(request);

            return Ok(result);
        }

        [HttpPut("{id}")]
        [Consumes("multipart/form-data")]
        [SwaggerOperation(
            Summary = "Update PPE item",
            Description = "Update PPE item information.",
            Tags = new[] { "PpeItems" })]
        [SwaggerResponse(StatusCodes.Status200OK, "Update successfully")]
        [SwaggerResponse(StatusCodes.Status404NotFound, "PPE item not found")]
        public async Task<IActionResult> Update(Guid id, [FromForm] PpeItemUpdateFormRequest form)
        {
            var request = new PpeItemUpdateRequest
            {
                ActionKey = form.ActionKey,
                Name = form.Name,
                Description = form.Description,
                ImageStream = form.ImageFile?.OpenReadStream(),
                ImageFileName = form.ImageFile?.FileName
            };

            var result = await _service.UpdateAsync(id, request);

            return Ok(result);
        }

        [HttpDelete("{id}")]
        [Consumes("application/json")]
        [SwaggerOperation(
            Summary = "Delete PPE item",
            Description = "Delete PPE item by id.",
            Tags = new[] { "PpeItems" })]
        [SwaggerResponse(StatusCodes.Status200OK, "Delete successfully")]
        [SwaggerResponse(StatusCodes.Status404NotFound, "PPE item not found")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var result = await _service.DeleteAsync(id);

            if (result > 0)
                return Ok(result);

            return NotFound();
        }

        [HttpGet("action-keys")]
        [Consumes("application/json")]
        [SwaggerOperation(
            Summary = "Get all PPE action keys",
            Description = "List all distinct PPE action keys.",
            Tags = new[] { "PpeItems" })]
        public async Task<IActionResult> GetActionKeys()
        {
            var result = await _service.GetAllActionKeysAsync();
            return Ok(result);
        }

        [HttpGet("by-action-key")]
        [Consumes("application/json")]
        [SwaggerOperation(
            Summary = "Get PPE by action key",
            Description = "List all PPE items by action key.",
            Tags = new[] { "PpeItems" })]
        public async Task<IActionResult> GetByActionKey([FromQuery] string actionKey)
        {
            var result = await _service.GetByActionKeyAsync(actionKey);
            return Ok(result);
        }
    }
}
