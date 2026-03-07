using MediatR;

namespace CleanOpsAi.Modules.ClientManagement.Application.Contracts
{
	/// <summary>
	/// use when command that need to retun infomation after execute
	/// </summary>
	/// <typeparam name="TResult"></typeparam>
	public interface ICommand<out TResult> : IRequest<TResult>
	{
		Guid Id { get; }
	}

	/// <summary>
	///  use when command that dont need to return any value
	/// </summary>
	/// <typeparam name="TResult"></typeparam>
	public interface ICommand : IRequest
	{
		Guid Id { get; }
	}
}
