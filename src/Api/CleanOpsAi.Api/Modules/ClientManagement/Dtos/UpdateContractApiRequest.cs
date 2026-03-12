namespace CleanOpsAi.Api.Modules.ClientManagement.Dtos
{
    public class UpdateContractApiRequest
    {
        public string? Name { get; set; }
        public IFormFile? File { get; set; }
    }
}
