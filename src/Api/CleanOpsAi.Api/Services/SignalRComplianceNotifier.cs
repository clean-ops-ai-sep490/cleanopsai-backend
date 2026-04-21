using CleanOpsAi.Api.Hubs;
using CleanOpsAi.Modules.TaskOperations.Application.Common.Interfaces.Services;
using Microsoft.AspNetCore.SignalR;

namespace CleanOpsAi.Api.Services
{ 
    public class SignalRComplianceNotifier : IComplianceNotifier
    {
        private readonly IHubContext<ComplianceHub> _hubContext;
        private readonly ILogger<SignalRComplianceNotifier> _logger;

        public SignalRComplianceNotifier(
            IHubContext<ComplianceHub> hubContext,
            ILogger<SignalRComplianceNotifier> logger)
        {
            _hubContext = hubContext;
            _logger = logger;
        }

        /// <inheritdoc/>
        public async Task NotifyAsync(
            ComplianceCheckNotification notification,
            CancellationToken ct = default)
        {
            var group = ComplianceHub.GroupName(notification.TaskStepExecutionId);

            var payload = new
            {
                complianceCheckId = notification.ComplianceCheckId,
                taskStepExecutionId = notification.TaskStepExecutionId,
                status = notification.Status,
                checkedBy = notification.CheckedBy,
                minScore = notification.MinScore,
                failedImageCount = notification.FailedImageCount,
                action = notification.Action,
                at = notification.At
            };

            await _hubContext.Clients
                .Group(group)
                .SendAsync("compliance-check-updated", payload, ct);

            _logger.LogDebug(
                "Pushed compliance-check-updated to SignalR group {Group}: " +
                "Status={Status}, Action={Action}",
                group, notification.Status, notification.Action);
        }
    }
}
