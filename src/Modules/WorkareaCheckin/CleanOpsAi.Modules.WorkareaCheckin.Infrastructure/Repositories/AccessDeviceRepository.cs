using CleanOpsAi.Modules.WorkareaCheckin.Application.Common.Interfaces.Repositories;
using CleanOpsAi.Modules.WorkareaCheckin.Domain.Entities;
using CleanOpsAi.Modules.WorkareaCheckin.Infrastructure.Data;

namespace CleanOpsAi.Modules.WorkareaCheckin.Infrastructure.Repositories
{
	public class AccessDeviceRepository : BaseRepo<AccessDevice, Guid>, IAccessDeviceRepository
	{
		public AccessDeviceRepository(WorkareaCheckinDbContext context) : base(context)
		{

		}
	}
}
