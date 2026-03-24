using CleanOpsAi.Modules.QualityControl.Domain.Entities;

namespace CleanOpsAi.Modules.QualityControl.Application.Common.Interfaces.Repositories
{
	public interface INotificationRepository : IBaseRepo<AppNotification, Guid>
	{
	}
}
