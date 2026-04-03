namespace CleanOpsAi.Api.Modules.ClientManagement.Dtos
{
    public class UpdateContractApiRequest
    {
        public string? Name { get; set; }
        public DateTime ContractStartDate { get; set; }
        public DateTime ContractEndDate { get; set; }
        public IFormFile? File { get; set; }
    }
}
