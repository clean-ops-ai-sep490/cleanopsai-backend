namespace CleanOpsAi.Api.Modules.Workforce.Dtos
{
    public class UpdateWorkerApiRequest
    {
        public string? FullName { get; set; }
        public string? Address { get; set; }        // FE chỉ truyền địa chỉ
        public IFormFile? Avatar { get; set; }
    }
}
