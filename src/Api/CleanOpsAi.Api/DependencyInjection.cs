using CleanOpsAi.Api.Common.Exceptions;
using CleanOpsAi.Api.Middlewares;
using CleanOpsAi.BuildingBlocks.Application;
using CleanOpsAi.BuildingBlocks.Infrastructure;
using CleanOpsAi.BuildingBlocks.Infrastructure.Extensions;
using CleanOpsAi.Modules.QualityControl.Infrastructure.Consumers;
using CleanOpsAi.Modules.ServicePlanning.Infrastructure.Consumer;
using CleanOpsAi.Modules.TaskOperations.Infrastructure.Consumers;
using CleanOpsAi.Modules.Workforce.Infrastructure.Consumers;
using Microsoft.OpenApi.Models;
using System.Text.Json.Serialization;

public static class DependencyInjection
{
	public static void AddWebAPIServices(this IHostApplicationBuilder builder)
	{
		builder.Services.AddControllers().AddJsonOptions(options =>
		{
			options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
			options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.Never;
		});
		
		builder.Services.AddEndpointsApiExplorer();  

        builder.Services.AddSwaggerGen(c =>
		{
			c.SwaggerDoc("v1", new OpenApiInfo { Title = "cleanopsai_api", Version = "v1" }); 
			c.CustomSchemaIds(type => type.FullName);
			c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme

			{
				In = ParameterLocation.Header,
				Description = "Please enter token",
				Name = "Authorization",
				Type = SecuritySchemeType.Http,
				BearerFormat = "JWT",
				Scheme = "bearer"
			});
			c.AddSecurityRequirement(new OpenApiSecurityRequirement
				{
					{
						new OpenApiSecurityScheme
						{
							Reference = new OpenApiReference
							{
								Type = ReferenceType.SecurityScheme,
								Id = "Bearer"
							}
						},
						new string[] {}
					}
				});

			c.EnableAnnotations();
		});

		builder.Services.AddMessageBroker(
			builder.Configuration,
			typeof(GenerateTaskAssignmentsConsumer).Assembly,
			typeof(UserRegisteredConsumer).Assembly,
			typeof(SendNotificationConsumer).Assembly,
			typeof(GetSopStepsByScheduleConsumer).Assembly,
			typeof(GetWorkersByIdsConsumer).Assembly

		); 

		builder.Services.AddCors(options =>
		{
			options.AddPolicy("AllowFrontend", policy =>
		{
			policy.WithOrigins(
				"http://localhost:3000",
				"https://localhost:3000")
			.AllowAnyHeader()
			.AllowAnyMethod()
			.AllowCredentials();
			});
		});

		builder.Services.AddExceptionHandler<CustomExceptionHandler>();
		builder.Services.AddProblemDetails(); 
		builder.Services.AddScoped<PerformanceMiddleware>();


		builder.Services.AddHttpContextAccessor();  

	}
}
