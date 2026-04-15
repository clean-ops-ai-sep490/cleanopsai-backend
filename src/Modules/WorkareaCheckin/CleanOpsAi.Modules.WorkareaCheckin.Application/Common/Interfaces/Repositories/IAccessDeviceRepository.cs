using CleanOpsAi.Modules.WorkareaCheckin.Domain.Entities;

namespace CleanOpsAi.Modules.WorkareaCheckin.Application.Common.Interfaces.Repositories
{
	public interface IAccessDeviceRepository : IBaseRepo<AccessDevice, Guid>
	{
		Task<AccessDevice?> GetByIdentifierAsync(string identifier, CancellationToken ct = default);
	}
}
