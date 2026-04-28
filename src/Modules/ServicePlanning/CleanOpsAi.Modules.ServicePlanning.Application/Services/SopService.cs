using AutoMapper;
using CleanOpsAi.BuildingBlocks.Application;
using CleanOpsAi.BuildingBlocks.Application.Exceptions;
using CleanOpsAi.BuildingBlocks.Application.Interfaces;
using CleanOpsAi.BuildingBlocks.Application.Pagination;
using CleanOpsAi.Modules.ServicePlanning.Application.Common.Interfaces.Repositories;
using CleanOpsAi.Modules.ServicePlanning.Application.Common.Interfaces.Services;
using CleanOpsAi.Modules.ServicePlanning.Domain.Entities; 
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using System.Text.Json;

namespace CleanOpsAi.Modules.ServicePlanning.Application.Services
{
	public class SopService : ISopService
	{
		private readonly ISopRepository _sopRepository;
		private readonly IStepRepository _stepRepository;
		private readonly ISopRequiredSkillRepository _sopRequiredSkillRepository;
		private readonly ISopRequiredCertificationRepository _sopRequiredCertificationRepository;
		private readonly IMapper _mapper;
		private readonly IDateTimeProvider _dateProvider;
		private readonly IIdGenerator _idGenerator;
		private readonly IUserContext _userContext;  

		public SopService(ISopRepository sopRepository, 
			IStepRepository stepRepository,
			ISopRequiredSkillRepository skillRequiredRepository,
			ISopRequiredCertificationRepository certificationRequiredRepository,
			IMapper mapper, IDateTimeProvider dateTimeProvider, IIdGenerator idGenerator,
			IUserContext userContext)
		{
			_sopRepository = sopRepository;
			_stepRepository = stepRepository;
			_sopRequiredSkillRepository = skillRequiredRepository;
			_sopRequiredCertificationRepository = certificationRequiredRepository;
			_mapper = mapper;
			_dateProvider = dateTimeProvider;
			_idGenerator = idGenerator;
			_userContext = userContext;
		}

