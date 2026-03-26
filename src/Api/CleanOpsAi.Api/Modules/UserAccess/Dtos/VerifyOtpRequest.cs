using System.ComponentModel.DataAnnotations;

namespace CleanOpsAi.Api.Modules.UserAccess.Dtos
{
    public class VerifyOtpRequest
    {
        [Required]
        public string Email { get; set; }
        [Required]
        public string OtpCode { get; set; }
    }
}
