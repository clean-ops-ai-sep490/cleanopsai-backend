using CleanOpsAi.BuildingBlocks.Application.Interfaces.Messaging;
using CleanOpsAi.BuildingBlocks.Infrastructure.Events.Request;
using CleanOpsAi.Modules.TaskOperations.Application.Common.Interfaces.Services; 

namespace CleanOpsAi.Modules.TaskOperations.Infrastructure.Services.Features.Queries
{
	public class WorkerCertificationSkillQueryService : IWorkerCertificationSkillQueryService
	{
		private readonly IIntegrationBus _bus;  

		public WorkerCertificationSkillQueryService(IIntegrationBus bus)
		{
			_bus = bus;
		}


		public async Task<List<Guid>> GetQualifiedWorkersAsync(
		   List<Guid> requiredSkillIds,
		   List<Guid> requiredCertificationIds,
		   CancellationToken ct)
		{
			var response = await _bus.RequestAsync<GetQualifiedWorkersRequested, GetQualifiedWorkersIntegrated>(
			new GetQualifiedWorkersRequested
			{
				RequiredSkillIds = requiredSkillIds,
				RequiredCertificationIds = requiredCertificationIds
			}, ct);

			return response.QualifiedWorkerIds;
		}

		// Dùng cho CreateSwapRequest
		public async Task<bool> IsWorkerQualifiedAsync(
			Guid workerId,
			List<Guid> requiredSkillIds,
			List<Guid> requiredCertificationIds,
			CancellationToken ct)
		{
			var response = await _bus.RequestAsync<CheckSingleWorkerCompetencyRequested, CheckSingleWorkerCompetencyIntegrated>(
			new CheckSingleWorkerCompetencyRequested
			{
				WorkerId = workerId,
				RequiredSkillIds = requiredSkillIds,
				RequiredCertificationIds = requiredCertificationIds
			}, ct);

			return response.IsQualified;
		}
	}
}
