using CleanOpsAi.Modules.ClientManagement.Application.Dtos.ContractScan;

namespace CleanOpsAi.Modules.ClientManagement.Application.Interfaces
{
    /// <summary>
    /// Scans a contract document (DOCX/PDF stored in Azure Blob Storage) with
    /// Azure Document Intelligence and then uses Google Gemini to parse the
    /// extracted text into structured SLA / shift / task data for user review.
    /// </summary>
    public interface IContractScanService
    {
        /// <summary>
        /// Downloads the contract file identified by <paramref name="contractId"/>,
        /// extracts its full text via Document Intelligence, and then sends the text
        /// to Gemini so it can identify SLA sections, shifts and recurring tasks.
        /// </summary>
        /// <param name="contractId">Primary key of the <see cref="Domain.Entities.Contract"/> to process.</param>
        /// <param name="cancellationToken">Propagates cancellation.</param>
        /// <returns>
        /// A <see cref="ContractScanResult"/> containing the AI-generated suggestion
        /// that the user can review and confirm before it is persisted.
        /// </returns>
        Task<ContractScanResult> ScanContractAsync(Guid contractId, CancellationToken cancellationToken = default);
    }
}
