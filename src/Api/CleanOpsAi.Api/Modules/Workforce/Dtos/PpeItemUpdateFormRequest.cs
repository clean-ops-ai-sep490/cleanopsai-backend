namespace CleanOpsAi.Api.Modules.Workforce.Dtos
{
    public class PpeItemUpdateFormRequest
    {
        public string? ActionKey { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }

        public IFormFile? ImageFile { get; set; }
    }
}
