using MediatR; 

namespace CleanOpsAi.Modules.ServicePlanning.Application.Contracts
{
	public interface IQuery<out TResult> : IRequest<TResult>
	{
	}
}
