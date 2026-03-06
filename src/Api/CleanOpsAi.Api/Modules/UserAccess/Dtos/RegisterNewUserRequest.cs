namespace CleanOpsAi.Api.Modules.UserAccess.Dtos
{
	public class RegisterNewUserRequest
	{
		public string Email { get; set; }
		public string Password { get; set; }
		public string FullName { get; set; }
	}
}
