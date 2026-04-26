using CleanOpsAi.Modules.Scoring.IntegrationEvents;

namespace CleanOpsAi.Modules.Scoring.Application.Common.Interfaces.Services
{
	public interface IScoringRetrainRequestHandler
	{
		bool InlineExecutionEnabled { get; }

		Task HandleAsync(ScoringRetrainRequestedEvent message, CancellationToken ct = default);
	}
}
