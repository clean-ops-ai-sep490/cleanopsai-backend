namespace CleanOpsAi.BuildingBlocks.Application.Interfaces
{
    public interface IEmailService
    {
        Task SendEmailAsync(string toEmail, string subject, string body);
        Task<string> LoadTemplate(string templateName);
    }
}
