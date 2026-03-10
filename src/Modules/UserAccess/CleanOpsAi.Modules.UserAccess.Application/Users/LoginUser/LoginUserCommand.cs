namespace CleanOpsAi.Modules.UserAccess.Application.Users.LoginUser
{
	public class LoginUserCommand : CommandBase<AuthTokenResult>
	{
		public string Email { get; }
		public string Password { get; }

		public LoginUserCommand(string email, string password)
		{
			Email = email;
			Password = password;
		}
	}

	public class AuthTokenResult
	{
		public string AccessToken { get; set; }
		public string RefreshToken { get; set; }
		public int ExpiresIn { get; set; }
	}
}
