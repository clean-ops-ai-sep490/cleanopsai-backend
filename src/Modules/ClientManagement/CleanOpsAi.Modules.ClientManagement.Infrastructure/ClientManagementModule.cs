using CleanOpsAi.Modules.ClientManagement.Application.Contracts;
using MediatR;

namespace CleanOpsAi.Modules.ClientManagement.Infrastructure
{
	public class ClientManagementModule : IClientManagementModule
	{
		private readonly IMediator _mediator;

		public ClientManagementModule(IMediator mediator)
		{
			_mediator = mediator;
		}

		public async Task<TResult> ExecuteCommandAsync<TResult>(ICommand<TResult> command)
		{
			return await _mediator.Send(command);
		}

		public async Task ExecuteCommandAsync(ICommand command)
		{
			await _mediator.Send(command);
		}

		public async Task<TResult> ExecuteQueryAsync<TResult>(IQuery<TResult> query)
		{
			return await _mediator.Send(query);
		}
	}
}
