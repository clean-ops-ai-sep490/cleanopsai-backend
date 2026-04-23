namespace CleanOpsAi.Modules.ClientManagement.Application.Dtos.ContractScan
{
    /// <summary>
    /// An SLA block extracted from the contract document, grouped by location / work area.
    /// The user confirms or edits this structure before it is persisted.
    /// </summary>
    public class ExtractedSlaDto
    {
        /// <summary>SLA name derived from the contract section heading, e.g. "Lobby Cleaning SLA".</summary>
        public string Name { get; set; } = null!;

        /// <summary>Optional description / scope statement found in the contract.</summary>
        public string? Description { get; set; }

        /// <summary>
        /// Work-area / location name as mentioned in the contract.
        /// The frontend should let the user map this to an existing WorkArea entity.
        /// </summary>
        public string? WorkAreaName { get; set; }

        /// <summary>Shifts defined for this SLA section.</summary>
        public List<ExtractedSlaShiftDto> Shifts { get; set; } = [];

        /// <summary>Recurring tasks defined for this SLA section.</summary>
        public List<ExtractedSlaTaskDto> Tasks { get; set; } = [];
    }
}
