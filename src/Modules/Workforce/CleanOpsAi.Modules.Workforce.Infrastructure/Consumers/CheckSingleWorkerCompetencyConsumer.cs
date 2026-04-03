using CleanOpsAi.BuildingBlocks.Infrastructure.Events.Request;
using CleanOpsAi.Modules.Workforce.Application.Interfaces;
using MassTransit; 

namespace CleanOpsAi.Modules.Workforce.Infrastructure.Consumers
{
	public class CheckSingleWorkerCompetencyConsumer
	: IConsumer<CheckSingleWorkerCompetencyRequested>
	{
		private readonly IWorkerService _workerService;

		public CheckSingleWorkerCompetencyConsumer(IWorkerService workerService)
			=> _workerService = workerService;

		public async Task Consume(ConsumeContext<CheckSingleWorkerCompetencyRequested> context)
		{
			var msg = context.Message;

			var isQualified = await _workerService.IsWorkerQualifiedAsync(
				workerId: msg.WorkerId,
				requiredSkillIds: msg.RequiredSkillIds,
				requiredCertificationIds: msg.RequiredCertificationIds,
				ct: context.CancellationToken);

			await context.RespondAsync(new CheckSingleWorkerCompetencyIntegrated
			{
				IsQualified = isQualified
			});
		}
	}
}
