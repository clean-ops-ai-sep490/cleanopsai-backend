using AutoMapper;
using CleanOpsAi.BuildingBlocks.Application.Interfaces;
using CleanOpsAi.BuildingBlocks.Application.Pagination;
using CleanOpsAi.Modules.ServicePlanning.Application.Common.Interfaces.Repositories;
using CleanOpsAi.Modules.ServicePlanning.Application.Common.Interfaces.Services;
using CleanOpsAi.Modules.ServicePlanning.Domain.Entities; 

namespace CleanOpsAi.Modules.ServicePlanning.Application.Services
{
	public class StepService : IStepService
	{
		private readonly IStepRepository _stepRepository;
		private readonly IMapper _mapper;
		private readonly IDateTimeProvider _dateTimeProvider;
		private readonly IIdGenerator _idGenerator;


		public StepService(IStepRepository stepRepository, IMapper mapper, IDateTimeProvider dateTimeProvider, IIdGenerator idGenerator)
		{
			_stepRepository = stepRepository;
			_mapper = mapper;
			_dateTimeProvider = dateTimeProvider;
			_idGenerator = idGenerator;
		}

		public async Task<StepDto> CreateNewStep(StepCreateDto dto, Guid userId, CancellationToken ct = default)
		{
			var newStep = _mapper.Map<Step>(dto);

			newStep.Id = _idGenerator.Generate();
			newStep.CreatedBy = userId.ToString();
			newStep.Created = _dateTimeProvider.UtcNow;
			newStep.ConfigSchema = dto.ConfigSchema.GetRawText();

			await _stepRepository.InsertAsync(newStep, ct);
			await _stepRepository.SaveChangesAsync(ct);

			return _mapper.Map<StepDto>(newStep);
		} 

		public async Task<StepDto> GetStepById(Guid id, CancellationToken ct = default)
		{
			var step = await _stepRepository.GetByIdAsync(id, ct); 
			return _mapper.Map<StepDto>(step);
		}

		public async Task<StepDto> UpdateStep(Guid id, StepUpdateDto dto, Guid userId, CancellationToken ct = default)
		{
			var step = await _stepRepository.GetByIdAsync(id);
			if (step == null)
				return null!;

			_mapper.Map(dto, step);
			step.LastModifiedBy = userId.ToString();
			step.LastModified = _dateTimeProvider.UtcNow;

			await _stepRepository.SaveChangesAsync(ct); 
			return _mapper.Map<StepDto>(step); 
		}

		public async Task<bool> DeleteStep(Guid id, CancellationToken ct = default)
		{
			var step = await _stepRepository.GetByIdAsync(id);
			if (step == null)
				return false;

			step.IsDeleted = true;
			return await _stepRepository.SaveChangesAsync(ct) > 0;
		}

		public async Task<PaginatedResult<StepDto>> Gets(PaginationRequest request, CancellationToken ct = default)
		{
			var result = await _stepRepository.GetsPaging(request, ct);

			return new PaginatedResult<StepDto>(
				result.PageNumber,
				result.PageSize,
				result.TotalElements,
				_mapper.Map<List<StepDto>>(result.Content)); 
		}
	}
}
