using CleanOpsAi.Modules.Workforce.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CleanOpsAi.Api.Modules.Workforce
{
    [Route("api/[controller]")]
    [ApiController]
    public class GeminiController : ControllerBase
    {
        private readonly IGeminiService _geminiService;

        public GeminiController(IGeminiService geminiService)
        {
            _geminiService = geminiService;
        }

        [HttpPost("chat")]
        public async Task<IActionResult> Chat([FromBody] string message, CancellationToken cancellationToken)
        {
            try
            {
                var result = await _geminiService.ChatAsync(message);
                return Ok(result);
            }
            catch (CleanOpsAi.BuildingBlocks.Application.Exceptions.BadRequestException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
