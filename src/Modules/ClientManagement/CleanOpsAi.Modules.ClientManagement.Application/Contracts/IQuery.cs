using MediatR; 

namespace CleanOpsAi.Modules.ClientManagement.Application.Contracts
{
	public interface IQuery<out TResult> : IRequest<TResult>
	{
	}
}
