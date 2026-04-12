using CleanOpsAi.BuildingBlocks.Application.Interfaces;
using CleanOpsAi.BuildingBlocks.Application.Pagination;
using CleanOpsAi.BuildingBlocks.Infrastructure.Configs;
using CleanOpsAi.BuildingBlocks.Infrastructure.Events;
using CleanOpsAi.Modules.UserAccess.Application.Contracts;
using CleanOpsAi.Modules.UserAccess.Application.Services;
using CleanOpsAi.Modules.UserAccess.Application.Users.LoginUser;
using CleanOpsAi.Modules.UserAccess.Application.Users.RegisterUserWithEmail;
using CleanOpsAi.Modules.UserAccess.Domain;
using MassTransit;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanOpsAi.Modules.UserAccess.UnitTests.Services
{
    public class AuthServiceTests
    {
        private readonly IAuthRepository _authRepo;
        private readonly IPublishEndpoint _publish;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailService _emailService;
        private readonly IPasswordResetOtpRepository _otpRepo;
        private readonly IOptions<FrontendSettings> _frontendOptions;

        private readonly AuthService _service;

        public AuthServiceTests()
        {
            _authRepo = Substitute.For<IAuthRepository>();
            _publish = Substitute.For<IPublishEndpoint>();
            _emailService = Substitute.For<IEmailService>();
            _otpRepo = Substitute.For<IPasswordResetOtpRepository>();

            // mock UserManager (cái này hơi đặc biệt)
            var store = Substitute.For<IUserStore<ApplicationUser>>();
            _userManager = Substitute.For<UserManager<ApplicationUser>>(
                store, null, null, null, null, null, null, null, null
            );

            _frontendOptions = Options.Create(new FrontendSettings
            {
                BaseUrl = "http://localhost",
                ResetPasswordPath = "/reset"
            });

            _service = new AuthService(
                _authRepo,
                _publish,
                _userManager,
                _emailService,
                _frontendOptions,
                _otpRepo
            );
        }

        // ================================
        // REGISTER
        // ================================
        [Fact]
        public async Task Register_ShouldPublishEvent()
        {
            var email = "test@gmail.com";
            var password = "123456";
            var fullName = "Test";
            var role = UserRole.Supervisor;

            var result = new RegisterUserResult
            {
                UserId = Guid.NewGuid(),
                Email = "test@gmail.com"
            };

            _authRepo.Register(email, password, fullName, role)
                     .Returns(result);

            var response = await _service.Register(email, password, fullName, role);

            Assert.NotNull(response);
            Assert.Equal(result.UserId, response.UserId);

            await _publish.Received(1).Publish(Arg.Any<UserRegisteredIntegrationEvent>());
        }

        // ================================
        // LOGIN
        // ================================
        [Fact]
        public async Task Login_ShouldReturnToken()
        {
            var expected = new AuthTokenResult
            {
                AccessToken = "access",
                RefreshToken = "refresh",
                ExpiresIn = 3600
            };

            _authRepo.Login("a@gmail.com", "123")
                     .Returns(expected);

            var result = await _service.Login("a@gmail.com", "123");

            Assert.Equal(expected, result);
        }

        // ================================
        // FORGOT PASSWORD
        // ================================
        [Fact]
        public async Task ForgotPassword_ShouldSendEmail()
        {
            var email = "test@gmail.com";

            var user = new ApplicationUser
            {
                Email = email,
                UserName = "TestUser"
            };

            _userManager.FindByEmailAsync(email).Returns(user);

            _emailService.LoadTemplate("reset-password.html")
                         .Returns("Hello {{FULL_NAME}} - {{OTP_CODE}}");

            await _service.ForgotPassword(email);

            await _otpRepo.Received(1).AddAsync(Arg.Any<PasswordResetOtp>());
            await _otpRepo.Received(1).SaveChangesAsync();

            await _emailService.Received(1).SendEmailAsync(
                email,
                Arg.Any<string>(),
                Arg.Any<string>()
            );
        }

        // ================================
        // VERIFY OTP
        // ================================
        [Fact]
        public async Task VerifyOtp_ShouldReturnToken()
        {
            var email = "test@gmail.com";
            var otp = "123456";

            var record = new PasswordResetOtp
            {
                Email = email,
                OtpCode = otp,
                IsUsed = false,
                ExpiredAt = DateTime.UtcNow.AddMinutes(5)
            };

            var user = new ApplicationUser { Email = email };

            _otpRepo.GetValidOtpAsync(email, otp).Returns(record);
            _userManager.FindByEmailAsync(email).Returns(user);
            _userManager.GeneratePasswordResetTokenAsync(user)
                        .Returns("reset-token");

            var result = await _service.VerifyOtp(email, otp);

            Assert.NotNull(result);
            Assert.True(record.IsUsed);

            await _otpRepo.Received(1).SaveChangesAsync();
        }

        // ================================
        // RESET PASSWORD
        // ================================
        [Fact]
        public async Task ResetPassword_ShouldSucceed()
        {
            var email = "test@gmail.com";
            var token = "encoded-token";
            var user = new ApplicationUser { Email = email };

            _userManager.FindByEmailAsync(email).Returns(user);
            _userManager.ResetPasswordAsync(user, Arg.Any<string>(), "newpass")
                        .Returns(IdentityResult.Success);

            await _service.ResetPassword(email, token, "newpass");

            await _userManager.Received(1).ResetPasswordAsync(
                user,
                Arg.Any<string>(),
                "newpass"
            );
        }

        // ================================
        // GET USERS
        // ================================
        [Fact]
        public async Task GetUsers_ShouldReturnPagedResult()
        {
            var users = new List<ApplicationUser>
            {
                new ApplicationUser
                {
                    Id = Guid.NewGuid(),
                    Email = "a@gmail.com",
                    FullName = "A",
                    Role = UserRole.Supervisor
                }
            };

            var paged = new PaginatedResult<ApplicationUser>(
                1, 10, 1, users
            );

            _authRepo.GetUsersPagingAsync(null, null, Arg.Any<PaginationRequest>(), default)
                     .Returns(paged);

            var result = await _service.GetUsers(null, null, new PaginationRequest());

            Assert.NotNull(result);
            Assert.Single(result.Content);
        }

        // ================================
        // LOCK USER
        // ================================
        [Fact]
        public async Task LockUser_ShouldReturnLockedStatus()
        {
            var userId = Guid.NewGuid();

            var user = new ApplicationUser
            {
                Id = userId,
                Email = "test@gmail.com",
                LockoutEnd = DateTimeOffset.UtcNow.AddDays(1)
            };

            _userManager.FindByIdAsync(userId.ToString())
                        .Returns(user);

            var result = await _service.LockUser(userId, 1);

            Assert.Equal("Locked", result.Status);
        }
    }
}
