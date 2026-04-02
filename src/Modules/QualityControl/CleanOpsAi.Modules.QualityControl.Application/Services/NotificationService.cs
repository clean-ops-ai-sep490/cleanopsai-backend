using AutoMapper;
using CleanOpsAi.BuildingBlocks.Application;
using CleanOpsAi.BuildingBlocks.Application.Interfaces;
using CleanOpsAi.BuildingBlocks.Domain.Dtos.Notifications;
using CleanOpsAi.BuildingBlocks.Infrastructure.Events;
using CleanOpsAi.Modules.QualityControl.Application.Common.Interfaces.Repositories;
using CleanOpsAi.Modules.QualityControl.Application.Common.Interfaces.Services;
using CleanOpsAi.Modules.QualityControl.Application.DTOs.Request;
using CleanOpsAi.Modules.QualityControl.Application.DTOs.Response;
using CleanOpsAi.Modules.QualityControl.Domain.Entities;
using Medo;
using Microsoft.Extensions.Logging;

namespace CleanOpsAi.Modules.QualityControl.Application.Services
{
	public class NotificationService : INotificationService
	{
		private readonly INotificationRepository _notificationRepository;
		private readonly IFcmTokenRepository _fcmTokenRepository;
		private readonly IFirebaseMessagingService _firebaseMessagingService;
		private readonly IMapper _mapper;
		private readonly IDateTimeProvider _dateProvider;
		private readonly IUserContext _userContext;
		private readonly ILogger<NotificationService> _logger;

		public NotificationService(INotificationRepository notificationRepo,
			IFcmTokenRepository fcmTokenRepository,
			IFirebaseMessagingService firebaseMessagingService, IMapper mapper,
			IDateTimeProvider dateTimeProvider,
			IUserContext userContext,
			ILogger<NotificationService> logger)
		{
			_notificationRepository = notificationRepo;
			_fcmTokenRepository = fcmTokenRepository;
			_firebaseMessagingService = firebaseMessagingService; 
			_mapper = mapper;
			_dateProvider = dateTimeProvider;
			_userContext = userContext;
			_logger = logger;
		}

		public async Task<NotificationDto> Create(NotificationCreateDto dto, CancellationToken ct = default)
		{
			var entity = _mapper.Map<AppNotification>(dto);
			entity.Id = Uuid7.NewGuid();
			entity.Created = _dateProvider.UtcNow;
			entity.SenderId = _userContext.UserId;
			entity.CreatedBy = _userContext.UserId.ToString();

			if(_userContext.Role == "Admin")
			{
				entity.SenderType = SenderTypeEnum.Admin;
			}else if(_userContext.Role == "Manager")
			{
				entity.SenderType = SenderTypeEnum.Manager;
			}
			else
			{
				entity.SenderType = SenderTypeEnum.System;
			}


			await _notificationRepository.InsertAsync(entity);
			await _notificationRepository.SaveChangesAsync();

			return _mapper.Map<NotificationDto>(entity);
		}

		public async Task<NotificationDto?> Update(Guid id, NotificationUpdateDto dto, CancellationToken ct = default)
		{
			var notification = await _notificationRepository.GetByIdAsync(id, ct);
			if (notification == null) return null;

			notification.SenderId = _userContext.UserId;
			notification.LastModified = _dateProvider.UtcNow;
			notification.LastModifiedBy = _userContext.UserId.ToString();


			await _notificationRepository.SaveChangesAsync(ct);
			return _mapper.Map<NotificationDto>(notification); 
		}

		public async Task<bool> Delete(Guid id, CancellationToken ct = default)
		{
			var notification = await _notificationRepository.GetByIdAsync(id, ct);
			if (notification == null) return false;

			notification.IsDeleted = true;
			await _notificationRepository.SaveChangesAsync(ct);

			return true;
		}

		public async Task<NotificationDto?> GetById(Guid id, CancellationToken ct = default)
		{
			var notification = await _notificationRepository.GetByIdAsync(id, ct);
			return _mapper.Map<NotificationDto?>(notification);
		}

		public async Task HandleAsync(SendNotificationEvent message)
		{
			var notification = new AppNotification
			{
				Title = message.Title,
				Body = message.Body,
				Payload = message.Payload,
				Priority = message.Priority,
				SenderType = message.SenderType,
				SenderId = message.SenderId,
			};

			notification.NotificationRecipients = message.Recipients
			.Select(r => new NotificationRecipient
			{
				RecipientType = r.RecipientType,
				RecipientId = r.RecipientId
			}).ToList();

			await _notificationRepository.InsertAsync(notification);
			await _notificationRepository.SaveChangesAsync();

			var recipientIds = message.Recipients.Select(r => r.RecipientId).ToList();

			var activeTokens = await _fcmTokenRepository.GetActiveTokensByUserIdsAsync(recipientIds);

			if (!activeTokens.Any()) return;

			await _firebaseMessagingService.SendMulticastAsync(
			tokens: activeTokens.Select(t => t.Token).ToList(), title: message.Title, body: message.Body, payload: message.Payload);
		}

		public async Task HandleSendNotificationAsync(SendNotificationEvent message, CancellationToken ct = default)
		{
			if (message.Recipients == null || !message.Recipients.Any())
				return;

			// Chỉ push cho Worker và Supervisor
			var pushRecipients = message.Recipients
				.Where(r => r.RecipientType == RecipientTypeEnum.Worker ||
							r.RecipientType == RecipientTypeEnum.Supervisor)
				.ToList();

			if (!pushRecipients.Any())
				return;

			try
			{
				var activeTokens = await _fcmTokenRepository.GetActiveTokensForPushAsync(pushRecipients, ct);

				if (!activeTokens.Any())
				{
					_logger.LogInformation("No active FCM tokens found for notification: {Title}", message.Title);
					return;
				}

				await _firebaseMessagingService.SendMulticastAsync(
					tokens: activeTokens.Select(t => t.Token).ToList(),
					title: message.Title,
					body: message.Body,
					payload: message.Payload,
					cancellationToken: ct);

				_logger.LogInformation("Successfully sent push notification '{Title}' to {Count} devices",
					message.Title, activeTokens.Count);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Failed to send FCM notification. Title: {Title}", message.Title);
				throw;
			}
		}
	}
}
