namespace CleanOpsAi.Modules.UserAccess.Application.Users.RegisterUserWithEmail
{
	public class RegisterUserWithEmailCommandHandler : ICommandHandler<RegisterUserWithEmailCommand, RegisterUserResult>
	{
		private readonly IAuthService _authService;

		public RegisterUserWithEmailCommandHandler(IAuthService authService)
		{
			_authService = authService;
		}

		public async Task<RegisterUserResult> Handle(RegisterUserWithEmailCommand request, CancellationToken cancellationToken)
		{
			return await _authService.Register(request.Email, request.Password, request.FullName);
		}
	}
}
