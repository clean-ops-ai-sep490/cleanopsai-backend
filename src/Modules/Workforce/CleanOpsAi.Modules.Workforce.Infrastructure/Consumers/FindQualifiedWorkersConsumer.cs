using CleanOpsAi.BuildingBlocks.Infrastructure.Events.Request;
using CleanOpsAi.Modules.Workforce.Application.Interfaces;
using MassTransit;

namespace CleanOpsAi.Modules.Workforce.Infrastructure.Consumers
{
	public class FindQualifiedWorkersConsumer
	: IConsumer<GetQualifiedWorkersRequested>
	{
		private readonly IWorkerService _workerService;

		public FindQualifiedWorkersConsumer(IWorkerService workerService)
			=> _workerService = workerService;

		public async Task Consume(ConsumeContext<GetQualifiedWorkersRequested> context)
		{
			var msg = context.Message;

			var qualifiedIds = await _workerService.GetQualifiedWorkersAsync(
				requiredSkillIds: msg.RequiredSkillIds,
				requiredCertificationIds: msg.RequiredCertificationIds,
				ct: context.CancellationToken);

			await context.RespondAsync(new GetQualifiedWorkersIntegrated
			{
				QualifiedWorkerIds = qualifiedIds
			});
		}
	}
}
