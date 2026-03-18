using CleanOpsAi.BuildingBlocks.Application.Interfaces;
using CleanOpsAi.BuildingBlocks.Infrastructure.Configs;
using CleanOpsAi.BuildingBlocks.Infrastructure.Events;
using CleanOpsAi.BuildingBlocks.Infrastructure.Services;
using CleanOpsAi.Modules.UserAccess.Application.Contracts;
using CleanOpsAi.Modules.UserAccess.Application.Users.LoginUser;
using CleanOpsAi.Modules.UserAccess.Application.Users.RegisterUserWithEmail;
using CleanOpsAi.Modules.UserAccess.Domain;
using MassTransit;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System.Net;

namespace CleanOpsAi.Modules.UserAccess.Application.Services
{
	public class AuthService : IAuthService
	{
		private readonly IAuthRepository _authRepository;
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailService _emailService;
        private readonly FrontendSettings _frontendSettings;

        public AuthService(IAuthRepository authRepository, IPublishEndpoint publishEndpoint, UserManager<ApplicationUser> userManager, IEmailService emailService, IOptions<FrontendSettings> frontendOptions)
		{
			_authRepository = authRepository;
			_publishEndpoint = publishEndpoint;
            _userManager = userManager;
            _emailService = emailService;
            _frontendSettings = frontendOptions.Value;
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

        // Forgot password implementation: generate reset token and send email with reset link
        public async Task ForgotPassword(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);

            if (user == null)
                return; // tránh lộ email tồn tại

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            var encodedToken = WebUtility.UrlEncode(token);

            // lấy path từ appsettings
            var resetLink =
                $"{_frontendSettings.BaseUrl}{_frontendSettings.ResetPasswordPath}?email={email}&token={encodedToken}";

            var template = await _emailService.LoadTemplate("reset-password.html");

            var body = template.Replace("{{RESET_LINK}}", resetLink);

            await _emailService.SendEmailAsync(
                email,
                "Reset Password",
                body
            );
        }

        // Reset password implementation: validate token and reset password
        public async Task ResetPassword(string email, string token, string newPassword)
        {
            var user = await _userManager.FindByEmailAsync(email)
                ?? throw new Exception("User not found");

            var decodedToken = WebUtility.UrlDecode(token);

            var result = await _userManager.ResetPasswordAsync(
                user,
                decodedToken,
                newPassword
            );

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new Exception(errors);
            }
        }

    }
}
