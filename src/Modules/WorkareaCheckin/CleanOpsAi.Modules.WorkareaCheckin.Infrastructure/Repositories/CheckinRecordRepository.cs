using CleanOpsAi.Modules.WorkareaCheckin.Application.Common.Interfaces.Repositories;
using CleanOpsAi.Modules.WorkareaCheckin.Domain.Entities;
using CleanOpsAi.Modules.WorkareaCheckin.Infrastructure.Data; 

namespace CleanOpsAi.Modules.WorkareaCheckin.Infrastructure.Repositories
{
	public class CheckinRecordRepository : BaseRepo<CheckinRecord, Guid> , ICheckinRecordRepository
	{
		public CheckinRecordRepository(WorkareaCheckinDbContext context) : base(context)
		{

		}
	}
}
