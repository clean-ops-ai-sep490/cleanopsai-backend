using CleanOpsAi.Modules.Workforce.Application.Dtos.Equipments;
using CleanOpsAi.Modules.Workforce.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace CleanOpsAi.Api.Modules.Workforce
{
    [Route("api/[controller]")]
    [ApiController]
    public class EquipmentsController : ControllerBase
    {
        private readonly IEquipmentService _service;

        public EquipmentsController(IEquipmentService service)
        {
            _service = service;
        }

        [HttpGet("{id:guid}")]
        [Consumes("application/json")]
        [SwaggerOperation(
            Summary = "Get equipment by id",
            Description = "Get equipment information using equipmentId.",
            Tags = new[] { "Equipments" })]
        public async Task<IActionResult> GetById(Guid id)
        {
            var equipment = await _service.GetByIdAsync(id);

            if (equipment == null)
                return NotFound();

            return Ok(equipment);
        }

        [HttpGet]
        [Consumes("application/json")]
        [SwaggerOperation(
            Summary = "Get all equipments",
            Description = "Get all equipments with pagination.",
            Tags = new[] { "Equipments" })]
        public async Task<IActionResult> GetAll(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            var result = await _service.GetAllPaginationAsync(pageNumber, pageSize);

            return Ok(result);
        }

        [HttpPost]
        [Consumes("application/json")]
        [SwaggerOperation(
            Summary = "Create equipment",
            Description = "Create new equipment.",
            Tags = new[] { "Equipments" })]
        public async Task<IActionResult> Create(EquipmentCreateRequest request)
        {
            var result = await _service.CreateAsync(request);

            if (result != null)
                return Ok(result);

            return BadRequest();
        }

        [HttpPut("{id}")]
        [Consumes("application/json")]
        [SwaggerOperation(
            Summary = "Update equipment",
            Description = "Update equipment information.",
            Tags = new[] { "Equipments" })]
        public async Task<IActionResult> Update(Guid id, EquipmentUpdateRequest request)
        {
            var result = await _service.UpdateAsync(id, request);

            if (result != null)
                return Ok(result);

            return NotFound();
        }

        [HttpDelete("{id}")]
        [Consumes("application/json")]
        [SwaggerOperation(
            Summary = "Delete equipment",
            Description = "Delete equipment by id.",
            Tags = new[] { "Equipments" })]
        public async Task<IActionResult> Delete(Guid id)
        {
            var result = await _service.DeleteAsync(id);

            if (result > 0)
                return Ok(result);

            return NotFound();
        }
    }
}
