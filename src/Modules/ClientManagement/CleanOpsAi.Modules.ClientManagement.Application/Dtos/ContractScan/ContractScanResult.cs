namespace CleanOpsAi.Modules.ClientManagement.Application.Dtos.ContractScan
{
    /// <summary>
    /// Top-level result returned to the caller after scanning and AI-parsing a contract document.
    /// The user reviews this data and confirms before SLAs, shifts and tasks are saved to the database.
    /// </summary>
    public class ContractScanResult
    {
        /// <summary>ID of the contract that was scanned.</summary>
        public Guid ContractId { get; set; }

        /// <summary>Name of the contract.</summary>
        public string ContractName { get; set; } = null!;

        /// <summary>
        /// Raw text content extracted by Azure Document Intelligence.
        /// Available for the frontend to display a read-only preview.
        /// </summary>
        public string? ExtractedRawText { get; set; }

        /// <summary>SLA sections detected by Gemini AI, grouped by location/work area.</summary>
        public List<ExtractedSlaDto> Slas { get; set; } = [];

        /// <summary>
        /// Any warnings produced during extraction (e.g. sections Gemini was uncertain about).
        /// </summary>
        public List<string> Warnings { get; set; } = [];
    }
}
