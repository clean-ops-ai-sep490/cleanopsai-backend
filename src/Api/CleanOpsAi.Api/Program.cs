using CleanOpsAi.Api.Hubs;
using CleanOpsAi.Modules.Scoring.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using CleanOpsAi.Api.Middlewares; 

var builder = WebApplication.CreateBuilder(args);

builder.InfrastructureBuildingBlocks();
builder.InfrastructureUserAccessModule();
builder.InfrastructureWorkforceModule();
builder.InfrastructureClientManagementModule();
builder.InfrastructureServicePlanningModule();
builder.InfrastructureTaskOperationsModule(); 
builder.InfrastructureQualityControlModule();
builder.InfrastructureWorkAreaCheckinModule();
builder.InfrastructureScoringModule();

builder.AddWebAPIServices();  

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var scoringDbContext = scope.ServiceProvider.GetRequiredService<ScoringDbContext>();
    scoringDbContext.Database.Migrate();
}

app.UseExceptionHandler();
app.UseMiddleware<PerformanceMiddleware>();

var swaggerEnabled = app.Configuration.GetValue<bool>("Swagger:Enabled"); 
if (app.Environment.IsDevelopment() || swaggerEnabled)
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
	{
		options.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.None);
	});
}

app.UseCors("AllowFrontend");
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<ComplianceHub>("/hubs/compliance");

app.Run();
