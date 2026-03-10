using CleanOpsAi.Modules.UserAccess.Application.Users.LoginUser;

namespace CleanOpsAi.Modules.UserAccess.Application.Users.RefreshToken
{
	public class RefreshTokenCommandHandler : ICommandHandler<RefreshTokenCommand, AuthTokenResult>
	{
		private readonly IAuthService _authService;

		public RefreshTokenCommandHandler(IAuthService authService)
		{
			_authService = authService;
		}

		public async Task<AuthTokenResult> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
		{
			return await _authService.RefreshToken(request.Token);
		}
	}
}
