namespace CleanOpsAi.Modules.ClientManagement.Application.Dtos.ContractScan
{
    /// <summary>
    /// A work-shift extracted from the contract document for user confirmation.
    /// Maps to <see cref="Domain.Entities.SlaShift"/>.
    /// </summary>
    public class ExtractedSlaShiftDto
    {
        /// <summary>Shift label, e.g. "Morning Shift", "Ca sáng".</summary>
        public string Name { get; set; } = null!;

        /// <summary>Shift start time in HH:mm format.</summary>
        public string StartTime { get; set; } = null!;

        /// <summary>Shift end time in HH:mm format.</summary>
        public string EndTime { get; set; } = null!;

        /// <summary>Number of workers required for this shift.</summary>
        public int RequiredWorker { get; set; }

        /// <summary>Break time in minutes.</summary>
        public int BreakTime { get; set; }
    }
}
