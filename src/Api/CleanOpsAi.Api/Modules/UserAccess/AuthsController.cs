using CleanOpsAi.Api.Modules.UserAccess.Dtos;
using CleanOpsAi.BuildingBlocks.Application.Pagination;
using CleanOpsAi.Modules.UserAccess.Application.Contracts;
using CleanOpsAi.Modules.UserAccess.Application.Users.LoginUser;
using CleanOpsAi.Modules.UserAccess.Application.Users.RegisterUserWithEmail;
using CleanOpsAi.Modules.UserAccess.Domain;
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
		private readonly IAuthService _authService;

		public AuthsController(IAuthService authService)
		{
			_authService = authService;
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
				var result = await _authService.Register(request.Email, request.Password, request.FullName, request.Role);
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
				var result = await _authService.Login(request.Email, request.Password);
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
				var result = await _authService.RefreshToken(request.RefreshToken);
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
			var userId = User.FindFirst("sub")?.Value;
			var email = User.FindFirst("email")?.Value;
			var fullName = User.FindFirst("fullName")?.Value;
			var role = User.FindFirst("role")?.Value;
			return Ok(new { userId, email, fullName, role });
		}

        [HttpPost("forgot-password")]
        [SwaggerOperation(
			Summary = "Forgot password",
			Description = "Send reset password email",
			Tags = new[] { "Auth" })]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest forgotPasswordRequest)
        {
            await _authService.ForgotPassword(forgotPasswordRequest.Email);
            return Ok("Đã gửi email reset");
        }

        [HttpPost("verify-otp")]
        [SwaggerOperation(
            Summary = "Verify Otp",
            Description = "Response token to reset password",
            Tags = new[] { "Auth" })]
        public async Task<IActionResult> VerifyOtp(VerifyOtpRequest request)
        {
            var token = await _authService.VerifyOtp(request.Email, request.OtpCode);

            return Ok(new { token });
        }

        [HttpPost("reset-password")]
        [SwaggerOperation(
			Summary = "Reset password",
			Description = "Reset password using token",
			Tags = new[] { "Auth" })]
        public async Task<IActionResult> ResetPassword(ResetPasswordRequest request)
        {
            await _authService.ResetPassword(request.Email, request.Token, request.NewPassword);
            return Ok("Reset password thành công");
        }

        [HttpGet("supervisors")]
        //[Authorize(Roles = "Admin,Manager")]
        [SwaggerOperation(
			Summary = "Get supervisors paging",
			Description = "Get list of supervisors with pagination and optional keyword search (email or name)",
			Tags = new[] { "Auth" })]
        [SwaggerResponse(StatusCodes.Status200OK, "Success")]
        public async Task<IActionResult> GetSupervisors(
			[FromQuery] string? keyword,
			[FromQuery] int pageNumber = 1,
			[FromQuery] int pageSize = 10,
			CancellationToken ct = default)
        {
            var request = new PaginationRequest
            {
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            var result = await _authService.GetSupervisors(keyword, request, ct);

            return Ok(result);
        }

        [HttpGet("users")]
        //[Authorize(Roles = "Admin")]
        [SwaggerOperation(
			Summary = "Get users paging",
			Description = "Get list of users with pagination, optional keyword and role filter",
			Tags = new[] { "User Management" })]
        public async Task<IActionResult> GetUsers(
			[FromQuery] string? keyword,
			[FromQuery] UserRole? role,
			[FromQuery] int pageNumber = 1,
			[FromQuery] int pageSize = 10,
			CancellationToken ct = default)
        {
            var request = new PaginationRequest
            {
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            var result = await _authService.GetUsers(keyword, role, request, ct);

            return Ok(result);
        }

        [HttpGet("users/{userId}")]
        //[Authorize(Roles = "Admin")]
        [SwaggerOperation(
			Summary = "Get user by id",
			Description = "Get detail of a user",
			Tags = new[] { "User Management" })]
        public async Task<IActionResult> GetUserById(Guid userId)
        {
            var result = await _authService.GetUserById(userId);
            return Ok(result);
        }

        [HttpPut("users/{userId}")]
        //[Authorize(Roles = "Admin")]
        [SwaggerOperation(
			Summary = "Update user",
			Description = "Update full name and role of user",
			Tags = new[] { "User Management" })]
        public async Task<IActionResult> UpdateUser(Guid userId, [FromBody] UpdateUserRequest request)
        {
            var result = await _authService.UpdateUser(userId, request.FullName, request.Role);
            return Ok(result);
        }

        [HttpDelete("users/{userId}")]
        //[Authorize(Roles = "Admin")]
        [SwaggerOperation(
			Summary = "Delete user",
			Description = "Soft delete user (lock permanently)",
			Tags = new[] { "User Management" })]
        public async Task<IActionResult> DeleteUser(Guid userId)
        {
            var result = await _authService.DeleteUser(userId);
            return Ok(result);
        }

        [HttpPost("users/{userId}/unlock")]
        //[Authorize(Roles = "Admin")]
        [SwaggerOperation(
			Summary = "Unlock user",
			Description = "Unlock user account",
			Tags = new[] { "User Management" })]
        public async Task<IActionResult> UnlockUser(Guid userId)
        {
            var result = await _authService.UnlockUser(userId);
            return Ok(result);
        }

    }
}
