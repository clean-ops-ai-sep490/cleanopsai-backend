using CleanOpsAi.BuildingBlocks.Domain.Dtos;

namespace CleanOpsAi.Modules.ClientManagement.Application.Dtos.ContractScan
{
    /// <summary>
    /// A recurring cleaning task extracted from the contract document for user confirmation.
    /// Maps to <see cref="Domain.Entities.SlaTask"/>.
    /// </summary>
    public class ExtractedSlaTaskDto
    {
        /// <summary>Task description, e.g. "Sweep lobby floor".</summary>
        public string Name { get; set; } = null!;

        /// <summary>Recurrence cadence detected from the contract text.</summary>
        public RecurrenceType RecurrenceType { get; set; }

        /// <summary>Structured recurrence configuration matching the detected cadence.</summary>
        public RecurrenceConfigSlaTask RecurrenceConfig { get; set; } = null!;

        /// <summary>
        /// Raw sentence(s) from the contract that led to this extraction.
        /// Shown to the user so they can verify the AI's interpretation.
        /// </summary>
        public string? SourceText { get; set; }
    }
}
