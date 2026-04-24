namespace CleanOpsAi.Modules.TaskOperations.Application.Common.Interfaces.Services
{
    /// <summary>
    /// Abstraction over the real-time notification channel (SignalR).
    /// Defined in the Application layer so services can depend on it
    /// without referencing ASP.NET SignalR directly.
    /// </summary>
    public interface IComplianceNotifier
    {
        /// <summary>
        /// Pushes a compliance-check-updated message to the SignalR group
        /// identified by <paramref name="taskStepExecutionId"/>.
        /// </summary>
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

        /// <summary>
        /// Client-facing action hint: None | RetakePhotos | WaitForSupervisor.
        /// Derived from Status so the client never needs to interpret Status itself.
        /// </summary>
        public string Action { get; init; } = null!;

        /// <summary>UTC timestamp when the scoring result was applied.</summary>
        public DateTime At { get; init; }
    }
}
