using CleanOpsAi.BuildingBlocks.Application.Pagination;
using CleanOpsAi.Modules.TaskOperations.Application.Common.Interfaces.Services;
using CleanOpsAi.Modules.TaskOperations.Application.DTOs.Request;
using CleanOpsAi.Modules.TaskOperations.Application.DTOs.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Security.Claims;

namespace CleanOpsAi.Api.Modules.TaskOperations
{
	[Route("api/[controller]")]
	[ApiController]
	public class TaskSwapRequestsController : ControllerBase
	{
		private readonly ITaskSwapRequestService _service; 

		public TaskSwapRequestsController(ITaskSwapRequestService taskSwapRequestService)
		{
			_service = taskSwapRequestService; 
		}

		[Authorize]
		[HttpGet("{taskAssignmentId}/swap-candidates")]
		[SwaggerOperation(
			Summary = "Get Swap Candidates",
			Description = "Finds eligible swap partners within the current week. Candidates must: 1. Be in the same work area. 2. Meet requirements for your task. 3. Have no schedule conflicts. Supports date/time filtering.",
			Tags = new[] { "TaskSwapRequest" }
		)]
		[ProducesResponseType(typeof(PaginatedResult<SwapCandidateDto>), StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		public async Task<IActionResult> GetSwapCandidates([FromRoute] Guid taskAssignmentId, [FromQuery] DateOnly? date, [FromQuery] TimeOnly? preferredStartTime, [FromQuery] PaginationRequest paginationRequest,
	       CancellationToken ct = default)
		{
			var dto = new GetSwapCandidatesDto
			{
				TaskAssignmentId = taskAssignmentId,
				Date = date,
				PreferredStartTime = preferredStartTime
			};
			var result = await _service.GetSwapCandidatesAsync(dto, paginationRequest, ct); 

			return Ok(result.Value);
		}


		//[Authorize]
		[HttpGet]
		[SwaggerOperation(
			Summary = "Get Task Swap Requests List",
			Description = "Retrieves a list of task swap requests with optional filtering, sorting, and pagination. " +
				  "Supports filtering by status, requester, target worker, and date range. " +
				  "The result can be scoped based on the current user's role (e.g., requester, target worker, or manager).",
			Tags = new[] { "TaskSwapRequest" }
		)]
		[ProducesResponseType(typeof(PaginatedResult<TaskSwapRequestDto>), StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		public async Task<IActionResult> Gets([FromQuery] GetSwapRequestsDto dto,[FromQuery] PaginationRequest paginationRequest, CancellationToken ct = default)
		{
			var result = await _service.GetSwapRequestsAsync(dto, paginationRequest, ct);
			return Ok(result);
		}

		//[Authorize]
		[HttpGet("{id:guid}")]
		[SwaggerOperation(
			Summary = "Get Task Swap Request by Id",
			Description = "Retrieves detailed information of a specific task swap request by its unique identifier.",
			Tags = new[] { "TaskSwapRequest" }
		)]
		[ProducesResponseType(typeof(TaskSwapRequestDto), StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		public async Task<IActionResult> GetById(Guid id, CancellationToken ct = default)
		{
			var swapDto= await _service.GetById(id, ct);
			return Ok(swapDto);
		}

		[Authorize]
		[HttpPost]
		[SwaggerOperation(
			Summary = "Create Task Swap Request",
			Description = "Initiates a swap. Validates: 1. Dual-competency (both must be qualified for each other's tasks). 2. Post-swap schedule conflicts. 3. Existing pending requests. Request expires in 2h.",
			Tags = new[] { "TaskSwapRequest" }
		)]
		[ProducesResponseType(typeof(TaskSwapRequestDto), StatusCodes.Status201Created)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		public async Task<IActionResult> Create([FromBody] TaskSwapRequestCreateDto dto, CancellationToken ct = default)
		{
			var result = await _service.CreateSwapRequestAsync(dto, ct); 

			return CreatedAtAction(
				nameof(GetById),
				new { id = result.Value!.Id },
				result.Value
			);
		}

		[Authorize]
		[HttpPut("respond")]
		[SwaggerOperation(
			Summary = "Respond to Task Swap Request",
			Description = "Allows the target worker to accept or reject the swap request. " +
				  "If accepted, the request will move to manager review stage. " +
				  "If rejected, the request will be closed.",
			Tags = new[] { "TaskSwapRequest" }
		)]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		public async Task<IActionResult> Respond([FromBody] RespondSwapRequestDto dto, CancellationToken ct)
		{ 
			var result = await _service.RespondSwapRequestAsync(dto, ct); 
			return Ok();
		}

		[Authorize]
		[HttpPut("review")]
		[SwaggerOperation(
			Summary = "Review Task Swap Request",
			Description = "Allows a manager to approve or reject a task swap request after it has been accepted by the target worker. " +
				  "Only approved requests will result in the actual task assignment swap.",
			Tags = new[] { "TaskSwapRequest" }
		)]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		public async Task<IActionResult> Review([FromBody] ReviewSwapRequestDto dto, CancellationToken ct)
		{
			var result = await _service.ReviewSwapRequestAsync(dto, ct); 

			return Ok();
		}

		[Authorize]
		[HttpDelete("{id:guid}/cancel")]
		[SwaggerOperation(
			Summary = "Cancel Task Swap Request",
			Description = "Allows the requester to cancel a pending task swap request. " +
				  "Requires workerId to identify the requester. " +
				  "Only requests that have not been finalized can be canceled.",
			Tags = new[] { "TaskSwapRequest" }
		)]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		public async Task<IActionResult> Cancel(Guid id, [FromBody] CancelSwapRequestDto dto, CancellationToken ct = default)
		{
			var result = await _service.CancelSwapRequestAsync(id, dto.RequesterId, ct); 

			return Ok();
		}
	}
}
