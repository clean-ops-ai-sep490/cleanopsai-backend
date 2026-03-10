using CleanOpsAi.Api.Modules.UserAccess.Dtos;
using CleanOpsAi.Modules.UserAccess.Application.Contracts;
using CleanOpsAi.Modules.UserAccess.Application.Users.LoginUser;
using CleanOpsAi.Modules.UserAccess.Application.Users.RefreshToken;
using CleanOpsAi.Modules.UserAccess.Application.Users.RegisterUserWithEmail;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
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
			Tags = new[] { "Auth" })]
		[SwaggerResponse(StatusCodes.Status200OK, "User registered successfully", typeof(RegisterUserResult))]
		[SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid request data")]
		public async Task<IActionResult> Register(RegisterNewUserRequest request)
		{
			try
			{
				var result = await _userAccessModule.ExecuteCommandAsync(new RegisterUserWithEmailCommand(request.Email, request.Password, request.FullName));
				return Ok(result);
			}
			catch (InvalidOperationException ex)
			{
				return BadRequest(new { message = ex.Message });
			}
		}

		[HttpPost("login")]
		[Consumes("application/json")]
		[SwaggerOperation(
			Summary = "Login user",
			Description = "Authenticates a user and returns JWT tokens.",
			Tags = new[] { "Auth" })]
		[SwaggerResponse(StatusCodes.Status200OK, "Login successful", typeof(AuthTokenResult))]
		[SwaggerResponse(StatusCodes.Status401Unauthorized, "Invalid credentials")]
		public async Task<IActionResult> Login(LoginRequest request)
		{
			try
			{
				var result = await _userAccessModule.ExecuteCommandAsync(new LoginUserCommand(request.Email, request.Password));
				return Ok(result);
			}
			catch (UnauthorizedAccessException)
			{
				return Unauthorized(new { message = "Invalid email or password" });
			}
		}

		[HttpPost("refresh")]
		[Consumes("application/json")]
		[SwaggerOperation(
			Summary = "Refresh access token",
			Description = "Uses a refresh token to obtain a new access token.",
			Tags = new[] { "Auth" })]
		[SwaggerResponse(StatusCodes.Status200OK, "Token refreshed", typeof(AuthTokenResult))]
		[SwaggerResponse(StatusCodes.Status401Unauthorized, "Invalid refresh token")]
		public async Task<IActionResult> Refresh(RefreshTokenRequest request)
		{
			try
			{
				var result = await _userAccessModule.ExecuteCommandAsync(new RefreshTokenCommand(request.RefreshToken));
				return Ok(result);
			}
			catch (UnauthorizedAccessException)
			{
				return Unauthorized(new { message = "Invalid or expired refresh token" });
			}
		}

		[HttpGet("me")]
		[Authorize]
		[SwaggerOperation(
			Summary = "Get current user",
			Description = "Returns the authenticated user's claims.",
			Tags = new[] { "Auth" })]
		[SwaggerResponse(StatusCodes.Status200OK, "User info returned")]
		[SwaggerResponse(StatusCodes.Status401Unauthorized, "Not authenticated")]
		public IActionResult Me()
		{
			var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
				?? User.FindFirst("sub")?.Value;
			var email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value
				?? User.FindFirst("email")?.Value;
			var fullName = User.FindFirst("fullName")?.Value;
			var role = User.FindFirst("role")?.Value;
			return Ok(new { userId, email, fullName, role });
		}
	}
}
