using CleanOpsAi.Api.Middlewares;

var builder = WebApplication.CreateBuilder(args);


builder.InfrastructureUserAccessModule();
builder.InfrastructureWorkforceModule();
builder.InfrastructureClientManagementModule();
builder.InfrastructureServicePlanningModule();
builder.InfrastructureTaskOperationsModule(); 

builder.AddWebAPIServices(); 

var app = builder.Build();

app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseMiddleware<PerformanceMiddleware>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(); 
}
app.UseHttpsRedirection();


app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
