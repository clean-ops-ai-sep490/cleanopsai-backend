using CleanOpsAi.Modules.UserAccess.Application.Users.LoginUser;

namespace CleanOpsAi.Modules.UserAccess.Application.Users.RefreshToken
{
	public class RefreshTokenCommand
	{
		public string Token { get; }

		public RefreshTokenCommand(string token)
		{
			Token = token;
		}
	}
}
