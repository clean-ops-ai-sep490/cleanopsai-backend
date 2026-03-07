using MediatR; 

namespace CleanOpsAi.Modules.TaskOperations.Application.Contracts
{
	public interface IQuery<out TResult> : IRequest<TResult>
	{
	}
}
