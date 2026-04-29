using CleanOpsAi.Modules.TaskOperations.Application.DTOs; 

namespace CleanOpsAi.Modules.TaskOperations.Application.Common.Interfaces.Services
{
	public interface IPpeCheckNotifier
	{
		Task NotifyAsync(PpeCheckNotification notification, CancellationToken ct = default);
	}
}
