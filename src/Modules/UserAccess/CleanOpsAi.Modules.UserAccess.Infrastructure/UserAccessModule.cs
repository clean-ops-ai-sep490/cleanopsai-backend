using CleanOpsAi.Modules.UserAccess.Application.Contracts;
using MediatR;

namespace CleanOpsAi.Modules.UserAccess.Infrastructure
{
	public class UserAccessModule : IUserAccessModule
	{
		private readonly IMediator _mediator;

		public UserAccessModule(IMediator mediator)
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
