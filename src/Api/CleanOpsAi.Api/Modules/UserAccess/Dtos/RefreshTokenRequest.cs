using System.ComponentModel.DataAnnotations;

namespace CleanOpsAi.Api.Modules.UserAccess.Dtos
{
	public class RefreshTokenRequest
	{
		[Required]
		public string RefreshToken { get; set; } = string.Empty;
	}
}
