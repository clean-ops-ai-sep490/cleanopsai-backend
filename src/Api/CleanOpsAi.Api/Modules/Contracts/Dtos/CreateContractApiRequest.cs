namespace CleanOpsAi.Api.Modules.Contracts.Dtos
{
    public class CreateContractApiRequest
    {
        public string Name { get; set; } = null!;
        public Guid ClientId { get; set; }
        public IFormFile? File { get; set; }
    }
}
