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
		public async Task<FcmTokenDto> RegisterAsync(FcmTokenRegisterDto dto,
			CancellationToken cancellationToken = default)
		{
			var now = _dateTimeProvider.UtcNow;
			var userId = _userContext.UserId;

			await DeactivateDuplicateTokenAsync(dto.Token, dto.UniqueId, cancellationToken);

			var existing = await _fcmTokenRepository
				.GetByUniqueIdAsync(dto.UniqueId, cancellationToken);

			if (existing is not null)
			{
				_mapper.Map(dto, existing);
				existing.UserId = userId;
				existing.IsActive = true;
				existing.LastUsed = now;
				await _fcmTokenRepository.SaveChangesAsync(cancellationToken);
				return _mapper.Map<FcmTokenDto>(existing);
			}

			var fcmToken = _mapper.Map<FcmToken>(dto);
			fcmToken.UserId = userId;
			fcmToken.Created = now;
			fcmToken.CreatedBy = userId.ToString();
			fcmToken.LastUsed = now;
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

			token.LastModified = _dateTimeProvider.UtcNow;
			token.LastModifiedBy = _userContext.UserId.ToString();

			await _fcmTokenRepository.SaveChangesAsync(cancellationToken);
		}

		public async Task RefreshTokenAsync(FcmTokenRefreshDto dto, CancellationToken cancellationToken = default)
		{
			var existing = await _fcmTokenRepository.GetByUniqueIdAsync(dto.UniqueId, cancellationToken);

			if (existing is null) return;
			await DeactivateDuplicateTokenAsync(dto.Token, dto.UniqueId, cancellationToken);

			existing.Token = dto.Token;
			existing.LastUsed = _dateTimeProvider.UtcNow;

			await _fcmTokenRepository.SaveChangesAsync(cancellationToken);
		}

		private async Task DeactivateDuplicateTokenAsync(
			string token,
			string uniqueId,
			CancellationToken cancellationToken)
		{
			var duplicate = await _fcmTokenRepository
				.GetByTokenAsync(token, cancellationToken);

			if (duplicate is not null && duplicate.UniqueId != uniqueId)
			{
				duplicate.IsActive = false; 
			}
		}
	}
}
