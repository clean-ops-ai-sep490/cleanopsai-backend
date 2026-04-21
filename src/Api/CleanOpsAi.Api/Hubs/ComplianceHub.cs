using Microsoft.AspNetCore.SignalR;

namespace CleanOpsAi.Api.Hubs
{
    public class ComplianceHub : Hub
    {
        private readonly ILogger<ComplianceHub> _logger;

        public ComplianceHub(ILogger<ComplianceHub> logger)
        {
            _logger = logger;
        }
         
        public async Task JoinExecution(Guid taskStepExecutionId)
        {
            var groupName = GroupName(taskStepExecutionId);
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);

            _logger.LogDebug(
                "Connection {ConnectionId} joined compliance group {Group}",
                Context.ConnectionId, groupName);
        }
         
        public async Task LeaveExecution(Guid taskStepExecutionId)
        {
            var groupName = GroupName(taskStepExecutionId);
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);

            _logger.LogDebug(
                "Connection {ConnectionId} left compliance group {Group}",
                Context.ConnectionId, groupName);
        }

        /// <summary>Consistent group key: lower-case stringified Guid.</summary>
        public static string GroupName(Guid taskStepExecutionId) =>
            $"compliance:{taskStepExecutionId:D}";
    }
}
