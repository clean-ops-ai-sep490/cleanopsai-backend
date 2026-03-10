namespace CleanOpsAi.Modules.UserAccess.Application.Users.LoginUser
{
	public class LoginUserCommandHandler : ICommandHandler<LoginUserCommand, AuthTokenResult>
	{
		private readonly IAuthService _authService;

		public LoginUserCommandHandler(IAuthService authService)
		{
			_authService = authService;
		}

		public async Task<AuthTokenResult> Handle(LoginUserCommand request, CancellationToken cancellationToken)
		{
			return await _authService.Login(request.Email, request.Password);
		}
	}
}
