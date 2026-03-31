using CleanOpsAi.BuildingBlocks.Infrastructure.Events.Request;
using CleanOpsAi.Modules.ServicePlanning.Application.Common.Interfaces.Services;
using MassTransit;

namespace CleanOpsAi.Modules.ServicePlanning.Infrastructure.Consumer
{
	public class GetSopRequirementsByScheduleConsumer : IConsumer<SopRequirementsRequested>
	{
		private readonly ITaskScheduleService _taskScheduleService;
		private readonly ISopService _sopService;

		public GetSopRequirementsByScheduleConsumer(ITaskScheduleService taskScheduleService, ISopService sopService)
		{
			_taskScheduleService = taskScheduleService;
			_sopService = sopService;
		}
		public async Task Consume(ConsumeContext<SopRequirementsRequested> context)
		{
			var msg = context.Message;
			var ct = context.CancellationToken;

			var schedule = await _taskScheduleService.GetById(msg.TaskScheduleId, ct);
			if (schedule is null)
			{
				await context.RespondAsync(new SopRequirementsIntegrated
				{
					Found = false
				});
				return;
			}
			var sop = await _sopService.GetSopWithDetail(schedule.SopId);


			if (sop is null)
			{
				await context.RespondAsync(new SopRequirementsIntegrated
				{
					Found = false
				});
				return;
			}

			await context.RespondAsync(new SopRequirementsIntegrated
			{
				Found = true,
				RequiredSkillIds = sop.SopRequiredSkills
				.Select(x => x.SkillId)
				.ToList(),
				RequiredCertificationIds = sop.SopRequiredCertifications
				.Select(x => x.CertificationId)
				.ToList()
			});
		}
	}
}
