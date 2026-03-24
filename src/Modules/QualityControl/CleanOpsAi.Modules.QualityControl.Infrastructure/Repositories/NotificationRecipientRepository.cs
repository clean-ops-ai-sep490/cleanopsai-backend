using CleanOpsAi.Modules.QualityControl.Application.Common.Interfaces.Repositories;
using CleanOpsAi.Modules.QualityControl.Domain.Entities;
using CleanOpsAi.Modules.QualityControl.Infrastructure.Data;

namespace CleanOpsAi.Modules.QualityControl.Infrastructure.Repositories
{
	public class NotificationRecipientRepository : BaseRepo<NotificationRecipient, Guid>, INotificationRecipientRepository
	{
		public NotificationRecipientRepository(QualityControlDbContext context) : base(context)
		{
			
		}
	}
}
