using System.ComponentModel.DataAnnotations;

namespace CleanOpsAi.Api.Modules.UserAccess.Dtos
{
    public class ResetPasswordRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = default!;
        [Required]
        public string Token { get; set; } = default!;
        [Required]
        public string NewPassword { get; set; } = default!;
    }
}
