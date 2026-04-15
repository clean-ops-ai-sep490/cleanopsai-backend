using AutoMapper;
using CleanOpsAi.BuildingBlocks.Application;
using CleanOpsAi.BuildingBlocks.Application.Exceptions;
using CleanOpsAi.BuildingBlocks.Application.Interfaces;
using CleanOpsAi.BuildingBlocks.Application.Pagination;
using CleanOpsAi.Modules.WorkareaCheckin.Application.Common.Interfaces.Repositories;
using CleanOpsAi.Modules.WorkareaCheckin.Application.Common.Interfaces.Services;
using CleanOpsAi.Modules.WorkareaCheckin.Application.DTOs.Request;
using CleanOpsAi.Modules.WorkareaCheckin.Application.DTOs.Response;
using CleanOpsAi.Modules.WorkareaCheckin.Domain.Entities;
using CleanOpsAi.Modules.WorkareaCheckin.Domain.Enums;

namespace CleanOpsAi.Modules.WorkareaCheckin.Application.Services
{
	public class AccessDeviceService : IAccessDeviceService
	{
		private readonly IAccessDeviceRepository _repo;
		private readonly IWorkareaCheckinPointRepository _checkinPointRepo;
		private readonly IIdGenerator _idGenerator;
		private readonly IDateTimeProvider _dateTimeProvider;
		private readonly IUserContext _userContext;
		private readonly IMapper _mapper;

		public AccessDeviceService(
			IAccessDeviceRepository accessDeviceRepository, 
			IWorkareaCheckinPointRepository checkinPointRepo,
			IIdGenerator idGenerator,
			IDateTimeProvider dateTimeProvider, 
			IUserContext userContext,
			IMapper mapper)
		{
			_repo = accessDeviceRepository;
			_checkinPointRepo = checkinPointRepo;
			_idGenerator = idGenerator;
			_dateTimeProvider = dateTimeProvider;
			_userContext = userContext;
			_mapper = mapper;
		}

		public async Task<AccessDeviceDto> Create(AccessDeviceCreateDto request, CancellationToken ct = default)
		{
			if (request.DeviceType == DeviceType.BleBeacon && string.IsNullOrWhiteSpace(request.Identifier))
				throw new BadRequestException("BLE beacon requires Identifier (device local name)");

			var checkinPoint = await _checkinPointRepo.GetByIdAsync(request.WorkareaCheckinPointId, ct);
			if (checkinPoint == null)
				throw new NotFoundException(nameof(WorkareaCheckinPoint), request.WorkareaCheckinPointId);

			if (!string.IsNullOrWhiteSpace(request.Identifier))
			{
				var existing = await _repo.GetByIdentifierAsync(request.Identifier, ct);
				if (existing != null)
					throw new BadRequestException($"Device with identifier '{request.Identifier}' already exists");
			}

			var device = _mapper.Map<AccessDevice>(request);
			device.Id = _idGenerator.Generate();
			device.Created = _dateTimeProvider.UtcNow;
			device.CreatedBy = _userContext.UserId.ToString();

			await _repo.InsertAsync(device, ct);
			await _repo.SaveChangesAsync(ct);

			return _mapper.Map<AccessDeviceDto>(device);
		}

		public Task<PaginatedResult<AccessDeviceDto>> GetByCheckinPointAsync(Guid checkinPointId, PaginationRequest request, CancellationToken ct = default)
		{
			throw new NotImplementedException();
		}

		public async Task<AccessDeviceDto> GetById(Guid id, CancellationToken ct = default)
		{
			var device = await _repo.GetByIdAsync(id, ct);
			if (device == null)
				throw new NotFoundException(nameof(AccessDevice), id);

			return _mapper.Map<AccessDeviceDto>(device);
		}

		public async Task<AccessDeviceDto?> GetByIdentifierAsync(string identifier, CancellationToken ct = default)
		{
			var device = await _repo.GetByIdentifierAsync(identifier, ct);
			if (device == null)
				throw new NotFoundException($"Device with identifier '{identifier}' not found");

			return _mapper.Map<AccessDeviceDto>(device);
		}
	}
}
