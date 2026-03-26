using CleanOpsAi.BuildingBlocks.Domain.Dtos.Notifications;

namespace CleanOpsAi.Modules.QualityControl.Application.Common.Interfaces.Services
{
	public interface IFirebaseMessagingService
	{
		Task SendMulticastAsync(
		List<string> tokens,
		string title,
		string body,
		string payload,
		NotificationPriority priority = NotificationPriority.Normal,
		CancellationToken cancellationToken = default);
	}
}
