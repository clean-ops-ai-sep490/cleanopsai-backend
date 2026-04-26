using CleanOpsAi.Modules.Scoring.Application.Common.Interfaces.Services;
using CleanOpsAi.Modules.Scoring.Application.DTOs.Request;
using CleanOpsAi.Modules.Scoring.Application.DTOs.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace CleanOpsAi.Api.Modules.Scoring
{
	[Route("api/scoring/jobs")]
	[ApiController]
	public class ScoringJobsController : ControllerBase
	{
		private readonly IScoringJobService _scoringJobService;

		public ScoringJobsController(IScoringJobService scoringJobService)
		{
			_scoringJobService = scoringJobService;
		}

		[HttpPost]
		[Authorize(Roles = "Worker")]
		[SwaggerOperation(
			Summary = "Submit scoring job",
			Description = "Submits a clean/dirty scoring job and returns a job id for async polling.",
			Tags = new[] { "Scoring" }
		)]
		[ProducesResponseType(typeof(SubmitScoringJobResponse), StatusCodes.Status202Accepted)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		public async Task<IActionResult> Submit([FromBody] CreateScoringJobRequest request, CancellationToken ct = default)
		{
			if (request is null)
			{
				return BadRequest("Request body cannot be null.");
			}

			var result = await _scoringJobService.SubmitAsync(request, ct);
			return AcceptedAtAction(nameof(GetById), new { jobId = result.JobId }, result);
		}

		[HttpGet("{jobId:guid}")]
		[SwaggerOperation(
			Summary = "Get scoring job status",
			Description = "Gets current status and scoring result (if completed) for a job id.",
			Tags = new[] { "Scoring" }
		)]
		[ProducesResponseType(typeof(ScoringJobDetailResponse), StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		public async Task<IActionResult> GetById(Guid jobId, CancellationToken ct = default)
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

		[HttpGet]
		[SwaggerOperation(
			Summary = "List scoring jobs",
			Description = "Lists recent scoring jobs, optionally filtered by status for operational polling.",
			Tags = new[] { "Scoring" }
		)]
		[ProducesResponseType(typeof(IReadOnlyCollection<ScoringJobListItemResponse>), StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		public async Task<IActionResult> GetJobs([FromQuery] string? status = null, [FromQuery] int take = 50, CancellationToken ct = default)
		{
			try
			{
				var jobs = await _scoringJobService.GetJobsAsync(status, take, ct);
				return Ok(jobs);
			}
			catch (ArgumentException ex)
			{
				return BadRequest(ex.Message);
			}
		}
	}
}
