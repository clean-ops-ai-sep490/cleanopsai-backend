using CleanOpsAi.BuildingBlocks.Infrastructure.Events;

namespace CleanOpsAi.Modules.QualityControl.Application.Common.Interfaces.Services
{
	public interface INotificationService
	{
		Task HandleAsync(SendNotificationEvent message);
	}
}
