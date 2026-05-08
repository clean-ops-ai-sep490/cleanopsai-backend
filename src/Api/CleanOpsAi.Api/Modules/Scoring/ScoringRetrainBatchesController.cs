using CleanOpsAi.Modules.Scoring.Application.Common.Interfaces.Services;
using CleanOpsAi.Modules.Scoring.Application.DTOs.Request;
using CleanOpsAi.Modules.Scoring.Application.DTOs.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace CleanOpsAi.Api.Modules.Scoring
{
	[Route("api/scoring/retrain/batches")]
	[ApiController]
	public class ScoringRetrainBatchesController : ControllerBase
	{
		private readonly IScoringJobService _scoringJobService;

		public ScoringRetrainBatchesController(IScoringJobService scoringJobService)
		{
			_scoringJobService = scoringJobService;
		}

		[HttpGet]
		[Authorize(Roles = "Supervisor,Manager,Admin,4,3,2")]
		[SwaggerOperation(
			Summary = "List scoring retrain batches",
			Description = "Lists recent retrain batches, optionally filtered by status for operations and audit.",
			Tags = new[] { "Scoring" }
		)]
		[ProducesResponseType(typeof(IReadOnlyCollection<ScoringRetrainBatchListItemResponse>), StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		public async Task<IActionResult> GetBatches([FromQuery] string? status = null, [FromQuery] int take = 50, CancellationToken ct = default)
		{
			try
			{
				var batches = await _scoringJobService.GetRetrainBatchesAsync(status, take, ct);
				return Ok(batches);
			}
			catch (ArgumentException ex)
			{
				return BadRequest(ex.Message);
			}
		}

		[HttpGet("{batchId:guid}")]
		[Authorize(Roles = "Supervisor,Manager,Admin,4,3,2")]
		[SwaggerOperation(
			Summary = "Get scoring retrain batch",
			Description = "Returns retrain batch state and execution history tracked in PostgreSQL.",
			Tags = new[] { "Scoring" }
		)]
		[ProducesResponseType(typeof(ScoringRetrainBatchDetailResponse), StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		public async Task<IActionResult> GetById(Guid batchId, CancellationToken ct = default)
		{
			var batch = await _scoringJobService.GetRetrainBatchByIdAsync(batchId, ct);
			if (batch is null)
			{
				return NotFound($"Retrain batch {batchId} was not found.");
			}

			return Ok(batch);
		}

		[HttpPost("trigger")]
		[Authorize(Roles = "Supervisor,Manager,Admin,4,3,2")]
		[SwaggerOperation(
			Summary = "Trigger scoring retrain batch",
			Description = "Creates a retrain batch from reviewed scoring samples and publishes it to the async retrain pipeline.",
			Tags = new[] { "Scoring" }
		)]
		[ProducesResponseType(typeof(ScoringRetrainBatchDetailResponse), StatusCodes.Status202Accepted)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status409Conflict)]
		public async Task<IActionResult> Trigger([FromBody] TriggerScoringRetrainRequest? request, CancellationToken ct = default)
		{
			try
			{
				var batch = await _scoringJobService.TriggerRetrainAsync(request ?? new TriggerScoringRetrainRequest(), ct);
				return AcceptedAtAction(nameof(GetById), new { batchId = batch.BatchId }, batch);
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