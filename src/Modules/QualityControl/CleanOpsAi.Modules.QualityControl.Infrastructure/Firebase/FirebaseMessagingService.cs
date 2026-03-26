using CleanOpsAi.BuildingBlocks.Domain.Dtos.Notifications;
using CleanOpsAi.Modules.QualityControl.Application.Common.Interfaces.Services;
using FirebaseAdmin.Messaging;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using FirebasePriority = FirebaseAdmin.Messaging.Priority;

namespace CleanOpsAi.Modules.QualityControl.Infrastructure.Firebase
{
	public class FirebaseMessagingService : IFirebaseMessagingService
	{
		private readonly FirebaseMessaging _messaging;
		private readonly ILogger<FirebaseMessagingService> _logger;

		public FirebaseMessagingService(ILogger<FirebaseMessagingService> logger)
		{
			_messaging = FirebaseMessaging.DefaultInstance;
			_logger = logger;
		}

		public async Task SendMulticastAsync(
			List<string> tokens,
			string title,
			string body,
			string payload,
			BuildingBlocks.Domain.Dtos.Notifications.NotificationPriority priority = BuildingBlocks.Domain.Dtos.Notifications.NotificationPriority.Normal,
			CancellationToken cancellationToken = default)
		{
			if (!tokens.Any()) return;

			var dataDict = new Dictionary<string, string>();
			try
			{
				var parsed = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(payload);
				if (parsed != null)
					foreach (var kv in parsed)
						dataDict[kv.Key] = kv.Value.ToString();
			}
			catch
			{
				_logger.LogWarning("Invalid payload JSON, sending without data: {Payload}", payload);
			}

			var chunks = tokens.Chunk(500);
			foreach (var chunk in chunks)
			{
				var message = new MulticastMessage
				{
					Tokens = chunk.ToList(),
					Notification = new Notification
					{
						Title = title,
						Body = body,
					},
					Data = dataDict,
					Android = new AndroidConfig
					{
						Priority = priority == BuildingBlocks.Domain.Dtos.Notifications.NotificationPriority.High
							? FirebasePriority.High
							: FirebasePriority.Normal,
						Notification = new AndroidNotification
						{
							Title = title,
							Body = body,
							Sound = "default",
						}
					},
					Apns = new ApnsConfig
					{
						Aps = new Aps
						{
							Alert = new ApsAlert { Title = title, Body = body },
							Sound = "default",
							Badge = 1,
						}
					}
				};

				var response = await _messaging.SendEachForMulticastAsync(message, cancellationToken);
				_logger.LogInformation(
					"FCM multicast sent: {Success} success, {Failure} failure out of {Total}",
					response.SuccessCount, response.FailureCount, chunk.Count());

				if (response.FailureCount > 0)
				{
					var failedTokens = response.Responses
						.Select((r, i) => new { r, token = chunk.ElementAt(i) })
						.Where(x => !x.r.IsSuccess)
						.Select(x => new { x.token, error = x.r.Exception?.Message });

					foreach (var failed in failedTokens)
						_logger.LogWarning("FCM failed token: {Token}, reason: {Error}",
							failed.token, failed.error);
				}
			}
		}
	}
}