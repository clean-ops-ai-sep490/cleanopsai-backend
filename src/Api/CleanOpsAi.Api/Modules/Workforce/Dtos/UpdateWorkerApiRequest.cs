namespace CleanOpsAi.Api.Modules.Workforce.Dtos
{
    public class UpdateWorkerApiRequest
    {
        public string? DisplayAddress { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public IFormFile? Avatar { get; set; }
    }
}
