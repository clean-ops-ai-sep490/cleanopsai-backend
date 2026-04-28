using AutoMapper;
using CleanOpsAi.BuildingBlocks.Application;
using CleanOpsAi.BuildingBlocks.Application.Exceptions;
using CleanOpsAi.BuildingBlocks.Application.Interfaces;
using CleanOpsAi.BuildingBlocks.Application.Pagination;
using CleanOpsAi.Modules.ServicePlanning.Application.Common.Interfaces.Repositories;
using CleanOpsAi.Modules.ServicePlanning.Application.Common.Interfaces.Services;
using CleanOpsAi.Modules.ServicePlanning.Domain.Entities;
using Medo; 

namespace CleanOpsAi.Modules.ServicePlanning.Application.Services
{
	public class EnvironmentTypeService : IEnvironmentTypeService
	{
		private readonly IEnvironmentTypeRepository _environmentTypeRepository;
		private readonly IMapper _mapper;
		private readonly IUserContext _userContext;
		private readonly IDateTimeProvider _dateTimeProvider;
		private readonly IIdGenerator _idGenerator;

		public EnvironmentTypeService(
			IEnvironmentTypeRepository environmentTypeRepository,
			IMapper mapper,
			IUserContext userContext,
			IDateTimeProvider dateTimeProvider,
			IIdGenerator idGenerator)
		{
			_environmentTypeRepository = environmentTypeRepository;
			_mapper = mapper;
			_userContext = userContext;
			_dateTimeProvider = dateTimeProvider;
			_idGenerator = idGenerator;
		}

		public async Task<EnvironmentTypeDto> Create(EnvironmentTypeCreateDto dto, CancellationToken ct = default)
		{
		    var entity = _mapper.Map<EnvironmentType>(dto);
			entity.Id = _idGenerator.Generate();
			entity.Created = _dateTimeProvider.UtcNow;
			entity.CreatedBy = _userContext.UserId.ToString();

			await _environmentTypeRepository.InsertAsync(entity);
			await _environmentTypeRepository.SaveChangesAsync();

			return _mapper.Map<EnvironmentTypeDto>(entity);
		}

		public async Task<bool> Delete(Guid id, CancellationToken ct = default)
		{
			var environmentType = await _environmentTypeRepository.GetByIdAsync(id, ct);
			if (environmentType == null)
				throw new NotFoundException(nameof(EnvironmentType), id);


			environmentType.IsDeleted = true;
			await _environmentTypeRepository.SaveChangesAsync(ct);
			return true;
		}

		public async Task<EnvironmentTypeDto> GetById(Guid id, CancellationToken ct = default)
		{
			var environmentType =  await _environmentTypeRepository.GetByIdAsync(id, ct);

			if (environmentType == null)
				throw new NotFoundException(nameof(EnvironmentType), id);

			return _mapper.Map<EnvironmentTypeDto>(environmentType);
		}

		public async Task<PaginatedResult<EnvironmentTypeDto>> Gets(PaginationRequest request, CancellationToken ct = default)
		{
			var result = await _environmentTypeRepository.GetsPaging(request, ct);

			return new PaginatedResult<EnvironmentTypeDto>(
				result.PageNumber,
				result.PageSize,
				result.TotalElements,
				_mapper.Map<List<EnvironmentTypeDto>>(result.Content));
		}

		public async Task<EnvironmentTypeDto?> Update(Guid id,EnvironmentTypeUpdateDto dto, CancellationToken ct = default)
		{
			var environmentType = await _environmentTypeRepository.GetByIdAsync(id, ct);
			if (environmentType == null)
				throw new NotFoundException(nameof(EnvironmentType), id);


			_mapper.Map(dto, environmentType);
			environmentType.LastModified = _dateTimeProvider.UtcNow;
			environmentType.LastModifiedBy = _userContext.UserId.ToString();

			await _environmentTypeRepository.SaveChangesAsync(ct);
			return _mapper.Map<EnvironmentTypeDto>(environmentType);
		}
	}
}
