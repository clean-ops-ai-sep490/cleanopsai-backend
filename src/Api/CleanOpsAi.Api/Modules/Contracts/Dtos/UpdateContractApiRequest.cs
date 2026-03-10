namespace CleanOpsAi.Api.Modules.Contracts.Dtos
{
    public class UpdateContractApiRequest
    {
        public string? Name { get; set; }
        public IFormFile? File { get; set; }
    }
}
