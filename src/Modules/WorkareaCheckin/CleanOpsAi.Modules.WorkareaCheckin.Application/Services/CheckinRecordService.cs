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
			bool isQr = request.WorkareaId != null && !string.IsNullOrWhiteSpace(request.Code);
			bool isBle = !string.IsNullOrWhiteSpace(request.DeviceUuid);

			if (!isQr && !isBle)
				throw new BadRequestException("QR or BLE data is required");

			Guid pointId;
			Guid? deviceId = null;
			CheckinType type;
			 
			if (isQr)
			{
				if (request.CheckinPointId == null)
					throw new BadRequestException("CheckinPointId is required for QR");

				var checkPoint = await _pointRepo.GetByIdAsync(request.CheckinPointId.Value, ct);
				if (checkPoint == null)
					throw new NotFoundException("CheckinPoint not found");

				if (!string.Equals(checkPoint.Code, request.Code, StringComparison.OrdinalIgnoreCase))
					throw new BadRequestException("Invalid QR");

				if (checkPoint.WorkareaId != request.WorkareaId)
					throw new BadRequestException("Wrong workarea");

				pointId = checkPoint.Id;
				type = CheckinType.Qr;
			}
			else
			{
				if (request.Rssi == null)
					throw new BadRequestException("RSSI is required for BLE check-in");

				var device = await _deviceRepo.GetByUuidAsync(request.DeviceUuid!, ct);
				if (device == null)
					throw new NotFoundException("Device not found");

				if (device.Status != DeviceStatus.Active)
					throw new BadRequestException("Device is inactive");

				var threshold = device.BleInfo?.RssiThreshold ?? -75;

				if (request.Rssi < threshold)
					throw new BadRequestException("Signal too weak");

				// optional tracking
				if (device.BleInfo != null)
				{
					device.BleInfo.LastRssi = request.Rssi;
					device.BleInfo.LastSeenAt = _dateTimeProvider.UtcNow;
				}

				pointId = device.WorkareaCheckinPointId;
				deviceId = device.Id;
				type = CheckinType.Ble;
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
