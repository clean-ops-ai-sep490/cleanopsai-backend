using CleanOpsAi.Modules.UserAccess.Application.Users.RegisterUserWithEmail;

namespace CleanOpsAi.Modules.UserAccess.Application.Contracts
{
	public interface IAuth0Service
	{
		Task<RegisterUserResult> Register(string email, string password, string fullName);
	}
}
