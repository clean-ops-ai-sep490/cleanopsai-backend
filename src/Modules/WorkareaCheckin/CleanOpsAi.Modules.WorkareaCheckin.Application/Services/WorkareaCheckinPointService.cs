using AutoMapper;
using CleanOpsAi.BuildingBlocks.Application;
using CleanOpsAi.BuildingBlocks.Application.Common;
using CleanOpsAi.BuildingBlocks.Application.Exceptions;
using CleanOpsAi.BuildingBlocks.Application.Interfaces;
using CleanOpsAi.Modules.WorkareaCheckin.Application.Common.Interfaces.Repositories;
using CleanOpsAi.Modules.WorkareaCheckin.Application.Common.Interfaces.Services;
using CleanOpsAi.Modules.WorkareaCheckin.Application.DTOs.Request;
using CleanOpsAi.Modules.WorkareaCheckin.Application.DTOs.Response;
using CleanOpsAi.Modules.WorkareaCheckin.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CleanOpsAi.Modules.WorkareaCheckin.Application.Services
{
	public class WorkareaCheckinPointService : IWorkareaCheckinPointService
	{
		private readonly IWorkareaCheckinPointRepository _workareaCheckinPointRepository;
		private readonly IMapper _mapper;
		private readonly IIdGenerator _idGenerator;
		private readonly IDateTimeProvider _dateTimeProvider;
		private readonly IUserContext _userContext;

		public WorkareaCheckinPointService(
			IWorkareaCheckinPointRepository workareaCheckinPointRepository,
			IMapper mapper,
			IIdGenerator idGenerator,
			IDateTimeProvider dateTimeProvider,
			IUserContext userContext)
		{
			_workareaCheckinPointRepository = workareaCheckinPointRepository;
			_mapper = mapper;
			_idGenerator = idGenerator;
			_dateTimeProvider = dateTimeProvider;
			_userContext = userContext; 
		}
		 
		public async Task<WorkareaCheckinPointDto> GetByIdAsync(Guid id)
		{
			var workareaCheckinPoint = await _workareaCheckinPointRepository.GetByIdAsync(id);
			if(workareaCheckinPoint == null)
				throw new NotFoundException(nameof(WorkareaCheckinPoint), id);

			return _mapper.Map<WorkareaCheckinPointDto>(workareaCheckinPoint);
		}

		public async Task<WorkareaCheckinPointDto> Create(
			WorkareaCheckinPointCreateDto request,
			CancellationToken ct = default)
		{
			if (string.IsNullOrWhiteSpace(request.Name))
				throw new BadRequestException("Name is required");

			string code;

			if (string.IsNullOrWhiteSpace(request.Code))
			{
				var count = await _workareaCheckinPointRepository
					.CountByWorkarea(request.WorkareaId, ct);

				code = $"WCP-{request.WorkareaId.ToString()[..4].ToUpper()}-{(count + 1):D3}";
			}
			else
			{
				code = request.Code.Trim().ToUpper();

				var isExist = await _workareaCheckinPointRepository
					.ExistsByWorkareaAndCode(request.WorkareaId, code, ct);

				if (isExist)
					throw new BadRequestException("Code already exists in this workarea");
			}

			var entity = _mapper.Map<WorkareaCheckinPoint>(request);
			entity.Id = _idGenerator.Generate();
			entity.Code = code;

			entity.Created = _dateTimeProvider.UtcNow;
			entity.CreatedBy = _userContext.UserId.ToString();

			try
			{
				await _workareaCheckinPointRepository.InsertAsync(entity, ct);
				await _workareaCheckinPointRepository.SaveChangesAsync(ct);
			}
			catch (DbUpdateException)
			{
				throw new BadRequestException("Duplicate code, please retry");
			}

			return _mapper.Map<WorkareaCheckinPointDto>(entity);
		}

		public async Task<bool> Delete(Guid id, CancellationToken ct = default)
		{
			var entity = await _workareaCheckinPointRepository.GetByIdAsync(id);
			if (entity == null)
				throw new NotFoundException(nameof(WorkareaCheckinPoint), id);

			entity.IsActive = false;
			entity.IsDeleted = true;
			entity.LastModified = _dateTimeProvider.UtcNow;
			entity.LastModifiedBy = _userContext.UserId.ToString(); 

			await _workareaCheckinPointRepository.SaveChangesAsync(ct);

			return true;
		}

		public async Task<WorkareaCheckinPointDto> Update(
			Guid id,
			WorkareaCheckinPointUpdateDto request,
			CancellationToken ct = default)
		{
			var entity = await _workareaCheckinPointRepository.GetByIdAsync(id);
			if (entity == null)
				throw new NotFoundException(nameof(WorkareaCheckinPoint), id);

			if (!string.IsNullOrWhiteSpace(request.Name))
				entity.Name = request.Name.Trim();
			 
			if (!string.IsNullOrWhiteSpace(request.Code))
			{
				var newCode = request.Code.Trim().ToUpper();

				if (newCode != entity.Code)
				{
					var isExist = await _workareaCheckinPointRepository
						.ExistsByWorkareaAndCode(entity.WorkareaId, newCode, ct);

					if (isExist)
						throw new BadRequestException("Code already exists in this workarea");

					entity.Code = newCode;
				}
			}

			entity.LastModified = _dateTimeProvider.UtcNow;
			entity.LastModifiedBy = _userContext.UserId.ToString(); 

			await _workareaCheckinPointRepository.SaveChangesAsync(ct);

			return _mapper.Map<WorkareaCheckinPointDto>(entity);
		}

		public async Task<Result> Activate(Guid id, CancellationToken ct = default)
		{
			var entity = await _workareaCheckinPointRepository.GetByIdAsync(id);
			if (entity == null)
				throw new NotFoundException(nameof(WorkareaCheckinPoint), id);

			if (entity.IsActive)
				return Result.Success();

			entity.IsActive = true;
			entity.LastModified = _dateTimeProvider.UtcNow;
			entity.LastModifiedBy = _userContext.UserId.ToString();
			 
			await _workareaCheckinPointRepository.SaveChangesAsync(ct);

			return Result.Success();
		}

		public async Task<Result> Deactivate(Guid id, CancellationToken ct = default)
		{
			var entity = await _workareaCheckinPointRepository.GetByIdAsync(id);
			if (entity == null)
				throw new NotFoundException(nameof(WorkareaCheckinPoint), id);

			if (!entity.IsActive)
				return Result.Success();

			entity.IsActive = false;
			entity.LastModified = _dateTimeProvider.UtcNow;
			entity.LastModifiedBy = _userContext.UserId.ToString();

			await _workareaCheckinPointRepository.SaveChangesAsync(ct);

			return Result.Success();
		}

		public async Task<WorkareaCheckinPointDto?> GetFirstByWorkarea(Guid workareaId, CancellationToken ct)
		{
			var entity = await _workareaCheckinPointRepository.GetFirstByWorkarea(workareaId, ct);
			if (entity == null)
				throw new NotFoundException("", $"No check-in point found for workarea {workareaId}");

			return _mapper.Map<WorkareaCheckinPointDto>(entity);
		}
	}
}
