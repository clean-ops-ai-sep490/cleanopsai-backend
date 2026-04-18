using CleanOpsAi.BuildingBlocks.Application.Interfaces;
using CleanOpsAi.BuildingBlocks.Application.Pagination;
using CleanOpsAi.BuildingBlocks.Infrastructure.Configs;
using CleanOpsAi.BuildingBlocks.Infrastructure.Events;
using CleanOpsAi.BuildingBlocks.Infrastructure.Services;
using CleanOpsAi.Modules.UserAccess.Application.Contracts;
using CleanOpsAi.Modules.UserAccess.Application.DTOs.Response;
using CleanOpsAi.Modules.UserAccess.Application.Users.LoginUser;
using CleanOpsAi.Modules.UserAccess.Application.Users.RegisterUserWithEmail;
using CleanOpsAi.Modules.UserAccess.Domain;
using MassTransit;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
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
        private readonly IPasswordResetOtpRepository _otpRepo;

        public AuthService(IAuthRepository authRepository, IPublishEndpoint publishEndpoint, UserManager<ApplicationUser> userManager, IEmailService emailService, IOptions<FrontendSettings> frontendOptions, IPasswordResetOtpRepository otpRepository)
		{
			_authRepository = authRepository;
			_publishEndpoint = publishEndpoint;
            _userManager = userManager;
            _emailService = emailService;
            _frontendSettings = frontendOptions.Value;
            _otpRepo = otpRepository;
        }

        public async Task<RegisterUserResult> Register(
            string email,
            string password,
            string fullName,
            UserRole role)
        {
            var result = await _authRepository.Register(email, password, fullName, role);

            Console.WriteLine("BEFORE PUBLISH");
            try
            {
                await _publishEndpoint.Publish(
                    new UserRegisteredIntegrationEvent
                    {
                        UserId = result.UserId.ToString(),
                        Role = role.ToString(),
                        FullName = fullName,
                        AvatarUrl = null
                    });
                Console.WriteLine($"PUBLISHED SUCCESSFULLY - Role: {role.ToString()}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"PUBLISH FAILED: {ex.Message}");
                throw;
            }

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

            // generate OTP 6 số
            var otp = new Random().Next(100000, 999999).ToString();

            var otpEntity = new PasswordResetOtp
            {
                Id = Guid.NewGuid(),
                Email = email,
                OtpCode = otp,
                ExpiredAt = DateTime.UtcNow.AddMinutes(5),
                IsUsed = false
            };

            await _otpRepo.AddAsync(otpEntity);
            await _otpRepo.SaveChangesAsync();

            //var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            //var encodedToken = WebUtility.UrlEncode(token);

            //// lấy path từ appsettings
            //var resetLink =
            //    $"{_frontendSettings.BaseUrl}{_frontendSettings.ResetPasswordPath}?email={email}&token={encodedToken}";

            var template = await _emailService.LoadTemplate("reset-password.html");

            var body = template
                .Replace("{{FULL_NAME}}", user.UserName)
                .Replace("{{OTP_CODE}}", otp);


            await _emailService.SendEmailAsync(
                email,
                "Your OTP Code",
                body
            );
        }

        // Verify OTP implementation: validate OTP code for email
        public async Task<string> VerifyOtp(string email, string otp)
        {
            var record = await _otpRepo.GetValidOtpAsync(email, otp);

            if (record == null)
                throw new Exception("OTP không đúng hoặc hết hạn");

            record.IsUsed = true;
            var user = await _userManager.FindByEmailAsync(email);

            var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
            // encode trước khi trả về client
            var encodedToken = WebUtility.UrlEncode(resetToken);
            Console.WriteLine( "ResetToken: ",resetToken);

            await _otpRepo.SaveChangesAsync();

            return encodedToken;
            
        }

        // Reset password implementation: validate token and reset password
        public async Task ResetPassword(string email, string token, string newPassword)
        {
            var user = await _userManager.FindByEmailAsync(email)
                ?? throw new Exception("User not found");

            var decodedToken = WebUtility.UrlDecode(token);
            Console.WriteLine("DecodeToken: ", decodedToken);

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

        public async Task<PaginatedResult<UserDto>> GetSupervisors(
            string? keyword,
            PaginationRequest request,
            CancellationToken ct = default)
        {
            var result = await _authRepository.GetSupervisorsPagingAsync(keyword, request, ct);

            var dtos = result.Content.Select(user => new UserDto
            {
                Id = user.Id,
                Email = user.Email!,
                FullName = user.FullName,
                Role = user.Role
            }).ToList();

            return new PaginatedResult<UserDto>(
                result.PageNumber,
                result.PageSize,
                result.TotalElements,
                dtos
            );
        }

        // Lấy danh sách người dùng với phân trang, có thể lọc theo keyword và role
        public async Task<PaginatedResult<UserDto>> GetUsers(
            string? keyword,
            UserRole? role,
            PaginationRequest request,
            CancellationToken ct = default)
        {
            var result = await _authRepository.GetUsersPagingAsync(keyword, role, request, ct);

            var dtos = result.Content.Select(user => new UserDto
            {
                Id = user.Id,
                Email = user.Email!,
                FullName = user.FullName,
                Role = user.Role,
                Status = (user.LockoutEnd != null && user.LockoutEnd > DateTimeOffset.UtcNow)
                    ? "Locked"
                    : "Active"
            }).ToList();

            return new PaginatedResult<UserDto>(
                result.PageNumber,
                result.PageSize,
                result.TotalElements,
                dtos
            );
        }

        // Lấy thông tin chi tiết người dùng theo Id, bao gồm trạng thái khóa tài khoản (Locked/Active)
        public async Task<UserDto> GetUserById(Guid userId)
        {
            var user = await _authRepository.GetUserByIdAsync(userId);

            return new UserDto
            {
                Id = user.Id,
                Email = user.Email!,
                FullName = user.FullName,
                Role = user.Role,
                Status = (user.LockoutEnd != null && user.LockoutEnd > DateTimeOffset.UtcNow)
                    ? "Locked"
                    : "Active"
            };
        }

        // Cập nhật thông tin người dùng (fullName, role), sau đó trả về thông tin đã cập nhật. Nếu userId không tồn tại, ném lỗi. Trạng thái khóa tài khoản vẫn giữ nguyên sau khi cập nhật.
        public async Task<UserDto> UpdateUser(Guid userId, string fullName, UserRole role)
        {
            await _authRepository.UpdateUserAsync(userId, fullName, role);

            var user = await _userManager.FindByIdAsync(userId.ToString())
                ?? throw new Exception("User not found after update");

            return new UserDto
            {
                Id = user.Id,
                Email = user.Email!,
                FullName = user.FullName,
                Role = user.Role,
                Status = (user.LockoutEnd != null && user.LockoutEnd > DateTimeOffset.UtcNow)
                    ? "Locked"
                    : "Active"
            };
        }

        // Xóa người dùng theo Id, sau đó trả về thông tin người dùng đã bị xóa. Nếu userId không tồn tại, ném lỗi. Trạng thái khóa tài khoản vẫn giữ nguyên sau khi xóa (vì thực tế là xóa mềm - soft delete).
        public async Task<UserDto> DeleteUser(Guid userId)
        {
            await _authRepository.DeleteUserAsync(userId);

            var user = await _userManager.FindByIdAsync(userId.ToString())
                ?? throw new Exception("User not found after delete");

            return new UserDto
            {
                Id = user.Id,
                Email = user.Email!,
                FullName = user.FullName,
                Role = user.Role,
                Status = (user.LockoutEnd != null && user.LockoutEnd > DateTimeOffset.UtcNow)
                    ? "Locked"
                    : "Active"
            };
        }

        // Khóa người dùng theo Id trong một số ngày nhất định, sau đó trả về thông tin người dùng đã bị khóa. Nếu userId không tồn tại, ném lỗi. Trạng thái khóa tài khoản sẽ là "Locked" nếu LockoutEnd trong tương lai, ngược lại là "Active".
        public async Task<UserDto> LockUser(Guid userId, int days)
        {
            await _authRepository.LockUserAsync(userId, days);

            var user = await _userManager.FindByIdAsync(userId.ToString())
                ?? throw new Exception("User not found after lock");

            return new UserDto
            {
                Id = user.Id,
                Email = user.Email!,
                FullName = user.FullName,
                Role = user.Role,
                Status = (user.LockoutEnd != null && user.LockoutEnd > DateTimeOffset.UtcNow)
                    ? "Locked"
                    : "Active"
            };
        }

        // Mở khóa người dùng theo Id, sau đó trả về thông tin người dùng đã được mở khóa. Nếu userId không tồn tại, ném lỗi. Trạng thái khóa tài khoản sẽ là "Locked" nếu LockoutEnd trong tương lai, ngược lại là "Active".
        public async Task<UserDto> UnlockUser(Guid userId)
        {
            await _authRepository.UnlockUserAsync(userId);

            var user = await _userManager.FindByIdAsync(userId.ToString())
                ?? throw new Exception("User not found after unlock");

            return new UserDto
            {
                Id = user.Id,
                Email = user.Email!,
                FullName = user.FullName,
                Role = user.Role,
                Status = (user.LockoutEnd != null && user.LockoutEnd > DateTimeOffset.UtcNow)
                    ? "Locked"
                    : "Active"
            };
        }

    }
}
