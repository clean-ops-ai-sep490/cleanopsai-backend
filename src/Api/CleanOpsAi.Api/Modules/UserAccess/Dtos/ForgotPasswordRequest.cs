using System.ComponentModel.DataAnnotations;

namespace CleanOpsAi.Api.Modules.UserAccess.Dtos
{
    public class ForgotPasswordRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
    }
}
