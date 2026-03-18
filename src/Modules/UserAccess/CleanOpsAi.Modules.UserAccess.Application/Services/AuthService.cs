using CleanOpsAi.BuildingBlocks.Infrastructure.Events;
using CleanOpsAi.Modules.UserAccess.Application.Contracts;
using CleanOpsAi.Modules.UserAccess.Application.Users.LoginUser;
using CleanOpsAi.Modules.UserAccess.Application.Users.RegisterUserWithEmail;
using CleanOpsAi.Modules.UserAccess.Domain;
using MassTransit;

namespace CleanOpsAi.Modules.UserAccess.Application.Services
{
	public class AuthService : IAuthService
	{
		private readonly IAuthRepository _authRepository;
        private readonly IPublishEndpoint _publishEndpoint;

        public AuthService(IAuthRepository authRepository, IPublishEndpoint publishEndpoint)
		{
			_authRepository = authRepository;
			_publishEndpoint = publishEndpoint;
        }

        public async Task<RegisterUserResult> Register(
            string email,
            string password,
            string fullName,
        UserRole role)
        {
            var result = await _authRepository.Register(email, password, fullName, role);

            await _publishEndpoint.Publish(
                new UserRegisteredIntegrationEvent
                {
                    UserId = result.UserId.ToString(),
                    Role = role.ToString(),
                    FullName = fullName,
                    AvatarUrl = null
                });

            return result;
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
