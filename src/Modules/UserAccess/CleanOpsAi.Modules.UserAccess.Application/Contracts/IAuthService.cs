using CleanOpsAi.BuildingBlocks.Application.Pagination;
using CleanOpsAi.Modules.UserAccess.Application.DTOs.Response;
using CleanOpsAi.Modules.UserAccess.Application.Users.LoginUser;
using CleanOpsAi.Modules.UserAccess.Application.Users.RegisterUserWithEmail;
using CleanOpsAi.Modules.UserAccess.Domain;

namespace CleanOpsAi.Modules.UserAccess.Application.Contracts
{
	public interface IAuthService
	{
		Task<RegisterUserResult> Register(string email, string password, string fullName, UserRole role);
        Task<AuthTokenResult> Login(string email, string password);
		Task<AuthTokenResult> RefreshToken(string refreshToken);
		Task ForgotPassword(string email);
		Task ResetPassword(string email, string token, string newPassword);
        Task<string> VerifyOtp(string email, string otp);
        Task<PaginatedResult<UserDto>> GetSupervisors(string? keyword, PaginationRequest request, CancellationToken ct = default);
        Task<PaginatedResult<UserDto>> GetUsers(string? keyword, UserRole? role, PaginationRequest request, CancellationToken ct = default);
        Task<UserDto> GetUserById(Guid userId);
        Task<UserDto> UpdateUser(Guid userId, string fullName, UserRole role);
        Task<UserDto> DeleteUser(Guid userId);
        Task<UserDto> LockUser(Guid userId, int days);
        Task<UserDto> UnlockUser(Guid userId);
    }
}
