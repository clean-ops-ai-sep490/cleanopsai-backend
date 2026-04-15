using AutoMapper;
using CleanOpsAi.BuildingBlocks.Application;
using CleanOpsAi.BuildingBlocks.Application.Interfaces;
using CleanOpsAi.Modules.QualityControl.Application.Common.Interfaces.Repositories;
using CleanOpsAi.Modules.QualityControl.Application.Common.Interfaces.Services;
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
		public async Task CreateOrUpdateAsync_WhenTokenNotExists_CreatesNewToken()
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
			var token = new FcmToken();
			var expectedDto = new FcmTokenDto { Token = dto.Token };

			_fcmTokenRepository.GetActiveTokenAsync(dto.UniqueId, userId, Arg.Any<CancellationToken>()).Returns((FcmToken?)null);
			_fcmTokenRepository.GetByTokenAsync(dto.Token, Arg.Any<CancellationToken>()).Returns((FcmToken?)null);
			_mapper.Map<FcmToken>(dto).Returns(token);
			_userContext.UserId.Returns(userId);
			_dateTimeProvider.UtcNow.Returns(now);
			_fcmTokenRepository.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);
			_mapper.Map<FcmTokenDto>(token).Returns(expectedDto);

			var result = await _service.CreateOrUpdateAsync(dto);

			Assert.Equal(expectedDto.Token, result.Token);
			Assert.Equal(now, token.Created);
			Assert.True(token.IsActive);

			await _fcmTokenRepository.Received(1).InsertAsync(token, Arg.Any<CancellationToken>());
			await _fcmTokenRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
		}

		[Fact]
		public async Task CreateOrUpdateAsync_WhenTokenExists_UpdatesExistingToken()
		{
			var dto = new FcmTokenRegisterDto
			{
				UniqueId = "device123",
				Token = "new_fcm_token_123",
				Platform = DevicePlatform.iOS,
				DeviceName = "Updated Device"
			};

			var userId = Guid.NewGuid();
			var now = DateTime.UtcNow;
			var existingToken = new FcmToken { Token = "old_token", IsActive = true };
			var expectedDto = new FcmTokenDto { Token = dto.Token };

			_fcmTokenRepository.GetActiveTokenAsync(dto.UniqueId, userId, Arg.Any<CancellationToken>()).Returns(existingToken);
			_userContext.UserId.Returns(userId);
			_dateTimeProvider.UtcNow.Returns(now);
			_fcmTokenRepository.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);
			_mapper.Map<FcmTokenDto>(existingToken).Returns(expectedDto);

			var result = await _service.CreateOrUpdateAsync(dto);

			Assert.Equal(dto.Token, existingToken.Token);
			Assert.Equal(dto.Platform, existingToken.Platform);
			Assert.Equal(dto.DeviceName, existingToken.DeviceName);
			Assert.Equal(now, existingToken.LastUsed);
		}

		[Fact]
		public async Task CreateOrUpdateAsync_WhenDuplicateTokenExists_DeactivatesOldToken()
		{
			var dto = new FcmTokenRegisterDto
			{
				UniqueId = "device123",
				Token = "duplicate_token"
			};

			var userId = Guid.NewGuid();
			var now = DateTime.UtcNow;
			var oldToken = new FcmToken { Token = "duplicate_token", IsActive = true };
			var newToken = new FcmToken();
			var expectedDto = new FcmTokenDto { Token = dto.Token };

			_fcmTokenRepository.GetActiveTokenAsync(dto.UniqueId, userId, Arg.Any<CancellationToken>()).Returns((FcmToken?)null);
			_fcmTokenRepository.GetByTokenAsync(dto.Token, Arg.Any<CancellationToken>()).Returns(oldToken);
			_mapper.Map<FcmToken>(dto).Returns(newToken);
			_userContext.UserId.Returns(userId);
			_dateTimeProvider.UtcNow.Returns(now);
			_fcmTokenRepository.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);
			_mapper.Map<FcmTokenDto>(newToken).Returns(expectedDto);

			var result = await _service.CreateOrUpdateAsync(dto);

			Assert.False(oldToken.IsActive);
		}

		[Fact]
		public async Task DeactivateTokenAsync_WhenTokenExists_DeactivatesIt()
		{
			var uniqueId = "device123";
			var userId = Guid.NewGuid();
			var now = DateTime.UtcNow;
			var token = new FcmToken { IsActive = true };

			_fcmTokenRepository.GetActiveTokenAsync(uniqueId, userId, Arg.Any<CancellationToken>()).Returns(token);
			_userContext.UserId.Returns(userId);
			_dateTimeProvider.UtcNow.Returns(now);
			_fcmTokenRepository.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);

			await _service.DeactivateTokenAsync(uniqueId);

			Assert.False(token.IsActive);
			Assert.Equal(now, token.LastUsed);
		}

		[Fact]
		public async Task DeactivateTokenAsync_WhenTokenNotExists_ReturnsWithoutError()
		{
			var uniqueId = "device_not_exists";
			var userId = Guid.NewGuid();

			_fcmTokenRepository.GetActiveTokenAsync(uniqueId, userId, Arg.Any<CancellationToken>()).Returns((FcmToken?)null);
			_userContext.UserId.Returns(userId);

			await _service.DeactivateTokenAsync(uniqueId);

			await _fcmTokenRepository.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
		}
	}
}
