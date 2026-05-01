using CleanOpsAi.Api.Hubs;
using CleanOpsAi.Modules.TaskOperations.Application.Common.Interfaces.Services;
using CleanOpsAi.Modules.TaskOperations.Application.DTOs;
using Microsoft.AspNetCore.SignalR;

namespace CleanOpsAi.Api.Services
{
	public class SignalRPpeCheckNotifier : IPpeCheckNotifier
	{
		private readonly IHubContext<ComplianceHub> _hubContext;
		private readonly ILogger<SignalRPpeCheckNotifier> _logger;

		public SignalRPpeCheckNotifier(
		IHubContext<ComplianceHub> hubContext,
		ILogger<SignalRPpeCheckNotifier> logger)
		{
			_hubContext = hubContext;
			_logger = logger;
		}


		public async Task NotifyAsync(PpeCheckNotification notification, CancellationToken ct = default)
		{
			var group = ComplianceHub.PpeGroupName(notification.TaskStepExecutionId);
			var payload = new
			{
				taskStepExecutionId = notification.TaskStepExecutionId,
				status = notification.Status,
				message = notification.Message, 
				missingItems = notification.MissingItems,
				at = notification.At,
			};

			await _hubContext.Clients
				.Group(group)
				.SendAsync("ppe-check-updated", payload, ct);

			_logger.LogDebug(
				"Pushed ppe-check-updated to SignalR group {Group}: Status={Status}",
				group, notification.Status);
		}
	}
}
