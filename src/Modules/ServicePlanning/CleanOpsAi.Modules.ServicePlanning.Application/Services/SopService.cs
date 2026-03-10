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
	}
}
