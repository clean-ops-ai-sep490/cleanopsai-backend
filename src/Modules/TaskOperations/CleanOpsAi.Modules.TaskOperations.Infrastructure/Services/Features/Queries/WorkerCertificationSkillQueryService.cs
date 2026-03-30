using CleanOpsAi.BuildingBlocks.Infrastructure.Events.Request;
using CleanOpsAi.Modules.TaskOperations.Application.Common.Interfaces.Services;
using MassTransit;

namespace CleanOpsAi.Modules.TaskOperations.Infrastructure.Services.Features.Queries
{
	public class WorkerCertificationSkillQueryService : IWorkerCertificationSkillQueryService
	{
		private readonly IRequestClient<GetQualifiedWorkersRequested> _getQualifiedWorkersClient;
		private readonly IRequestClient<CheckSingleWorkerCompetencyRequested> _checkSingleWorkerCompetencyClient;

		public WorkerCertificationSkillQueryService(
			IRequestClient<GetQualifiedWorkersRequested> getQualifiedWorkersClient,
			IRequestClient<CheckSingleWorkerCompetencyRequested> checkSingleWorkerCompetencyClient
		)
		{
			_getQualifiedWorkersClient = getQualifiedWorkersClient;
			_checkSingleWorkerCompetencyClient = checkSingleWorkerCompetencyClient;
		}

		public async Task<List<Guid>> GetQualifiedWorkersAsync(
		   List<Guid> requiredSkillIds,
		   List<Guid> requiredCertificationIds,
		   CancellationToken ct)
		{
			var response = await _getQualifiedWorkersClient.GetResponse<GetQualifiedWorkersIntegrated>(
				new GetQualifiedWorkersRequested
				{
					RequiredSkillIds = requiredSkillIds,
					RequiredCertificationIds = requiredCertificationIds
				}, ct);

			return response.Message.QualifiedWorkerIds;
		}

		// Dùng cho CreateSwapRequest
		public async Task<bool> IsWorkerQualifiedAsync(
			Guid workerId,
			List<Guid> requiredSkillIds,
			List<Guid> requiredCertificationIds,
			CancellationToken ct)
		{
			var response = await _checkSingleWorkerCompetencyClient.GetResponse<CheckSingleWorkerCompetencyIntegrated>(
				new CheckSingleWorkerCompetencyRequested
				{
					WorkerId = workerId,
					RequiredSkillIds = requiredSkillIds,
					RequiredCertificationIds = requiredCertificationIds
				}, ct);

			return response.Message.IsQualified;
		}
	}
}
