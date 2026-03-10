using AutoMapper;
using CleanOpsAi.Modules.ServicePlanning.Application.Common.Interfaces.Repositories;
using CleanOpsAi.Modules.ServicePlanning.Application.Common.Interfaces.Services;
using CleanOpsAi.Modules.ServicePlanning.Domain.Entities;
using Medo;

namespace CleanOpsAi.Modules.ServicePlanning.Application.Services
{
	public class StepService : IStepService
	{
		private readonly IStepRepository _stepRepository;
		private readonly IMapper _mapper;

		public StepService(IStepRepository stepRepository, IMapper mapper)
		{
			_stepRepository = stepRepository;
			_mapper = mapper;
		}

		public async Task<StepDto> CreateNewStep(StepCreateDto dto)
		{
			var newStep = _mapper.Map<Step>(dto);

			newStep.Id = Uuid7.NewGuid();
			newStep.CreatedBy = "admin-123";
			newStep.Created = DateTime.UtcNow;
			newStep.ConfigSchema = dto.ConfigSchema.GetRawText();

			await _stepRepository.InsertAsync(newStep);
			await _stepRepository.SaveChangesAsync();

			return _mapper.Map<StepDto>(newStep);
		} 

		public async Task<StepDto> GetStepById(Guid id)
		{
			var step = await _stepRepository.GetByIdAsync(id); 
			return _mapper.Map<StepDto>(step);
		}

		public async Task<StepDto> UpdateStep(Guid id, StepUpdateDto dto)
		{
			var step = await _stepRepository.GetByIdAsync(id);
			if (step == null)
				return null!;

			_mapper.Map(dto, step);
			step.LastModifiedBy = "admin-123";
			step.LastModified = DateTime.UtcNow;

			await _stepRepository.SaveChangesAsync(); 
			return _mapper.Map<StepDto>(step); 
		}

		public async Task<bool> DeleteStep(Guid id)
		{
			var step = await _stepRepository.GetByIdAsync(id);
			if (step == null)
				return false;

			step.IsDeleted = true;
			return await _stepRepository.SaveChangesAsync() > 0;
		}
	}
}
