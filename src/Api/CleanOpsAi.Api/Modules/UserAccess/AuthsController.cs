using CleanOpsAi.Api.Modules.UserAccess.Dtos;
using CleanOpsAi.Modules.UserAccess.Application.Contracts;
using CleanOpsAi.Modules.UserAccess.Application.Users.RegisterUserWithEmail;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;

namespace CleanOpsAi.Api.Modules.UserAccess
{
	[Route("api/[controller]")]
	[ApiController]
	public class AuthsController : ControllerBase
	{
		private readonly IUserAccessModule _userAccessModule;

		public AuthsController(IUserAccessModule userAccessModule)
		{
			_userAccessModule = userAccessModule;
		}

		[HttpPost("register")]
		public async Task<IActionResult> Register(RegisterNewUserRequest request)
		{
			var result = await _userAccessModule.ExecuteCommandAsync( new RegisterUserWithEmailCommand(request.Email, request.Password, request.FullName));

			return Ok(result);
		}
	}
}
