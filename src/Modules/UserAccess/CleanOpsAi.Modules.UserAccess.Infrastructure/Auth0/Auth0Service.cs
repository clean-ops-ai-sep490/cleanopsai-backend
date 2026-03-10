using CleanOpsAi.Modules.UserAccess.Application.Contracts;
using CleanOpsAi.Modules.UserAccess.Application.Users.LoginUser;
using CleanOpsAi.Modules.UserAccess.Application.Users.RegisterUserWithEmail;
using CleanOpsAi.Modules.UserAccess.Domain;
using CleanOpsAi.Modules.UserAccess.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace CleanOpsAi.Modules.UserAccess.Infrastructure.Auth
{
	public class AuthService : IAuthService
	{
		private readonly UserManager<ApplicationUser> _userManager;
		private readonly UserAccessDbContext _dbContext;
		private readonly IConfiguration _configuration;

		public AuthService(
			UserManager<ApplicationUser> userManager,
			UserAccessDbContext dbContext,
			IConfiguration configuration)
		{
			_userManager = userManager;
			_dbContext = dbContext;
			_configuration = configuration;
		}

		public async Task<RegisterUserResult> Register(string email, string password, string fullName)
		{
			var user = new ApplicationUser
			{
				UserName = email,
				Email = email,
				FullName = fullName,
				Role = UserRole.Worker
			};

			var result = await _userManager.CreateAsync(user, password);

			if (!result.Succeeded)
			{
				var errors = string.Join(", ", result.Errors.Select(e => e.Description));
				throw new InvalidOperationException($"Registration failed: {errors}");
			}

			// Assign Identity role
			await _userManager.AddToRoleAsync(user, nameof(UserRole.Worker));

			return new RegisterUserResult
			{
				UserId = user.Id,
				Email = user.Email
			};
		}

		public async Task<AuthTokenResult> Login(string email, string password)
		{
			var user = await _userManager.FindByEmailAsync(email)
				?? throw new UnauthorizedAccessException("Invalid credentials");

			var passwordValid = await _userManager.CheckPasswordAsync(user, password);
			if (!passwordValid)
			{
				throw new UnauthorizedAccessException("Invalid credentials");
			}

			var roles = await _userManager.GetRolesAsync(user);
			var accessToken = GenerateAccessToken(user, roles);
			var refreshToken = await CreateRefreshTokenAsync(user.Id);

			return new AuthTokenResult
			{
				AccessToken = accessToken,
				RefreshToken = refreshToken,
				ExpiresIn = int.Parse(_configuration["Jwt:ExpiryMinutes"] ?? "60") * 60
			};
		}

		public async Task<AuthTokenResult> RefreshToken(string refreshToken)
		{
			var storedToken = await _dbContext.RefreshTokens
				.Include(rt => rt.User)
				.FirstOrDefaultAsync(rt => rt.Token == refreshToken && !rt.IsRevoked);

			if (storedToken == null || storedToken.ExpiresAt < DateTime.UtcNow)
			{
				throw new UnauthorizedAccessException("Invalid or expired refresh token");
			}

			// Revoke old token
			storedToken.IsRevoked = true;

			// Generate new tokens
			var roles = await _userManager.GetRolesAsync(storedToken.User);
			var accessToken = GenerateAccessToken(storedToken.User, roles);
			var newRefreshToken = await CreateRefreshTokenAsync(storedToken.UserId);

			await _dbContext.SaveChangesAsync();

			return new AuthTokenResult
			{
				AccessToken = accessToken,
				RefreshToken = newRefreshToken,
				ExpiresIn = int.Parse(_configuration["Jwt:ExpiryMinutes"] ?? "60") * 60
			};
		}

		private string GenerateAccessToken(ApplicationUser user, IList<string> roles)
		{
			var key = new SymmetricSecurityKey(
				Encoding.UTF8.GetBytes(_configuration["Jwt:Secret"]!));
			var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

			var claims = new List<Claim>
			{
				new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
				new(JwtRegisteredClaimNames.Email, user.Email!),
				new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
				new("fullName", user.FullName),
				new("role", user.Role.ToString())
			};

			// Add Identity roles as claims
			foreach (var role in roles)
			{
				claims.Add(new Claim(ClaimTypes.Role, role));
			}

			var expiryMinutes = int.Parse(_configuration["Jwt:ExpiryMinutes"] ?? "60");

			var token = new JwtSecurityToken(
				issuer: _configuration["Jwt:Issuer"],
				audience: _configuration["Jwt:Audience"],
				claims: claims,
				expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
				signingCredentials: credentials);

			return new JwtSecurityTokenHandler().WriteToken(token);
		}

		private async Task<string> CreateRefreshTokenAsync(Guid userId)
		{
			var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

			var refreshToken = new Domain.RefreshToken
			{
				Id = Guid.NewGuid(),
				Token = token,
				UserId = userId,
				ExpiresAt = DateTime.UtcNow.AddDays(
					int.Parse(_configuration["Jwt:RefreshTokenExpiryDays"] ?? "7")),
				CreatedAt = DateTime.UtcNow
			};

			_dbContext.RefreshTokens.Add(refreshToken);
			await _dbContext.SaveChangesAsync();

			return token;
		}
	}
}
