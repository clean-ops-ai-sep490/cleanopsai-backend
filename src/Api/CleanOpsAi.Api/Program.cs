using CleanOpsAi.Api.Middlewares; 

var builder = WebApplication.CreateBuilder(args);

builder.InfrastructureBuildingBlocks();
builder.InfrastructureUserAccessModule();
builder.InfrastructureWorkforceModule();
builder.InfrastructureClientManagementModule();
builder.InfrastructureServicePlanningModule();
builder.InfrastructureTaskOperationsModule(); 
builder.InfrastructureQualityControlModule();

builder.AddWebAPIServices();  

var app = builder.Build();

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
