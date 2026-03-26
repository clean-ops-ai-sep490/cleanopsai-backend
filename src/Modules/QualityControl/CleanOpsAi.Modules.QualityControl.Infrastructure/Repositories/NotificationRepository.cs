using CleanOpsAi.Modules.QualityControl.Application.Common.Interfaces.Repositories;
using CleanOpsAi.Modules.QualityControl.Domain.Entities;
using CleanOpsAi.Modules.QualityControl.Infrastructure.Data;

namespace CleanOpsAi.Modules.QualityControl.Infrastructure.Repositories
{
	public class NotificationRepository : BaseRepo<AppNotification, Guid>, INotificationRepository
	{
		public NotificationRepository(QualityControlDbContext context) : base(context)
		{
			
		}
	}
}
