using CleanOpsAi.BuildingBlocks.Application;
using CleanOpsAi.BuildingBlocks.Application.Exceptions;
using CleanOpsAi.BuildingBlocks.Application.Interfaces;
using CleanOpsAi.Modules.WorkareaCheckin.Application.Common.Interfaces.Repositories;
using CleanOpsAi.Modules.WorkareaCheckin.Application.Common.Interfaces.Services;
using CleanOpsAi.Modules.WorkareaCheckin.Application.DTOs.Request;
using CleanOpsAi.Modules.WorkareaCheckin.Application.DTOs.Response;
using CleanOpsAi.Modules.WorkareaCheckin.Domain.Entities;
using CleanOpsAi.Modules.WorkareaCheckin.Domain.Enums;

namespace CleanOpsAi.Modules.WorkareaCheckin.Application.Services
{
	public class CheckinRecordService : ICheckinRecordService
	{
		private readonly ICheckinRecordRepository _checkinRecordRepository;
		private readonly IWorkareaCheckinPointRepository _pointRepo;
		private readonly IAccessDeviceRepository _deviceRepo;
		private readonly IIdGenerator _idGenerator;
		private readonly IDateTimeProvider _dateTimeProvider;
		private readonly IUserContext _userContext;

		public CheckinRecordService(
			ICheckinRecordRepository checkinRecordRepository,
			IWorkareaCheckinPointRepository pointRepo,
			IAccessDeviceRepository deviceRepo,
			IIdGenerator idGenerator,
			IDateTimeProvider dateTimeProvider,
			IUserContext userContext)
		{
			_checkinRecordRepository = checkinRecordRepository;
			_pointRepo = pointRepo;
			_deviceRepo = deviceRepo;
			_idGenerator = idGenerator;
			_dateTimeProvider = dateTimeProvider;
			_userContext = userContext;
		}

		public async Task<CheckinResponseDto> Checkin(CheckinRequestDto request, CancellationToken ct = default)
		{
			if (request.WorkareaId == null && string.IsNullOrWhiteSpace(request.DeviceUuid))
				throw new BadRequestException("QR or BLE data is required");

			Guid pointId;
			Guid? deviceId = null;
			CheckinType type;
			 
			if (request.WorkareaId != null)
			{
				var point = await _pointRepo.GetFirstByWorkarea(request.WorkareaId.Value, ct);
				if (point == null)
					throw new NotFoundException("CheckinPoint not found");

				pointId = point.Id;
				type = CheckinType.Qr;
			} 
			else
			{
				throw new NotImplementedException("BLE checkin is not implemented yet");
				//var device = await _deviceRepo.GetByUuid(request.DeviceUuid!, ct);
				//if (device == null)
				//	throw new NotFoundException("Device not found");

				//pointId = device.WorkareaCheckinPointId;
				//deviceId = device.Id;
				//type = CheckinType.Ble;
			}

			var record = new CheckinRecord
			{
				Id = _idGenerator.Generate(),
				WorkerId = request.WorkerId,

				WorkareaCheckinPointId = pointId,
				AccessDeviceId = deviceId,

				TaskId = request.TaskId,
				TaskStepId = request.TaskStepId,

				CheckinAt = _dateTimeProvider.UtcNow,
				CheckinType = type,
				Status = CheckinStatus.Active,

				Notes = request.Notes, 

				Created = _dateTimeProvider.UtcNow,
				CreatedBy = _userContext.UserId.ToString()
			};

			await _checkinRecordRepository.InsertAsync(record, ct);
			await _checkinRecordRepository.SaveChangesAsync(ct);

			return new CheckinResponseDto
			{
				Id = record.Id,
				CheckinAt = record.CheckinAt,
				Type = record.CheckinType
			};
		}

	}
}
