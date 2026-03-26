using CleanOpsAi.BuildingBlocks.Infrastructure.Events;
using CleanOpsAi.Modules.QualityControl.Application.DTOs.Request;
using CleanOpsAi.Modules.QualityControl.Application.DTOs.Response;

namespace CleanOpsAi.Modules.QualityControl.Application.Common.Interfaces.Services
{
	public interface INotificationService
	{
		Task HandleAsync(SendNotificationEvent message);

		Task<NotificationDto?> GetById(Guid id, CancellationToken ct = default);

		Task<NotificationDto> Create(NotificationCreateDto dto, Guid userId, string role, CancellationToken ct = default);

		Task<NotificationDto?> Update(Guid id, NotificationUpdateDto dto, Guid userId, CancellationToken ct = default);

		Task<bool> Delete(Guid id, CancellationToken ct = default);
	}
}
