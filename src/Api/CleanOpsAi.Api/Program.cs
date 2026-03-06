using dotenv.net;

var builder = WebApplication.CreateBuilder(args);

DotEnv.Load();

builder.InfrastructureUserAccessModule();
// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();


}
app.UseHttpsRedirection();


app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/api/public", () =>
	Results.Ok(new { Message = "This endpoint is public" }))
	.WithName("GetPublic");

// Protected endpoint - requires authentication
app.MapGet("/api/private", () =>
	Results.Ok(new { Message = "This endpoint requires authentication" }))
	.RequireAuthorization()
	.WithName("GetPrivate");

app.MapControllers();

app.Run();
