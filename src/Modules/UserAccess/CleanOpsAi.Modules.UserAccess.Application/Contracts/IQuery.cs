using MediatR;

namespace CleanOpsAi.Modules.UserAccess.Application.Contracts
{
	public interface IQuery<out TResult> : IRequest<TResult>
	{
	}
}
