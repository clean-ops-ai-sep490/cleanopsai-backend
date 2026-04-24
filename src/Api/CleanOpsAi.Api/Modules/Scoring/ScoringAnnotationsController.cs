using CleanOpsAi.Modules.Scoring.Application.Common.Interfaces.Services;
using CleanOpsAi.Modules.Scoring.Application.DTOs.Request;
using CleanOpsAi.Modules.Scoring.Application.DTOs.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace CleanOpsAi.Api.Modules.Scoring
{
	[Route("api/scoring/annotations/candidates")]
	[ApiController]
	[Authorize(Roles = "Manager,Admin")]
	public class ScoringAnnotationsController : ControllerBase
	{
		private readonly IScoringJobService _scoringJobService;

		public ScoringAnnotationsController(IScoringJobService scoringJobService)
		{
			_scoringJobService = scoringJobService;
		}

		[HttpGet]
		[SwaggerOperation(
			Summary = "List scoring annotation candidates",
			Description = "Returns manager/admin annotation queue items created from reviewed FAIL decisions.",
			Tags = new[] { "Scoring" }
		)]
		[ProducesResponseType(typeof(IReadOnlyCollection<ScoringAnnotationCandidateListItemResponse>), StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		public async Task<IActionResult> GetCandidates(
			[FromQuery] string? status = null,
			[FromQuery] string? environmentKey = null,
			[FromQuery] Guid? assignedTo = null,
			[FromQuery] DateTime? createdFrom = null,
			[FromQuery] int take = 50,
			CancellationToken ct = default)
		{
			try
			{
				var candidates = await _scoringJobService.GetAnnotationCandidatesAsync(
					status,
					environmentKey,
					assignedTo,
					createdFrom,
					Math.Clamp(take, 1, 500),
					ct);
				return Ok(candidates);
			}
			catch (ArgumentException ex)
			{
				return BadRequest(ex.Message);
			}
		}

		[HttpGet("{candidateId:guid}")]
		[SwaggerOperation(
			Summary = "Get scoring annotation candidate detail",
			Description = "Returns reviewed image metadata, raw payload, and current annotation draft for a candidate.",
			Tags = new[] { "Scoring" }
		)]
		[ProducesResponseType(typeof(ScoringAnnotationCandidateDetailResponse), StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		public async Task<IActionResult> GetById(Guid candidateId, CancellationToken ct = default)
		{
			var candidate = await _scoringJobService.GetAnnotationCandidateByIdAsync(candidateId, ct);
			if (candidate is null)
			{
				return NotFound($"Annotation candidate {candidateId} was not found.");
			}

			return Ok(candidate);
		}

		[HttpPost("{candidateId:guid}/claim")]
		[SwaggerOperation(
			Summary = "Claim scoring annotation candidate",
			Description = "Assigns an annotation candidate to the current manager/admin and moves it into progress.",
			Tags = new[] { "Scoring" }
		)]
		[ProducesResponseType(typeof(ScoringAnnotationCandidateDetailResponse), StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		[ProducesResponseType(StatusCodes.Status409Conflict)]
		public async Task<IActionResult> Claim(Guid candidateId, CancellationToken ct = default)
		{
			try
			{
				var candidate = await _scoringJobService.ClaimAnnotationCandidateAsync(candidateId, ct);
				if (candidate is null)
				{
					return NotFound($"Annotation candidate {candidateId} was not found.");
				}

				return Ok(candidate);
			}
			catch (InvalidOperationException ex)
			{
				return Conflict(ex.Message);
			}
		}

		[HttpPut("{candidateId:guid}/annotation")]
		[SwaggerOperation(
			Summary = "Save or submit scoring annotation",
			Description = "Stores annotation labels for a candidate as a draft or submitted package.",
			Tags = new[] { "Scoring" }
		)]
		[ProducesResponseType(typeof(ScoringAnnotationCandidateDetailResponse), StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		[ProducesResponseType(StatusCodes.Status409Conflict)]
		public async Task<IActionResult> UpsertAnnotation(
			Guid candidateId,
			[FromBody] UpsertScoringAnnotationRequest request,
			CancellationToken ct = default)
		{
			if (request is null)
			{
				return BadRequest("Request body cannot be null.");
			}

			try
			{
				var candidate = await _scoringJobService.UpsertAnnotationCandidateAsync(candidateId, request, ct);
				if (candidate is null)
				{
					return NotFound($"Annotation candidate {candidateId} was not found.");
				}

				return Ok(candidate);
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

		[HttpPost("{candidateId:guid}/approve")]
		[SwaggerOperation(
			Summary = "Approve scoring annotation candidate",
			Description = "Finalizes an annotation candidate and publishes approved annotation artifacts for retraining.",
			Tags = new[] { "Scoring" }
		)]
		[ProducesResponseType(typeof(ScoringAnnotationCandidateDetailResponse), StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		[ProducesResponseType(StatusCodes.Status409Conflict)]
		public async Task<IActionResult> Approve(
			Guid candidateId,
			[FromBody] ApproveScoringAnnotationCandidateRequest? request,
			CancellationToken ct = default)
		{
			try
			{
				var candidate = await _scoringJobService.ApproveAnnotationCandidateAsync(candidateId, request, ct);
				if (candidate is null)
				{
					return NotFound($"Annotation candidate {candidateId} was not found.");
				}

				return Ok(candidate);
			}
			catch (InvalidOperationException ex)
			{
				return Conflict(ex.Message);
			}
		}

		[HttpPost("{candidateId:guid}/reject")]
		[SwaggerOperation(
			Summary = "Reject scoring annotation candidate",
			Description = "Marks an annotation candidate as rejected while preserving its audit trail.",
			Tags = new[] { "Scoring" }
		)]
		[ProducesResponseType(typeof(ScoringAnnotationCandidateDetailResponse), StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		[ProducesResponseType(StatusCodes.Status409Conflict)]
		public async Task<IActionResult> Reject(
			Guid candidateId,
			[FromBody] RejectScoringAnnotationCandidateRequest? request,
			CancellationToken ct = default)
		{
			try
			{
				var candidate = await _scoringJobService.RejectAnnotationCandidateAsync(candidateId, request, ct);
				if (candidate is null)
				{
					return NotFound($"Annotation candidate {candidateId} was not found.");
				}

				return Ok(candidate);
			}
			catch (InvalidOperationException ex)
			{
				return Conflict(ex.Message);
			}
		}
	}
}
