using AutoMapper;
using CleanOpsAi.BuildingBlocks.Application;
using CleanOpsAi.BuildingBlocks.Application.Interfaces;
using CleanOpsAi.BuildingBlocks.Domain.Dtos.Notifications;
using CleanOpsAi.BuildingBlocks.Infrastructure.Events;
using CleanOpsAi.Modules.QualityControl.Application.Common.Interfaces.Repositories;
using CleanOpsAi.Modules.QualityControl.Application.Common.Interfaces.Services;
using CleanOpsAi.Modules.QualityControl.Application.DTOs.Request;
using CleanOpsAi.Modules.QualityControl.Application.DTOs.Response;
using CleanOpsAi.Modules.QualityControl.Application.Services;
using CleanOpsAi.Modules.QualityControl.Domain.Entities;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace CleanOpsAi.Modules.QualityControl.UnitTests.Services
{
	public class AppNotificationTests
	{
		private readonly INotificationRepository _notificationRepository;
		private readonly IFcmTokenRepository _fcmTokenRepository;
		private readonly IFirebaseMessagingService _firebaseMessagingService;
		private readonly IMapper _mapper;
		private readonly IDateTimeProvider _dateProvider;
		private readonly IUserContext _userContext;
		private readonly ILogger<NotificationService> _logger;
		private readonly NotificationService _service;

		public AppNotificationTests()
		{
			_notificationRepository = Substitute.For<INotificationRepository>();
			_fcmTokenRepository = Substitute.For<IFcmTokenRepository>();
			_firebaseMessagingService = Substitute.For<IFirebaseMessagingService>();
			_mapper = Substitute.For<IMapper>();
			_dateProvider = Substitute.For<IDateTimeProvider>();
			_userContext = Substitute.For<IUserContext>();
			_logger = Substitute.For<ILogger<NotificationService>>();

			_service = new NotificationService(
				_notificationRepository,
				_fcmTokenRepository,
				_firebaseMessagingService,
				_mapper,
				_dateProvider,
				_userContext,
				_logger);
		}

		[Fact]
		public async Task Create_WithAdminRole_SetsSenderTypeToAdmin()
		{
			var dto = new NotificationCreateDto
			{
				Title = "Test Notification",
				Body = "Test body"
			};

			var entity = new AppNotification();
			var expectedDto = new NotificationDto { Id = Guid.NewGuid(), Title = dto.Title };
			var userId = Guid.NewGuid();
			var now = DateTime.UtcNow;

			_mapper.Map<AppNotification>(dto).Returns(entity);
			_userContext.UserId.Returns(userId);
			_userContext.Role.Returns("Admin");
			_dateProvider.UtcNow.Returns(now);
			_notificationRepository.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);
			_mapper.Map<NotificationDto>(entity).Returns(expectedDto);

			var result = await _service.Create(dto);

			Assert.Equal(expectedDto.Id, result.Id);
			Assert.Equal(SenderTypeEnum.Admin, entity.SenderType);
			Assert.Equal(userId, entity.SenderId);
			Assert.Equal(now, entity.Created);

			await _notificationRepository.Received(1).InsertAsync(entity);
			await _notificationRepository.Received(1).SaveChangesAsync();
		}

		[Fact]
		public async Task Create_WithManagerRole_SetsSenderTypeToManager()
		{
			var dto = new NotificationCreateDto { Title = "Test" };
			var entity = new AppNotification();
			var expectedDto = new NotificationDto { Id = Guid.NewGuid() };

			_mapper.Map<AppNotification>(dto).Returns(entity);
			_userContext.UserId.Returns(Guid.NewGuid());
			_userContext.Role.Returns("Manager");
			_dateProvider.UtcNow.Returns(DateTime.UtcNow);
			_notificationRepository.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);
			_mapper.Map<NotificationDto>(entity).Returns(expectedDto);

			var result = await _service.Create(dto);

			Assert.Equal(SenderTypeEnum.Manager, entity.SenderType);
		}

		[Fact]
		public async Task Create_WithOtherRole_SetsSenderTypeToSystem()
		{
			var dto = new NotificationCreateDto { Title = "Test" };
			var entity = new AppNotification();
			var expectedDto = new NotificationDto { Id = Guid.NewGuid() };

			_mapper.Map<AppNotification>(dto).Returns(entity);
			_userContext.UserId.Returns(Guid.NewGuid());
			_userContext.Role.Returns("Other");
			_dateProvider.UtcNow.Returns(DateTime.UtcNow);
			_notificationRepository.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);
			_mapper.Map<NotificationDto>(entity).Returns(expectedDto);

			var result = await _service.Create(dto);

			Assert.Equal(SenderTypeEnum.System, entity.SenderType);
		}

		[Fact]
		public async Task GetById_WhenNotificationExists_ReturnsDto()
		{
			var id = Guid.NewGuid();
			var notification = new AppNotification { Id = id };
			var expectedDto = new NotificationDto { Id = id };

			_notificationRepository.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns(notification);
			_mapper.Map<NotificationDto>(notification).Returns(expectedDto);

			var result = await _service.GetById(id);

			Assert.Equal(expectedDto.Id, result.Id);
		}

		[Fact]
		public async Task GetById_WhenNotificationNotFound_ReturnsNull()
		{
			var id = Guid.NewGuid();
			_notificationRepository.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns((AppNotification?)null);

			var result = await _service.GetById(id);

			Assert.Null(result);
		}

		[Fact]
		public async Task Update_WhenNotificationNotFound_ReturnsNull()
		{
			var id = Guid.NewGuid();
			var dto = new NotificationUpdateDto();

			_notificationRepository.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns((AppNotification?)null);

			var result = await _service.Update(id, dto);

			Assert.Null(result);
		}

		[Fact]
		public async Task Update_WhenNotificationExists_UpdatesAndReturnsDto()
		{
			var id = Guid.NewGuid();
			var notification = new AppNotification { Id = id };
			var dto = new NotificationUpdateDto();
			var expectedDto = new NotificationDto { Id = id };
			var userId = Guid.NewGuid();
			var now = DateTime.UtcNow;

			_notificationRepository.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns(notification);
			_userContext.UserId.Returns(userId);
			_dateProvider.UtcNow.Returns(now);
			_notificationRepository.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);
			_mapper.Map<NotificationDto>(notification).Returns(expectedDto);

			var result = await _service.Update(id, dto);

			Assert.Equal(expectedDto.Id, result.Id);
			Assert.Equal(userId, notification.SenderId);
			Assert.Equal(now, notification.LastModified);
		}

		[Fact]
		public async Task Delete_WhenNotificationNotFound_ReturnsFalse()
		{
			var id = Guid.NewGuid();
			_notificationRepository.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns((AppNotification?)null);

			var result = await _service.Delete(id);

			Assert.False(result);
		}

		[Fact]
		public async Task Delete_WhenNotificationExists_SetsIsDeletedAndReturnsTrue()
		{
			var id = Guid.NewGuid();
			var notification = new AppNotification { Id = id };

			_notificationRepository.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns(notification);
			_notificationRepository.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);

			var result = await _service.Delete(id);

			Assert.True(result);
			Assert.True(notification.IsDeleted);
		}
	}
}
