using CleanOpsAi.Modules.UserAccess.Application.Users.LoginUser;
using CleanOpsAi.Modules.UserAccess.Application.Users.RegisterUserWithEmail;

namespace CleanOpsAi.Modules.UserAccess.Application.Contracts
{
	public interface IAuthService
	{
		Task<RegisterUserResult> Register(string email, string password, string fullName);
		Task<AuthTokenResult> Login(string email, string password);
		Task<AuthTokenResult> RefreshToken(string refreshToken);
	}
}
