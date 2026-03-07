using MediatR; 

namespace CleanOpsAi.Modules.Workforce.Application.Contracts
{
	public interface IQuery<out TResult> : IRequest<TResult>
	{
	}
}
