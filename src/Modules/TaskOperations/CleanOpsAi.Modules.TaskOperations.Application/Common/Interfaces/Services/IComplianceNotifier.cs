namespace CleanOpsAi.Modules.TaskOperations.Application.Common.Interfaces.Services
{ 
    public interface IComplianceNotifier
    { 
        Task NotifyAsync(ComplianceCheckNotification notification, CancellationToken ct = default);
    }

    /// <summary>
    /// Payload pushed to the client via SignalR after AI scoring completes.
    /// </summary>
    public sealed class ComplianceCheckNotification
    {
        public Guid ComplianceCheckId { get; init; }
        public Guid TaskStepExecutionId { get; init; }

        /// <summary>Stringified status: Passed | PendingSupervisor | Failed.</summary>
        public string Status { get; init; } = null!;

        /// <summary>Always "AI" for automated checks.</summary>
        public string CheckedBy { get; init; } = "AI";

        public double MinScore { get; init; }
        public int FailedImageCount { get; init; }
         
        public string Action { get; init; } = null!;

        /// <summary>UTC timestamp when the scoring result was applied.</summary>
        public DateTime At { get; init; }
    }
}
