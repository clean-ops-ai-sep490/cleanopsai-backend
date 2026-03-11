using CleanOpsAi.Modules.UserAccess.Application.Contracts;
using CleanOpsAi.Modules.UserAccess.Application.Users.LoginUser;
using CleanOpsAi.Modules.UserAccess.Application.Users.RegisterUserWithEmail;

namespace CleanOpsAi.Modules.UserAccess.Application.Services
{
	public class AuthService : IAuthService
	{
		private readonly IAuthRepository _authRepository;

		public AuthService(IAuthRepository authRepository)
		{
			_authRepository = authRepository;
		}

		public Task<RegisterUserResult> Register(string email, string password, string fullName)
		{
			return _authRepository.Register(email, password, fullName);
		}

		public Task<AuthTokenResult> Login(string email, string password)
		{
			return _authRepository.Login(email, password);
		}

		public Task<AuthTokenResult> RefreshToken(string refreshToken)
		{
			return _authRepository.RefreshToken(refreshToken);
		}
	}
}
