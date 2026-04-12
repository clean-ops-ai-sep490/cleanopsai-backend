using CleanOpsAi.Modules.Scoring.Application.Common.Interfaces.Services;
using CleanOpsAi.Modules.Scoring.Application.DTOs.Request;
using CleanOpsAi.Modules.Scoring.Application.DTOs.Response;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace CleanOpsAi.Api.Modules.Scoring
{
	[Route("api/scoring/visualizations")]
	[ApiController]
	public class ScoringVisualizationsController : ControllerBase
	{
		private readonly IScoringJobService _scoringJobService;
		private readonly IScoringInferenceClient _scoringInferenceClient;

		public ScoringVisualizationsController(IScoringJobService scoringJobService, IScoringInferenceClient scoringInferenceClient)
		{
			_scoringJobService = scoringJobService;
			_scoringInferenceClient = scoringInferenceClient;
		}

		[HttpPost("jobs")]
		[SwaggerOperation(
			Summary = "Submit visualization scoring job",
			Description = "Submits an async scoring job that also enriches results with temporary visualization links when available.",
			Tags = new[] { "Scoring" }
		)]
		[ProducesResponseType(typeof(SubmitScoringJobResponse), StatusCodes.Status202Accepted)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		public async Task<IActionResult> SubmitVisualizationJob([FromBody] CreateScoringJobRequest request, CancellationToken ct = default)
		{
			if (request is null)
			{
				return BadRequest("Request body cannot be null.");
			}

			request.IncludeVisualizations = true;
			var result = await _scoringJobService.SubmitAsync(request, ct);
			return AcceptedAtAction(nameof(GetVisualizationJobById), new { jobId = result.JobId }, result);
		}

		[HttpGet("jobs/{jobId:guid}")]
		[SwaggerOperation(
			Summary = "Get visualization scoring job status",
			Description = "Gets async scoring status and enriched payload (including visualization metadata when generated).",
			Tags = new[] { "Scoring" }
		)]
		[ProducesResponseType(typeof(ScoringJobDetailResponse), StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		public async Task<IActionResult> GetVisualizationJobById(Guid jobId, CancellationToken ct = default)
		{
			if (jobId == Guid.Empty)
			{
				return BadRequest("Job id cannot be empty.");
			}

			var result = await _scoringJobService.GetByIdAsync(jobId, ct);
			if (result is null)
			{
				return NotFound($"Scoring job {jobId} was not found.");
			}

			return Ok(result);
		}

		[HttpGet("images/{token}")]
		[SwaggerOperation(
			Summary = "Get visualization image by token",
			Description = "Fetches a temporary visualization image using the token from scoring payload. This proxies the AI scoring service response through backend.",
			Tags = new[] { "Scoring" }
		)]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		public async Task<IActionResult> GetVisualizationImage(string token, CancellationToken ct = default)
		{
			if (string.IsNullOrWhiteSpace(token))
			{
				return BadRequest("Visualization token cannot be empty.");
			}

			try
			{
				var (content, contentType) = await _scoringInferenceClient.GetVisualizationImageAsync(token, ct);
				return File(content, contentType);
			}
			catch (InvalidOperationException ex) when (ex.Message.Contains("404", StringComparison.OrdinalIgnoreCase))
			{
				return NotFound("Visualization token not found or expired.");
			}
			catch (Exception ex)
			{
				return StatusCode(StatusCodes.Status502BadGateway, $"Failed to fetch visualization image: {ex.Message}");
			}
		}
	}
}
