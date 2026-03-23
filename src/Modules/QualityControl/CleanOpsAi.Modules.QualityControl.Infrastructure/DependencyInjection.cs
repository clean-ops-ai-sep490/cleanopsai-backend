using CleanOpsAi.Modules.QualityControl.Application.Common.Interfaces.Repositories;
using CleanOpsAi.Modules.QualityControl.Application.Common.Interfaces.Services;
using CleanOpsAi.Modules.QualityControl.Application.Common.Mappings;
using CleanOpsAi.Modules.QualityControl.Application.Services;
using CleanOpsAi.Modules.QualityControl.Infrastructure.Data;
using CleanOpsAi.Modules.QualityControl.Infrastructure.Repositories;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Microsoft.AspNetCore.Builder.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text;

namespace Microsoft.Extensions.DependencyInjection;
public static class DependencyInjection
{
	public static void InfrastructureQualityControlModule(this IHostApplicationBuilder builder)
	{
		builder.Services.AddSingleton(TimeProvider.System);

		builder.Services.AddDbContext<QualityControlDbContext>(options =>
		{
			options.UseNpgsql(
				 builder.Configuration["ConnectionStrings:DefaultConnection"]
			)
				   .UseSnakeCaseNamingConvention()
				   .EnableSensitiveDataLogging()
				   .EnableDetailedErrors();
			options.EnableSensitiveDataLogging();
			options.LogTo(Console.WriteLine, new[] { DbLoggerCategory.Database.Command.Name }, LogLevel.Information);
		});

		var serviceAccountKey = builder.Configuration["Firebase:ServiceAccountKey"];  
		var credential = CredentialFactory
			.FromJson<ServiceAccountCredential>(serviceAccountKey)
			.ToGoogleCredential();

		if (FirebaseApp.DefaultInstance == null)
		{
			FirebaseApp.Create(new AppOptions
			{
				Credential = credential,
				ProjectId = builder.Configuration["Firebase:ProjectId"]
			});
		}
		builder.Services.AddAutoMapper(cfg => cfg.LicenseKey = builder.Configuration["AutoMapper:Key"], typeof(MappingProfile));

		//Repo
		builder.Services.AddScoped<IFcmTokenRepository, FcmTokenRepository>();

		
		//Services
		builder.Services.AddScoped<IFcmTokenService, FcmTokenService>();



	}
}