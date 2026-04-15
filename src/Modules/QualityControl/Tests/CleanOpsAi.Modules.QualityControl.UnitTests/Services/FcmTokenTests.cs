using AutoMapper;
using CleanOpsAi.BuildingBlocks.Application;
using CleanOpsAi.BuildingBlocks.Application.Interfaces;
using CleanOpsAi.Modules.QualityControl.Application.Common.Interfaces.Repositories; 
using CleanOpsAi.Modules.QualityControl.Application.DTOs.Request;
using CleanOpsAi.Modules.QualityControl.Application.DTOs.Response;
using CleanOpsAi.Modules.QualityControl.Application.Services;
using CleanOpsAi.Modules.QualityControl.Domain.Entities;
using CleanOpsAi.Modules.QualityControl.Domain.Enums;
using NSubstitute;

namespace CleanOpsAi.Modules.QualityControl.UnitTests.Services
{
	public class FcmTokenTests
	{
		private readonly IFcmTokenRepository _fcmTokenRepository;
		private readonly IMapper _mapper;
		private readonly IDateTimeProvider _dateTimeProvider;
		private readonly IUserContext _userContext;
		private readonly FcmTokenService _service;

		public FcmTokenTests()
		{
			_fcmTokenRepository = Substitute.For<IFcmTokenRepository>();
			_mapper = Substitute.For<IMapper>();
			_dateTimeProvider = Substitute.For<IDateTimeProvider>();
			_userContext = Substitute.For<IUserContext>();

			_service = new FcmTokenService(
				_fcmTokenRepository,
				_mapper,
				_dateTimeProvider,
				_userContext);
		}

		[Fact]
		public async Task RegisterAsync_WhenTokenNotExists_CreatesNewToken()
		{
			var dto = new FcmTokenRegisterDto
			{
				UniqueId = "device123",
				Token = "fcm_token_123",
				Platform = DevicePlatform.Android,
				DeviceName = "Device Name"
			};

			var userId = Guid.NewGuid();
			var now = DateTime.UtcNow;

			var entity = new FcmToken();
			var expectedDto = new FcmTokenDto { Token = dto.Token };

			_userContext.UserId.Returns(userId);
			_dateTimeProvider.UtcNow.Returns(now);

			_fcmTokenRepository.GetByUniqueIdAsync(dto.UniqueId, Arg.Any<CancellationToken>())
				.Returns((FcmToken?)null);

			_fcmTokenRepository.GetByTokenAsync(dto.Token, Arg.Any<CancellationToken>())
				.Returns((FcmToken?)null);

			_mapper.Map<FcmToken>(dto).Returns(entity);
			_mapper.Map<FcmTokenDto>(entity).Returns(expectedDto);

			var result = await _service.RegisterAsync(dto);

			Assert.Equal(dto.Token, result.Token);
			Assert.Equal(userId, entity.UserId);
			Assert.Equal(now, entity.Created);
			Assert.Equal(now, entity.LastUsed);
			Assert.True(entity.IsActive);

			await _fcmTokenRepository.Received(1).InsertAsync(entity, Arg.Any<CancellationToken>());
			await _fcmTokenRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
		}

		[Fact]
		public async Task RegisterAsync_WhenTokenExists_UpdatesExistingToken()
		{
			var dto = new FcmTokenRegisterDto
			{
				UniqueId = "device123",
				Token = "new_token",
				Platform = DevicePlatform.iOS,
				DeviceName = "Updated Device"
			};

			var userId = Guid.NewGuid();
			var now = DateTime.UtcNow;

			var existing = new FcmToken { UniqueId = dto.UniqueId };
			var expectedDto = new FcmTokenDto { Token = dto.Token };

			_userContext.UserId.Returns(userId);
			_dateTimeProvider.UtcNow.Returns(now);

			_fcmTokenRepository.GetByUniqueIdAsync(dto.UniqueId, Arg.Any<CancellationToken>())
				.Returns(existing);

			_fcmTokenRepository.GetByTokenAsync(dto.Token, Arg.Any<CancellationToken>())
				.Returns((FcmToken?)null);

			_mapper.Map(dto, existing);
			_mapper.Map<FcmTokenDto>(existing).Returns(expectedDto);

			var result = await _service.RegisterAsync(dto);

			Assert.Equal(userId, existing.UserId);
			Assert.True(existing.IsActive);
			Assert.Equal(now, existing.LastUsed);

			await _fcmTokenRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
		}

