using CleanOpsAi.Modules.Scoring.Application.Common.Interfaces.Services;
using CleanOpsAi.Modules.Scoring.Application.DTOs.Request;
using CleanOpsAi.Modules.Scoring.Application.DTOs.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace CleanOpsAi.Api.Modules.Scoring
{
	[Route("api/scoring/reviews")]
	[ApiController]
	[Authorize(Roles = "Supervisor,Admin")]
	public class ScoringReviewsController : ControllerBase
	{
		private readonly IScoringJobService _scoringJobService;

		public ScoringReviewsController(IScoringJobService scoringJobService)
		{
			_scoringJobService = scoringJobService;
		}

		[HttpGet("pending")]
		[SwaggerOperation(
			Summary = "List pending scoring results for supervisor review",
			Description = "Returns a capped list of scoring results whose AI verdict is PENDING.",
			Tags = new[] { "Scoring" }
		)]
		[ProducesResponseType(typeof(IReadOnlyCollection<PendingScoringReviewItemResponse>), StatusCodes.Status200OK)]
		public async Task<IActionResult> GetPending([FromQuery] int take = 100, CancellationToken ct = default)
		{
			var safeTake = Math.Clamp(take, 1, 500);
			var pendingItems = await _scoringJobService.GetPendingResultsAsync(safeTake, ct);
			return Ok(pendingItems);
		}

		[HttpPut("results/{resultId:guid}")]
		[SwaggerOperation(
			Summary = "Review a pending scoring result",
			Description = "Updates a PENDING verdict to PASS or FAIL and stores review metadata in payload history.",
			Tags = new[] { "Scoring" }
		)]
		[ProducesResponseType(typeof(ScoringResultReviewResponse), StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		[ProducesResponseType(StatusCodes.Status409Conflict)]
		public async Task<IActionResult> ReviewResult(Guid resultId, [FromBody] ReviewScoringResultRequest request, CancellationToken ct = default)
		{
			if (resultId == Guid.Empty)
			{
				return BadRequest("Result id cannot be empty.");
			}

			if (request is null)
			{
				return BadRequest("Request body cannot be null.");
			}

			try
			{
				var updated = await _scoringJobService.ReviewPendingResultAsync(resultId, request, ct);
				if (updated is null)
				{
					return NotFound($"Scoring result {resultId} was not found.");
				}

				return Ok(updated);
			}
			catch (ArgumentException ex)
			{
				return BadRequest(ex.Message);
			}
			catch (InvalidOperationException ex)
			{
				return Conflict(ex.Message);
			}
		}
	}
}
