namespace CleanOpsAi.Api.Modules.Workforce.Dtos
{
    public class PpeItemCreateFormRequest
    {
        public string ActionKey { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;

        public IFormFile? ImageFile { get; set; }
    }
}
