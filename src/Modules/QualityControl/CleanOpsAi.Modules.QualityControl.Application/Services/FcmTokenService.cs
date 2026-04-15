using AutoMapper;
using CleanOpsAi.BuildingBlocks.Application;
using CleanOpsAi.BuildingBlocks.Application.Interfaces;
using CleanOpsAi.Modules.QualityControl.Application.Common.Interfaces.Repositories;
using CleanOpsAi.Modules.QualityControl.Application.Common.Interfaces.Services;
using CleanOpsAi.Modules.QualityControl.Application.DTOs.Request;
using CleanOpsAi.Modules.QualityControl.Application.DTOs.Response;
using CleanOpsAi.Modules.QualityControl.Domain.Entities;

namespace CleanOpsAi.Modules.QualityControl.Application.Services
{
	public class FcmTokenService : IFcmTokenService
	{
		private readonly IFcmTokenRepository _fcmTokenRepository;
		private readonly IMapper _mapper; 
		private readonly IDateTimeProvider _dateTimeProvider;
		private readonly IUserContext _userContext;

		public FcmTokenService(IFcmTokenRepository fcmTokenRepository, IMapper mapper, 
			IDateTimeProvider dateTimeProvider,
			IUserContext userContext)
		{
			_fcmTokenRepository = fcmTokenRepository;
			_mapper = mapper; 
			_dateTimeProvider = dateTimeProvider;
			_userContext = userContext;

		}
		public async Task<FcmTokenDto> CreateOrUpdateAsync(FcmTokenCreateDto dto, CancellationToken cancellationToken = default)
		{
			var existing = await _fcmTokenRepository.GetActiveTokenAsync(dto.UniqueId, _userContext.UserId, cancellationToken);

			if (existing is not null)
			{
				existing.Token = dto.Token;
				existing.Platform = dto.Platform;
				existing.DeviceName = dto.DeviceName;
				existing.WorkerId = dto.WorkerId;
				existing.IsActive = true;
				existing.LastUsed = _dateTimeProvider.UtcNow;

				await _fcmTokenRepository.SaveChangesAsync(cancellationToken);
				return _mapper.Map<FcmTokenDto>(existing);
			}

			var duplicateToken = await _fcmTokenRepository.GetByTokenAsync(dto.Token, cancellationToken);
			if (duplicateToken is not null)
			{
				duplicateToken.IsActive = false;
			}

			var fcmToken = _mapper.Map<FcmToken>(dto); 
			fcmToken.Created = _dateTimeProvider.UtcNow;
			fcmToken.CreatedBy = _userContext.UserId.ToString();
			fcmToken.IsActive = true;

			await _fcmTokenRepository.InsertAsync(fcmToken, cancellationToken);
			await _fcmTokenRepository.SaveChangesAsync(cancellationToken);

			return _mapper.Map<FcmTokenDto>(fcmToken);

		}

		public async Task DeactivateTokenAsync(string uniqueId, CancellationToken cancellationToken = default)
		{
			var token = await _fcmTokenRepository.GetActiveTokenAsync(uniqueId, _userContext.UserId, cancellationToken);
			if (token is null) return;

			token.IsActive = false;
			token.LastUsed = _dateTimeProvider.UtcNow;

			await _fcmTokenRepository.SaveChangesAsync(cancellationToken);
		}
	}
}
