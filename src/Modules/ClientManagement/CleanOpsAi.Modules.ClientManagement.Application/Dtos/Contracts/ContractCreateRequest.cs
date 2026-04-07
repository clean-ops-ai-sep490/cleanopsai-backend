namespace CleanOpsAi.Modules.ClientManagement.Application.Dtos.Contracts
{
    public class ContractCreateRequest
    {
        public string Name { get; set; } = null!;
		public Guid ClientId { get; set; }
        public DateTime ContractStartDate { get; set; }
        public DateTime ContractEndDate { get; set; }
        public Stream? FileStream { get; set; }
        public string? FileName { get; set; }
    }
}
