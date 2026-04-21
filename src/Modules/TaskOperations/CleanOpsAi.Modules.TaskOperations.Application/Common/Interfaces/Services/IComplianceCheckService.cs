using CleanOpsAi.BuildingBlocks.Infrastructure.Events.Request;
using CleanOpsAi.Modules.TaskOperations.Application.DTOs.Response;

namespace CleanOpsAi.Modules.TaskOperations.Application.Common.Interfaces.Services
{
    /// <summary>
    /// Orchestrates the full lifecycle of AI-automated <c>ComplianceCheck</c> records:
    /// initiation, scoring submission, result application, and real-time notification.
    /// </summary>
    public interface IComplianceCheckService
    {
        /// <summary>
        /// Initiates an AI compliance check for the given step execution:
        /// <list type="number">
        ///   <item>Creates (or reuses) a <c>Pending</c> <c>ComplianceCheck</c> record.</item>
        ///   <item>Fetches all <c>After</c>-type images for the execution.</item>
        ///   <item>Submits the images to <c>IScoringJobService.SubmitAsync</c>,
        ///     using the compliance check ID as <c>RequestId</c> for later correlation.</item>
        /// </list>
        /// </summary>
        /// <param name="taskStepExecutionId">The step execution to check.</param> 
        ///   Scoring environment key (e.g. <c>"LOBBY_CORRIDOR"</c>).
        /// </param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>
        ///   An <see cref="InitiateAiCheckResult"/> carrying the compliance check ID,
        ///   scoring job ID, and queue status — enough for the controller's 202 response.
        /// </returns>
        Task<InitiateAiCheckResult> InitiateAiCheckAsync(
            Guid taskStepExecutionId,
            CancellationToken ct = default);

        /// <summary>
        /// Processes a completed scoring batch (called by the RabbitMQ consumer):
        /// <list type="bullet">
        ///   <item>Updates <c>QualityScore</c> and <c>Verdict</c> on each matched image.</item>
        ///   <item>Calculates <c>MinScore</c> and <c>FailedImageCount</c>.</item>
        ///   <item>Determines the final <c>ComplianceCheckStatus</c> via business rules.</item>
        ///   <item>Upserts the <c>ComplianceCheck</c> record with the raw JSON payload.</item>
        ///   <item>Pushes the result to the client via SignalR (through <see cref="IComplianceNotifier"/>).</item>
        /// </list>
        /// </summary>
        Task ApplyScoringResultsAsync(
			ScoringCompletedEvent evt,
            CancellationToken ct = default);

        /// <summary>
        /// Returns the current AI <c>ComplianceCheck</c> status for the given step execution.
        /// Used as a polling fallback when the client reconnects after a SignalR disconnect.
        /// Returns <c>null</c> if no AI compliance check exists yet.
        /// </summary>
        Task<ComplianceCheckStatusResponse?> GetCurrentStatusAsync(
            Guid taskStepExecutionId,
            CancellationToken ct = default);
    }
}
