using CleanOpsAi.Modules.ClientManagement.Application.Dtos.SlaTasks;
using CleanOpsAi.Modules.ClientManagement.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace CleanOpsAi.Api.Modules.ClientManagement
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Manager")]
    public class SlaTasksController : ControllerBase
    {
        private readonly ISlaTaskService _service;

        public SlaTasksController(ISlaTaskService service)
        {
            _service = service;
        }

        [HttpGet("{id}")]
        [SwaggerOperation(Summary = "Get SLA Task by id", Tags = new[] { "SlaTasks" })]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _service.GetByIdAsync(id);

            if (result == null)
                return NotFound();

            return Ok(result);
        }

        [HttpGet]
        [SwaggerOperation(Summary = "Get all SLA Tasks", Tags = new[] { "SlaTasks" })]
        public async Task<IActionResult> GetAll(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            var result = await _service.GetAllPaginationAsync(pageNumber, pageSize);
            return Ok(result);
        }

        [HttpGet("sla/{slaId}")]
        [SwaggerOperation(Summary = "Get SLA Tasks by SLA id", Tags = new[] { "SlaTasks" })]
        public async Task<IActionResult> GetBySla(Guid slaId)
        {
            var result = await _service.GetBySlaIdAsync(slaId);
            return Ok(result);
        }

		[Authorize]
		[HttpPost]
        [SwaggerOperation(Summary = "Create SLA Task",
            Description = "recurrenceType have Daily, Weekly, Monthly. And recurrenceConfig have fix with the recurrenceType must read the docs for know json config", 
            Tags = new[] { "SlaTasks" })]
        public async Task<IActionResult> Create(SlaTaskCreateRequest request)
        {
            try
            {
                var result = await _service.CreateAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    message = ex.Message
                });
            }
        }

		[Authorize]
		[HttpPut("{id}")]
        [SwaggerOperation(Summary = "Update SLA Task",
            Description = "recurrenceType have Daily, Weekly, Monthly. And recurrenceConfig have fix with the recurrenceType must read the docs for know json config", 
            Tags = new[] { "SlaTasks" })]
        public async Task<IActionResult> Update(Guid id, SlaTaskUpdateRequest request)
        {
            try
            {
                var result = await _service.UpdateAsync(id, request);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new
                {
                    message = ex.Message
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    message = ex.Message
                });
            }
        }

		[Authorize]
		[HttpDelete("{id}")]
        [SwaggerOperation(Summary = "Delete SLA Task", Tags = new[] { "SlaTasks" })]
        public async Task<IActionResult> Delete(Guid id)
        {
            var result = await _service.DeleteAsync(id);

            if (result > 0)
                return Ok();

            return NotFound();
        }
    }
}
