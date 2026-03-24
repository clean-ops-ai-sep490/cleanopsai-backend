using CleanOpsAi.BuildingBlocks.Infrastructure.Events;
using CleanOpsAi.Modules.QualityControl.Application.Common.Interfaces.Repositories;
using CleanOpsAi.Modules.QualityControl.Application.Common.Interfaces.Services;
using CleanOpsAi.Modules.QualityControl.Domain.Entities; 

namespace CleanOpsAi.Modules.QualityControl.Application.Services
{
	public class NotificationService : INotificationService
	{
		private readonly INotificationRepository _notificationRepository;
		private readonly IFcmTokenRepository _fcmTokenRepository;
		private readonly IFirebaseMessagingService _firebaseMessagingService;

		public NotificationService(INotificationRepository notificationRepo,
			IFcmTokenRepository fcmTokenRepository,
			IFirebaseMessagingService firebaseMessagingService)
		{
			_notificationRepository = notificationRepo;
			_fcmTokenRepository = fcmTokenRepository;
			_firebaseMessagingService = firebaseMessagingService;
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
