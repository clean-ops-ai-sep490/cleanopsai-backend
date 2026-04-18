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

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(); 
}

app.UseCors("AllowFrontend");
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
