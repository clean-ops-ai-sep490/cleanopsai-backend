using CleanOpsAi.Modules.UserAccess.Application.Users.LoginUser;
using CleanOpsAi.Modules.UserAccess.Application.Users.RegisterUserWithEmail;
using CleanOpsAi.Modules.UserAccess.Domain;

namespace CleanOpsAi.Modules.UserAccess.Application.Contracts
{
	public interface IAuthService
	{
		Task<RegisterUserResult> Register(string email, string password, string fullName, UserRole role);
        Task<AuthTokenResult> Login(string email, string password);
		Task<AuthTokenResult> RefreshToken(string refreshToken);
	}
}
