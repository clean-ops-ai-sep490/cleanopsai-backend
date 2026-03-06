namespace CleanOpsAi.Modules.UserAccess.Application.Users.RegisterUserWithEmail
{
	public class RegisterUserWithEmailCommand : CommandBase<RegisterUserResult>
	{
		public string Email { get; }
		public string Password { get; }
		public string FullName { get; } 

		public RegisterUserWithEmailCommand(string email, string password, string fullName)
		{
			Email = email;
			Password = password;
			FullName = fullName;
		}
	}

	public class RegisterUserResult
	{
		public string Auth0UserId { get; set; }
		public string Email { get; set; }
	}
}
