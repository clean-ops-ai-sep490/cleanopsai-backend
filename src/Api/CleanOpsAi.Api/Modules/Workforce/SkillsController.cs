using CleanOpsAi.Modules.Workforce.Application.Dtos.Skills;
using CleanOpsAi.Modules.Workforce.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace CleanOpsAi.Api.Modules.Workforce
{
    [Route("api/[controller]")]
    [ApiController]
    public class SkillsController : ControllerBase
    {
        private readonly ISkillService _service;

        public SkillsController(ISkillService service)
        {
            _service = service;
        }

        [HttpGet("{id:guid}")]
        [Consumes("application/json")]
        [SwaggerOperation(
            Summary = "Get skill by id",
            Description = "Get skill information using skillId.",
            Tags = new[] { "Skills" })]
        public async Task<IActionResult> GetById(Guid id)
        {
            var skill = await _service.GetByIdAsync(id);

            if (skill == null)
                return NotFound();

            return Ok(skill);
        }

        [HttpGet]
        [Consumes("application/json")]
        [SwaggerOperation(
            Summary = "Get all skills",
            Description = "Get all skills with pagination.",
            Tags = new[] { "Skills" })]
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
            Summary = "Create skill",
            Description = "Create new skill.",
            Tags = new[] { "Skills" })]
        public async Task<IActionResult> Create(SkillCreateRequest request)
        {
            var result = await _service.CreateAsync(request);

            if (result != null)
                return Ok(result);

            return BadRequest();
        }

        [HttpPut("{id}")]
        [Consumes("application/json")]
        [SwaggerOperation(
            Summary = "Update skill",
            Description = "Update skill information.",
            Tags = new[] { "Skills" })]
        public async Task<IActionResult> Update(Guid id, SkillUpdateRequest request)
        {
            var result = await _service.UpdateAsync(id, request);

            if (result != null)
                return Ok(result);

            return NotFound();
        }

        [HttpDelete("{id}")]
        [Consumes("application/json")]
        [SwaggerOperation(
            Summary = "Delete skill",
            Description = "Delete skill by id.",
            Tags = new[] { "Skills" })]
        public async Task<IActionResult> Delete(Guid id)
        {
            var result = await _service.DeleteAsync(id);

            if (result > 0)
                return Ok(result);

            return NotFound();
        }
    }
}
