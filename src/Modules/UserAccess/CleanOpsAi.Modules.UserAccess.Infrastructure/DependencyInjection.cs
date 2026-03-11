using CleanOpsAi.Modules.UserAccess.Application.Contracts;
using CleanOpsAi.Modules.UserAccess.Application.Services;
using CleanOpsAi.Modules.UserAccess.Domain;
using CleanOpsAi.Modules.UserAccess.Infrastructure.Auth;
using CleanOpsAi.Modules.UserAccess.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Microsoft.Extensions.DependencyInjection;
public static class DependencyInjection
{
	public static void InfrastructureUserAccessModule(this IHostApplicationBuilder builder)
	{
		builder.Services.AddSingleton(TimeProvider.System);

		// EF Core DbContext
		builder.Services.AddDbContext<UserAccessDbContext>(options =>
			options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
				.UseSnakeCaseNamingConvention());

		// ASP.NET Core Identity
		builder.Services.AddIdentity<ApplicationUser, IdentityRole<Guid>>(options =>
		{
			options.Password.RequiredLength = 8;
			options.Password.RequireDigit = true;
			options.Password.RequireUppercase = true;
			options.Password.RequireLowercase = true;
			options.Password.RequireNonAlphanumeric = false;
			options.User.RequireUniqueEmail = true;
		})
		.AddEntityFrameworkStores<UserAccessDbContext>()
		.AddDefaultTokenProviders();

		// JWT Authentication
		var jwtSecret = builder.Configuration["Jwt:Secret"]!;
		var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret));

		builder.Services.AddAuthentication(options =>
		{
			options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
			options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
		})
		.AddJwtBearer(options =>
		{
			options.MapInboundClaims = false;
			options.TokenValidationParameters = new TokenValidationParameters
			{
				ValidateIssuer = true,
				ValidateAudience = true,
				ValidateLifetime = true,
				ValidateIssuerSigningKey = true,
				ValidIssuer = builder.Configuration["Jwt:Issuer"],
				ValidAudience = builder.Configuration["Jwt:Audience"],
				IssuerSigningKey = key,
				ClockSkew = TimeSpan.Zero,
				NameClaimType = "sub",
				RoleClaimType = "role"
			};
		});

		builder.Services.AddAuthorization();

		builder.Services.AddScoped<IAuthRepository, AuthRepository>();
		builder.Services.AddScoped<IAuthService, AuthService>();
	}
}