		public async Task<SopDto> CreateSopAsync(SopCreateDto dto, CancellationToken ct = default)
		{
			if (dto.Steps == null || dto.Steps.Count == 0)
				throw new BadRequestException("SOP must have at least one step");

			var orders = dto.Steps.Select(s => s.StepOrder).ToList();
			if (orders.Distinct().Count() != orders.Count)
				throw new BadRequestException("StepOrder must be unique");

			if (orders.Min() != 1 || orders.Max() != orders.Count)
				throw new BadRequestException("StepOrder must be sequential starting from 1");

			var stepIds = dto.Steps.Select(x => x.StepId).Distinct().ToList();
			var steps = await _stepRepository.GetByIdsAsync(stepIds);

			if (steps.Count != stepIds.Count)
			{
				var missingIds = stepIds.Except(steps.Select(s => s.Id));
				throw new BadRequestException($"Steps not found: {string.Join(", ", missingIds)}");
			}

			var stepDict = steps.ToDictionary(x => x.Id);

			foreach (var sopStep in dto.Steps)
			{
				var step = stepDict[sopStep.StepId];
				ValidateConfigDetail(sopStep.ConfigDetail, step.ConfigSchema, step.Name);
			}

			var now = _dateProvider.UtcNow;

			var sop = _mapper.Map<Sop>(dto);
			sop.Id = _idGenerator.Generate();
			sop.Version = 1;
			sop.CreatedBy = _userContext.UserId.ToString();
			sop.Created = now;

			sop.SopSteps = dto.Steps.Select(s => new SopStep
			{
				Id = _idGenerator.Generate(),
				SopId = sop.Id,
				StepId = s.StepId,
				StepOrder = s.StepOrder,
				ConfigDetail = s.ConfigDetail.GetRawText(),
				CreatedBy = _userContext.UserId.ToString(),
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

			await _sopRepository.InsertAsync(sop, ct);
			await _sopRepository.SaveChangesAsync(ct);

			var created = await _sopRepository.GetByIdWithStepsAsync(sop.Id);
			return _mapper.Map<SopDto>(created);
		}

		public async Task<SopDto?> GetSopByIdAsync(Guid id, CancellationToken ct = default)
		{
			var sop = await _sopRepository.GetByIdAsync(id, ct);
			if(sop==null) throw new NotFoundException(nameof(Sop), id);
			return _mapper.Map<SopDto>(sop);
		}

		public async Task<SopDto?> UpdateSopAsync(Guid id, SopUpdateDto dto, CancellationToken cancellationToken = default)
		{
			var sop = await _sopRepository.GetByIdWithStepsAsync(id, true);
			if (sop == null) return null;

			if (dto.Steps != null)
			{
				if (dto.Steps.Count == 0)
					throw new BadRequestException("SOP must have at least one step");

				var orders = dto.Steps.Select(s => s.StepOrder).ToList();

				if (orders.Distinct().Count() != orders.Count)
					throw new BadRequestException("StepOrder must be unique");

				if (orders.Min() != 1 || orders.Max() != orders.Count)
					throw new BadRequestException("StepOrder must be sequential starting from 1");

				var stepIds = dto.Steps.Select(x => x.StepId).Distinct().ToList();
				var steps = await _stepRepository.GetByIdsAsync(stepIds);

				if (steps.Count != stepIds.Count)
				{
					var missingIds = stepIds.Except(steps.Select(s => s.Id));
					throw new BadRequestException($"Steps not found: {string.Join(", ", missingIds)}");
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
				MergeSopSteps(sop, dto.Steps, _userContext.UserId.ToString());

			sop.Version++;
			sop.LastModified = _dateProvider.UtcNow;
			sop.LastModifiedBy = _userContext.UserId.ToString();

			if(dto.RequiredSkillIds != null)
			{
				await _sopRequiredSkillRepository.MergeAsync(sop.Id, dto.RequiredSkillIds.ToHashSet());
			}

			if(dto.RequiredCertificationIds != null)
			{
				await _sopRequiredCertificationRepository.MergeAsync(sop.Id, dto.RequiredCertificationIds.ToHashSet(), cancellationToken);
			}

			await _sopRepository.SaveChangesAsync(cancellationToken);

			var updated = await _sopRepository.GetByIdWithStepsAsync(sop.Id);
			return _mapper.Map<SopDto>(updated);
		}

		public async Task<bool> DeleteSopAsync(Guid id, CancellationToken ct = default)
		{
			var sop = await _sopRepository.GetByIdWithStepsAsync(id);
			if (sop == null) return false;

			var now = DateTime.UtcNow;

			sop.IsDeleted = true;
			sop.LastModified = _dateProvider.UtcNow;
			sop.LastModifiedBy = _userContext.UserId.ToString();

			// Cascade soft delete SopSteps
			foreach (var step in sop.SopSteps)
			{
				step.IsDeleted = true;
				step.LastModified = _dateProvider.UtcNow;
				step.LastModifiedBy = _userContext.UserId.ToString();
			}

			await _sopRepository.SaveChangesAsync();
			return true;
		}

		private void MergeSopSteps(Sop sop, List<SopStepUpdateDto> newSteps, string userId)
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
				step.LastModified = _dateProvider.UtcNow;
				step.LastModifiedBy = userId.ToString();
			}

			// 2. Update step còn tồn tại
			foreach (var existing in activeSteps.Where(s => newStepIds.Contains(s.StepId)))
			{
				var newStep = newSteps.First(s => s.StepId == existing.StepId);
				existing.StepOrder = newStep.StepOrder;
				existing.ConfigDetail = newStep.ConfigDetail.GetRawText();
				existing.LastModified = _dateProvider.UtcNow;
				existing.LastModifiedBy = userId;
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
					deletedStep.LastModified = _dateProvider.UtcNow;
					deletedStep.LastModifiedBy = userId;
				}
				else
				{
					sop.SopSteps.Add(new SopStep
					{ 
						SopId = sop.Id,
						StepId = newStep.StepId,
						StepOrder = newStep.StepOrder,
						ConfigDetail = newStep.ConfigDetail.GetRawText(),
						CreatedBy = userId,
						Created = _dateProvider.UtcNow
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
					throw new BadRequestException(
						$"ConfigDetail for step '{stepName}' is invalid:\n{string.Join("\n", errors)}"
					);
			}
			catch (JsonException ex)
			{
				throw new BadRequestException($"ConfigDetail for step '{stepName}' is not valid JSON: {ex.Message}");
			}
		}

		public async Task<SopDto?> GetByIdWithStepsAsync(Guid id, CancellationToken cancellationToken = default)
		{
			var sop = await _sopRepository.GetSopWithStepDetail(id, ct: cancellationToken);
			return _mapper.Map<SopDto>(sop);
		}

		public async Task<PaginatedResult<SopListDto>> Gets(GetsSopQueryFilter query, PaginationRequest request, CancellationToken ct = default)
		{
			var result = await _sopRepository.GetsPaging(query, request, ct);

			return new PaginatedResult<SopListDto>(
				result.PageNumber,
				result.PageSize,
				result.TotalElements,
				_mapper.Map<List<SopListDto>>(result.Content));
		}

		public async Task<Sop?> GetSopWithDetail(Guid id, CancellationToken ct = default)
		{
			return await _sopRepository.GetSopWithDetail(id, ct);
		}
	}
}
