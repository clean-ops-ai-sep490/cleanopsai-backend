using CleanOpsAi.Modules.Workforce.Application.Dtos.WorkerSkills;
using CleanOpsAi.Modules.Workforce.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace CleanOpsAi.Api.Modules.Workforce
{
    [Route("api/[controller]")]
    [ApiController]
    public class WorkerSkillsController : ControllerBase
    {
        private readonly IWorkerSkillService _service;

        public WorkerSkillsController(IWorkerSkillService service)
        {
            _service = service;
        }

        [HttpGet("{workerId:guid}/{skillId:guid}")]
        [Consumes("application/json")]
        [SwaggerOperation(
            Summary = "Get worker skill",
            Description = "Get worker skill using workerId and skillId.",
            Tags = new[] { "WorkerSkills" })]
        public async Task<IActionResult> GetById(Guid workerId, Guid skillId)
        {
            var result = await _service.GetByIdAsync(workerId, skillId);

            if (result == null)
                return NotFound();

            return Ok(result);
        }

        [HttpGet]
        [Consumes("application/json")]
        [SwaggerOperation(
            Summary = "Get all worker skills",
            Description = "Get all worker skills with pagination.",
            Tags = new[] { "WorkerSkills" })]
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
            Summary = "Assign skill to worker",
            Description = "Create worker skill.",
            Tags = new[] { "WorkerSkills" })]
        public async Task<IActionResult> Create(WorkerSkillCreateRequest request)
        {
            var result = await _service.CreateAsync(request);

            if (result != null)
                return Ok(result);

            return BadRequest();
        }

        [HttpPut("{workerId:guid}/{skillId:guid}")]
        [Consumes("application/json")]
        [SwaggerOperation(
            Summary = "Update worker skill",
            Description = "Update worker skill level.",
            Tags = new[] { "WorkerSkills" })]
        public async Task<IActionResult> Update(
            Guid workerId,
            Guid skillId,
            WorkerSkillUpdateRequest request)
        {
            var result = await _service.UpdateAsync(workerId, skillId, request);

            if (result != null)
                return Ok(result);

            return NotFound();
        }

        [HttpDelete("{workerId:guid}/{skillId:guid}")]
        [Consumes("application/json")]
        [SwaggerOperation(
            Summary = "Delete worker skill",
            Description = "Remove skill from worker.",
            Tags = new[] { "WorkerSkills" })]
        public async Task<IActionResult> Delete(Guid workerId, Guid skillId)
        {
            var result = await _service.DeleteAsync(workerId, skillId);

            if (result > 0)
                return Ok(result);

            return NotFound();
        }
    }
}
