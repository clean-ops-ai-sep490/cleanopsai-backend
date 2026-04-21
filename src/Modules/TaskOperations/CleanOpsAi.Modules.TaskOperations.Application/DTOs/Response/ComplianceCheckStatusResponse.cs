namespace CleanOpsAi.Modules.TaskOperations.Application.DTOs.Response
{
    /// <summary>
    /// Polling/fallback response for the current AI compliance check status.
    /// Returned by GET /api/task-step-executions/{id}/compliance-check.
    /// </summary>
    public sealed class ComplianceCheckStatusResponse
    {
        public Guid ComplianceCheckId { get; init; }
        public Guid TaskStepExecutionId { get; init; }

        /// <summary>Stringified <c>ComplianceCheckStatus</c> enum value.</summary>
        public string Status { get; init; } = null!;

        public double MinScore { get; init; }
        public int FailedImageCount { get; init; }

        /// <summary>UTC timestamp of the last update.</summary>
        public DateTime? UpdatedAt { get; init; }
    }
}
