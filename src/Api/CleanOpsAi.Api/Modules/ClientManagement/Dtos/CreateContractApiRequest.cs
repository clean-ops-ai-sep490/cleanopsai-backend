namespace CleanOpsAi.Api.Modules.ClientManagement.Dtos
{
    public class CreateContractApiRequest
    {
        public string Name { get; set; } = null!;
        public DateTime ContractStartDate { get; set; }
        public DateTime ContractEndDate { get; set; }
        public Guid ClientId { get; set; }
        public IFormFile? File { get; set; }
    }
}
