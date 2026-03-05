namespace CleanOpsAi.Modules.UserAccess.Application.Users.RegisterUserWithEmail
{
	public class RegisterUserWithEmailCommand : CommandBase<RegisterUserResult>
	{
		public string Email { get; }
		public string Password { get; }
		public string FullName { get; }
		public string Connection { get; }
	}

	public class RegisterUserResult
	{
		public string Auth0UserId { get; set; }
		public string Email { get; set; }
	}
}
