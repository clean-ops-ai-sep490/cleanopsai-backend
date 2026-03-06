namespace CleanOpsAi.Modules.UserAccess.Application.Users.RegisterUserWithEmail
{
	public class RegisterUserWithEmailCommandHandler : ICommandHandler<RegisterUserWithEmailCommand, RegisterUserResult>
	{
		private readonly IAuth0Service _auth0Service;

		public RegisterUserWithEmailCommandHandler(IAuth0Service auth0Service)
		{
			_auth0Service = auth0Service;
		}

		public async Task<RegisterUserResult> Handle(RegisterUserWithEmailCommand request, CancellationToken cancellationToken)
		{
			var result = await _auth0Service.Register(request.Email, request.Password, request.FullName);

			return result;
		}
	}
}
