using CleanOpsAi.Modules.UserAccess.Application.Configuration;
using CleanOpsAi.Modules.UserAccess.Application.Contracts;
using CleanOpsAi.Modules.UserAccess.Infrastructure;
using CleanOpsAi.Modules.UserAccess.Infrastructure.Auth0;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens; 
using System.Security.Claims;

namespace Microsoft.Extensions.DependencyInjection;
public static class DependencyInjection
{
	public static void InfrastructureUserAccessModule(this IHostApplicationBuilder builder)
	{
		builder.Services.AddSingleton(TimeProvider.System);

		builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
			.AddJwtBearer(options =>
			{
				options.Authority = builder.Configuration["Auth0:Domain"]; 
				options.Audience = builder.Configuration["Auth0:Audience"];
				options.TokenValidationParameters = new TokenValidationParameters
				{
					NameClaimType = ClaimTypes.NameIdentifier
				};
			});
		builder.Services.AddAuthorization(options =>
		{ 
		}); 

		builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(AssemblyReference.Assembly));

		builder.Services.AddHttpClient<IAuth0Service, Auth0Service>();

		builder.Services.AddScoped<IUserAccessModule, UserAccessModule>();
	}
}