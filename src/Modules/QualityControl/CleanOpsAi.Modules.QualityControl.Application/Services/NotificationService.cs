using AutoMapper; 
using CleanOpsAi.BuildingBlocks.Application.Interfaces;
using CleanOpsAi.BuildingBlocks.Domain.Dtos.Notifications;
using CleanOpsAi.BuildingBlocks.Infrastructure.Events;
using CleanOpsAi.Modules.QualityControl.Application.Common.Interfaces.Repositories;
using CleanOpsAi.Modules.QualityControl.Application.Common.Interfaces.Services;
using CleanOpsAi.Modules.QualityControl.Application.DTOs.Request;
using CleanOpsAi.Modules.QualityControl.Application.DTOs.Response;
using CleanOpsAi.Modules.QualityControl.Domain.Entities;
using Medo;

namespace CleanOpsAi.Modules.QualityControl.Application.Services
{
	public class NotificationService : INotificationService
	{
		private readonly INotificationRepository _notificationRepository;
		private readonly IFcmTokenRepository _fcmTokenRepository;
		private readonly IFirebaseMessagingService _firebaseMessagingService;
		private readonly IMapper _mapper;
		private readonly IDateTimeProvider _dateProvider;

		public NotificationService(INotificationRepository notificationRepo,
			IFcmTokenRepository fcmTokenRepository,
			IFirebaseMessagingService firebaseMessagingService, IMapper mapper,
			IDateTimeProvider dateTimeProvider)
		{
			_notificationRepository = notificationRepo;
			_fcmTokenRepository = fcmTokenRepository;
			_firebaseMessagingService = firebaseMessagingService; 
			_mapper = mapper;
			_dateProvider = dateTimeProvider;
		}

		public async Task<NotificationDto> Create(NotificationCreateDto dto, Guid userId, string role, CancellationToken ct = default)
		{
			var entity = _mapper.Map<AppNotification>(dto);
			entity.Id = Uuid7.NewGuid();
			entity.Created = _dateProvider.UtcNow;
			entity.SenderId = userId;
			entity.CreatedBy = userId.ToString();

			if(role == "Admin")
			{
				entity.SenderType = SenderTypeEnum.Admin;
			}else if(role == "Manager")
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

		public async Task<NotificationDto?> Update(Guid id, NotificationUpdateDto dto, Guid userId, CancellationToken ct = default)
		{
			var notification = await _notificationRepository.GetByIdAsync(id, ct);
			if (notification == null) return null;

			notification.SenderId = userId;
			notification.LastModified = _dateProvider.UtcNow;
			notification.LastModifiedBy = userId.ToString();


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

		
	}
}
