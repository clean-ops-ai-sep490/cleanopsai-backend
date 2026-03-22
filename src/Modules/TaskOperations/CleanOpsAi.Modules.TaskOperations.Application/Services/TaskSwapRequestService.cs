using AutoMapper;
using CleanOpsAi.BuildingBlocks.Application.Pagination;
using CleanOpsAi.Modules.TaskOperations.Application.Common.Interfaces.Repositories;
using CleanOpsAi.Modules.TaskOperations.Application.Common.Interfaces.Services;
using CleanOpsAi.Modules.TaskOperations.Application.DTOs.Request;
using CleanOpsAi.Modules.TaskOperations.Application.DTOs.Response;
using CleanOpsAi.Modules.TaskOperations.Domain.Entities;
using MassTransit;
using Medo;
//using System.ComponentModel.DataAnnotations;


namespace CleanOpsAi.Modules.TaskOperations.Application.Services
{
	public class TaskSwapRequestService : ITaskSwapRequestService
	{
		private readonly ITaskSwapRequestRepository _taskSwapRequestRepository;
		private readonly ITaskAssignmentRepository _taskAssignmentRepository;
		private readonly IMapper _mapper;

		public TaskSwapRequestService(ITaskSwapRequestRepository taskSwapRequestRepository,
			ITaskAssignmentRepository taskAssignmentRepository,
			IMapper mapper)
		{
			_taskSwapRequestRepository = taskSwapRequestRepository;
			_taskAssignmentRepository = taskAssignmentRepository;
			_mapper = mapper;
		}

		public Task<ValidationResultExtensions.Result> CancelSwapRequestAsync(Guid swapRequestId, Guid requesterId, CancellationToken ct = default)
		{
			throw new NotImplementedException();
		}

		public Task<SwapRequestDto> CreateSwapRequestAsync(TaskSwapRequestCreateDto dto, Guid requesterId, CancellationToken ct = default)
		{
			throw new NotImplementedException();
		}

		public Task<PaginatedResult<SwapCandidateDto>> GetSwapCandidatesAsync(GetSwapCandidatesDto dto, PaginationRequest paginationRequest, CancellationToken ct = default)
		{
			throw new NotImplementedException();
		}

		public Task<ValidationResultExtensions.Result> RespondSwapRequestAsync(RespondSwapRequestDto dto, Guid responderId, CancellationToken ct = default)
		{
			throw new NotImplementedException();
		}

		public Task<ValidationResultExtensions.Result> ReviewSwapRequestAsync(ReviewSwapRequestDto dto, Guid reviewerId, CancellationToken ct = default)
		{
			throw new NotImplementedException();
		}
	}
}
