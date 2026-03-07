using CleanOpsAi.Api.Modules.UserAccess.Dtos;
using CleanOpsAi.Modules.UserAccess.Application.Contracts;
using CleanOpsAi.Modules.UserAccess.Application.Users.RegisterUserWithEmail;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

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
		[Consumes("application/json")]
		[SwaggerOperation(
	Summary = "Register new user",
	Description = "Registers a new user using email and password.",
	Tags = new[] { "Users" })]
		[SwaggerResponse(StatusCodes.Status200OK, "User registered successfully", typeof(RegisterNewUserRequest))]
		[SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid request data")]
		public async Task<IActionResult> Register(RegisterNewUserRequest request)
		{
			var result = await _userAccessModule.ExecuteCommandAsync(new RegisterUserWithEmailCommand(request.Email, request.Password, request.FullName));

			return Ok(result);
		}
	}
}