		[Fact]
		public async Task RegisterAsync_WhenDuplicateTokenExists_DeactivatesOldToken()
		{
			var dto = new FcmTokenRegisterDto
			{
				UniqueId = "device123",
				Token = "duplicate_token"
			};

			var userId = Guid.NewGuid();
			var now = DateTime.UtcNow;

			var oldToken = new FcmToken
			{
				Token = dto.Token,
				UniqueId = "other_device",
				IsActive = true
			};

			var newEntity = new FcmToken();
			var expectedDto = new FcmTokenDto { Token = dto.Token };

			_userContext.UserId.Returns(userId);
			_dateTimeProvider.UtcNow.Returns(now);

			_fcmTokenRepository.GetByUniqueIdAsync(dto.UniqueId, Arg.Any<CancellationToken>())
				.Returns((FcmToken?)null);

			_fcmTokenRepository.GetByTokenAsync(dto.Token, Arg.Any<CancellationToken>())
				.Returns(oldToken);

			_mapper.Map<FcmToken>(dto).Returns(newEntity);
			_mapper.Map<FcmTokenDto>(newEntity).Returns(expectedDto);

			await _service.RegisterAsync(dto);

			Assert.False(oldToken.IsActive);
		}

		[Fact]
		public async Task DeactivateTokenAsync_WhenTokenExists_DeactivatesIt()
		{
			var uniqueId = "device123";
			var userId = Guid.NewGuid();
			var now = DateTime.UtcNow;

			var token = new FcmToken { IsActive = true };

			_userContext.UserId.Returns(userId);
			_dateTimeProvider.UtcNow.Returns(now);

			_fcmTokenRepository.GetActiveTokenAsync(uniqueId, userId, Arg.Any<CancellationToken>())
				.Returns(token);

			await _service.DeactivateTokenAsync(uniqueId);

			Assert.False(token.IsActive);
			Assert.Equal(now, token.LastUsed);

			await _fcmTokenRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
		}

		[Fact]
		public async Task DeactivateTokenAsync_WhenTokenNotExists_DoesNothing()
		{
			var uniqueId = "device_not_found";
			var userId = Guid.NewGuid();

			_userContext.UserId.Returns(userId);

			_fcmTokenRepository.GetActiveTokenAsync(uniqueId, userId, Arg.Any<CancellationToken>())
				.Returns((FcmToken?)null);

			await _service.DeactivateTokenAsync(uniqueId);

			await _fcmTokenRepository.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
		}

		[Fact]
		public async Task RefreshTokenAsync_WhenTokenExists_UpdatesToken()
		{
			var dto = new FcmTokenRefreshDto
			{
				UniqueId = "device123",
				Token = "new_token"
			};

			var now = DateTime.UtcNow;

			var existing = new FcmToken { UniqueId = dto.UniqueId };

			_dateTimeProvider.UtcNow.Returns(now);

			_fcmTokenRepository.GetByUniqueIdAsync(dto.UniqueId, Arg.Any<CancellationToken>())
				.Returns(existing);

			_fcmTokenRepository.GetByTokenAsync(dto.Token, Arg.Any<CancellationToken>())
				.Returns((FcmToken?)null);

			await _service.RefreshTokenAsync(dto);

			Assert.Equal(dto.Token, existing.Token);
			Assert.Equal(now, existing.LastUsed);

			await _fcmTokenRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
		}
	}
}
