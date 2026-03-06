using dotenv.net;

var builder = WebApplication.CreateBuilder(args);

DotEnv.Load();
builder.Configuration
	.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
	.AddEnvironmentVariables();

builder.InfrastructureUserAccessModule();
builder.InfrastructureWorkforceModule();
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

app.MapControllers();

app.Run();
