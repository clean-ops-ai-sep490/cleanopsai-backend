using AutoMapper;
using CleanOpsAi.Modules.ServicePlanning.Application.Common.Interfaces.Repositories;
using CleanOpsAi.Modules.ServicePlanning.Application.Common.Interfaces.Services;
using CleanOpsAi.Modules.ServicePlanning.Domain.Entities;
using Medo;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace CleanOpsAi.Modules.ServicePlanning.Application.Services
{
	public class SopService : ISopService
	{
		private readonly ISopRepository _sopRepository;
		private readonly IStepRepository _stepRepository;
		private readonly IMapper _mapper;

		public SopService(ISopRepository sopRepository, IStepRepository stepRepository, IMapper mapper)
		{
			_sopRepository = sopRepository;
			_stepRepository = stepRepository;
			_mapper = mapper;
		}

		public async Task<SopDto> CreateSopAsync(SopCreateDto dto)
		{
			if (dto.Steps == null || dto.Steps.Count == 0)
				throw new ValidationException("SOP must have at least one step");

			var orders = dto.Steps.Select(s => s.StepOrder).ToList();
			if (orders.Distinct().Count() != orders.Count)
				throw new ValidationException("StepOrder must be unique");

			if (orders.Min() != 1 || orders.Max() != orders.Count)
				throw new ValidationException("StepOrder must be sequential starting from 1");

			var stepIds = dto.Steps.Select(x => x.StepId).Distinct().ToList();
			var steps = await _stepRepository.GetByIdsAsync(stepIds);

			if (steps.Count != stepIds.Count)
			{
				var missingIds = stepIds.Except(steps.Select(s => s.Id));
				throw new ValidationException($"Steps not found: {string.Join(", ", missingIds)}");
			}

			var stepDict = steps.ToDictionary(x => x.Id);

			foreach (var sopStep in dto.Steps)
			{
				var step = stepDict[sopStep.StepId];
				ValidateConfigDetail(sopStep.ConfigDetail, step.ConfigSchema, step.Name);
			}

			var now = DateTime.UtcNow;

			var sop = _mapper.Map<Sop>(dto);
			sop.Id = Uuid7.NewGuid();
			sop.Version = 1;
			sop.CreatedBy = "admin-123";
			sop.Created = now;

			sop.SopSteps = dto.Steps.Select(s => new SopStep
			{
				Id = Uuid7.NewGuid(),
				SopId = sop.Id,
				StepId = s.StepId,
				StepOrder = s.StepOrder,
				ConfigDetail = s.ConfigDetail.GetRawText(),
				CreatedBy = "admin-123",
				Created = now
			}).ToList();

			if (dto.RequiredSkillIds?.Any() == true)
			{
				sop.SopRequiredSkills = dto.RequiredSkillIds
					.Distinct()
					.Select(x => new SopRequiredSkill
					{
						SopId = sop.Id,
						SkillId = x
					}).ToList();
			}

			if (dto.RequiredCertificationIds?.Any() == true)
			{
				sop.SopRequiredCertifications = dto.RequiredCertificationIds
					.Distinct()
					.Select(x => new SopRequiredCertification
					{
						SopId = sop.Id,
						CertificationId = x
					}).ToList();
			}

			await _sopRepository.InsertAsync(sop);
			await _sopRepository.SaveChangesAsync();

			var created = await _sopRepository.GetByIdWithStepsAsync(sop.Id);
			return _mapper.Map<SopDto>(created);
		}

		public async Task<SopDto?> GetSopByIdAsync(Guid id)
		{
			var sop = await _sopRepository.GetByIdAsync(id);
			return _mapper.Map<SopDto>(sop);
		}

		public async Task<SopDto?> UpdateSopAsync(Guid id, SopUpdateDto dto)
		{
			var sop = await _sopRepository.GetByIdWithStepsAsync(id, true);
			if (sop == null) return null;

			if (dto.Steps != null)
			{
				if (dto.Steps.Count == 0)
					throw new ValidationException("SOP must have at least one step");

				var orders = dto.Steps.Select(s => s.StepOrder).ToList();

				if (orders.Distinct().Count() != orders.Count)
					throw new ValidationException("StepOrder must be unique");

				if (orders.Min() != 1 || orders.Max() != orders.Count)
					throw new ValidationException("StepOrder must be sequential starting from 1");

				var stepIds = dto.Steps.Select(x => x.StepId).Distinct().ToList();
				var steps = await _stepRepository.GetByIdsAsync(stepIds);

				if (steps.Count != stepIds.Count)
				{
					var missingIds = stepIds.Except(steps.Select(s => s.Id));
					throw new ValidationException($"Steps not found: {string.Join(", ", missingIds)}");
				}

				var stepDict = steps.ToDictionary(x => x.Id);

				foreach (var sopStep in dto.Steps)
				{
					var step = stepDict[sopStep.StepId];
					ValidateConfigDetail(sopStep.ConfigDetail, step.ConfigSchema, step.Name);
				}
			}

			_mapper.Map(dto, sop); // map field thường

			if (dto.Steps != null)
				MergeSopSteps(sop, dto.Steps);

			sop.Version++;
			sop.LastModified = DateTime.UtcNow;
			sop.LastModifiedBy = "admin-123";

			await _sopRepository.SaveChangesAsync();

			var updated = await _sopRepository.GetByIdWithStepsAsync(sop.Id);
			return _mapper.Map<SopDto>(updated);
		}

		public async Task<bool> DeleteSopAsync(Guid id)
		{
			var sop = await _sopRepository.GetByIdWithStepsAsync(id);
			if (sop == null) return false;

			var now = DateTime.UtcNow;

			sop.IsDeleted = true;
			sop.LastModified = now;
			sop.LastModifiedBy = "admin-123";

			// Cascade soft delete SopSteps
			foreach (var step in sop.SopSteps)
			{
				step.IsDeleted = true;
				step.LastModified = now;
				step.LastModifiedBy = "admin-123";
			}

			await _sopRepository.SaveChangesAsync();
			return true;
		}

		private void MergeSopSteps(Sop sop, List<SopStepUpdateDto> newSteps)
		{
			var now = DateTime.UtcNow;
			var allSteps = sop.SopSteps.ToList(); 
			var activeSteps = allSteps.Where(s => !s.IsDeleted).ToList();
			var newStepIds = newSteps.Select(s => s.StepId).ToHashSet();
			var activeStepIds = activeSteps.Select(s => s.StepId).ToHashSet();

			// 1. Soft delete step không còn trong danh sách mới
			foreach (var step in activeSteps.Where(s => !newStepIds.Contains(s.StepId)))
			{
				step.IsDeleted = true;
				step.LastModified = now;
				step.LastModifiedBy = "admin-123";
			}

			// 2. Update step còn tồn tại
			foreach (var existing in activeSteps.Where(s => newStepIds.Contains(s.StepId)))
			{
				var newStep = newSteps.First(s => s.StepId == existing.StepId);
				existing.StepOrder = newStep.StepOrder;
				existing.ConfigDetail = newStep.ConfigDetail.GetRawText();
				existing.LastModified = now;
				existing.LastModifiedBy = "admin-123";
			}
			 
			foreach (var newStep in newSteps.Where(s => !activeStepIds.Contains(s.StepId)))
			{
				var deletedStep = allSteps
					.FirstOrDefault(s => s.StepId == newStep.StepId && s.IsDeleted);

				if (deletedStep != null)
				{
					// Restore với configDetail mới
					deletedStep.IsDeleted = false;
					deletedStep.StepOrder = newStep.StepOrder;
					deletedStep.ConfigDetail = newStep.ConfigDetail.GetRawText();
					deletedStep.LastModified = now;
					deletedStep.LastModifiedBy = "admin-123";
				}
				else
				{
					sop.SopSteps.Add(new SopStep
					{ 
						SopId = sop.Id,
						StepId = newStep.StepId,
						StepOrder = newStep.StepOrder,
						ConfigDetail = newStep.ConfigDetail.GetRawText(),
						CreatedBy = "admin-123",
						Created = now
					});
				}
			}
		} 

		private void ValidateConfigDetail(JsonElement configDetail, string configSchema, string stepName)
		{
			try
			{
				var schema = JSchema.Parse(configSchema);
				var token = JToken.Parse(configDetail.GetRawText());
				var isValid = token.IsValid(schema, out IList<string> errors);

				if (!isValid)
					throw new ValidationException(
						$"ConfigDetail for step '{stepName}' is invalid:\n{string.Join("\n", errors)}"
					);
			}
			catch (JsonException ex)
			{
				throw new ValidationException($"ConfigDetail for step '{stepName}' is not valid JSON: {ex.Message}");
			}
		}

		public async Task<SopDto?> GetByIdWithStepsAsync(Guid id, CancellationToken cancellationToken = default)
		{
			var sop = await _sopRepository.GetByIdWithStepsAsync(id,cancellationToken: cancellationToken);
			return _mapper.Map<SopDto>(sop);
		}
	}
}
