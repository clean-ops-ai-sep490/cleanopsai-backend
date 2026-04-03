using CleanOpsAi.Modules.TaskOperations.Application.Common.Interfaces.Repositories;
using CleanOpsAi.Modules.TaskOperations.Application.Common.Interfaces.Services;
using CleanOpsAi.Modules.TaskOperations.Application.Common.Mappings;
using CleanOpsAi.Modules.TaskOperations.Application.Configurations;
using CleanOpsAi.Modules.TaskOperations.Application.Services;
using CleanOpsAi.Modules.TaskOperations.Infrastructure.Data;
using CleanOpsAi.Modules.TaskOperations.Infrastructure.Repositories;
using CleanOpsAi.Modules.TaskOperations.Infrastructure.Services;
using CleanOpsAi.Modules.TaskOperations.Infrastructure.Services.Features.Queries;
using Microsoft.EntityFrameworkCore; 
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging; 


namespace Microsoft.Extensions.DependencyInjection;
public static class DependencyInjection
{
	public static void InfrastructureTaskOperationsModule(this IHostApplicationBuilder builder)
	{
		builder.Services.AddSingleton(TimeProvider.System);

		builder.Services.AddDbContext<TaskOperationsDbContext>(options =>
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

		builder.Services.AddAutoMapper(cfg => cfg.LicenseKey = builder.Configuration["AutoMapper:Key"], typeof(MappingProfile));


		builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(AssemblyReference.Assembly));  

		builder.Services.AddScoped<ITaskAssignmentRepository, TaskAssignmentRepository>();
		builder.Services.AddScoped<ITaskStepExecutionRepository, TaskStepExecutionRepository>();
		builder.Services.AddScoped<ITaskSwapRequestRepository, TaskSwapRequestRepository>();
		builder.Services.AddScoped<IEquipmentRequestRepository, EquipmentRequestRepository>();
		builder.Services.AddScoped<IIssueReportRepository, IssueReportRepository>();
		builder.Services.AddScoped<IEmergencyLeaveRequestRepository, EmergencyLeaveRequestRepository>();
		builder.Services.AddScoped<IAdHocRequestRepository, AdHocRequestRepository>();
		builder.Services.AddScoped<IFileStorageService, AzureBlobStorageService>();


        builder.Services.AddScoped<IRecurrenceExpander, RecurrenceExpander>();
		builder.Services.AddScoped<ITaskAssignmentService, TaskAssignmentService>();
		builder.Services.AddScoped<ITaskSwapRequestService, TaskSwapRequestService>();
		builder.Services.AddScoped<IEquipmentRequestService, EquipmentRequestService>();
		builder.Services.AddScoped<IIssueReportService, IssueReportService>();
        builder.Services.AddScoped<IEmergencyLeaveRequestService, EmergencyLeaveRequestService>();
        builder.Services.AddScoped<IAdHocRequestService, AdHocRequestService>();
        builder.Services.AddScoped<IWorkerQueryService, WorkerQueryService>();
        builder.Services.AddScoped<ISupervisorQueryService, SupervisorQueryService>();
        builder.Services.AddScoped<IWorkAreaQueryService, WorkAreaQueryService>();
		builder.Services.AddScoped<IWorkerQueryService, WorkerQueryService>();
		 
		builder.Services.AddScoped<IWorkerCertificationSkillQueryService, WorkerCertificationSkillQueryService>();
		builder.Services.AddScoped<ISopRequirementsQueryService, SopRequirementsQueryService>();
		builder.Services.AddScoped<ISupervisorQueryService, SupervisorQueryService>();


    }
}
