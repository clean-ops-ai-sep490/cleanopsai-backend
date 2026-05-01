using CleanOpsAi.BuildingBlocks.Infrastructure.Extensions; 
using CleanOpsAi.Modules.Scoring.Infrastructure.Consumers;

var builder = Host.CreateApplicationBuilder(args);

builder.InfrastructureBuildingBlocks();
builder.InfrastructureScoringModule();
builder.InfrastructureTaskOperationsModule();

builder.Services.AddMessageBroker(
	builder.Configuration,
	typeof(ScoringJobRequestedConsumer).Assembly
);

var host = builder.Build();
await host.RunAsync();
