namespace CleanOpsAi.Modules.UserAccess.Application.Users.RegisterUserWithEmail
{
	public class RegisterUserWithEmailCommandHandler : ICommandHandler<RegisterUserWithEmailCommand, RegisterUserResult>
	{
		private readonly IAuth0Service _auth0Service;

		public RegisterUserWithEmailCommandHandler(IAuth0Service auth0Service)
		{
			_auth0Service = auth0Service;
		}

		public Task<RegisterUserResult> Handle(RegisterUserWithEmailCommand request, CancellationToken cancellationToken)
		{
			throw new NotImplementedException();
		}
	}
}
