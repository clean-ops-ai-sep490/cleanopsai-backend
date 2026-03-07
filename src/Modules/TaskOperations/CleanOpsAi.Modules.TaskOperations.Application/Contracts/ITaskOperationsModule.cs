namespace CleanOpsAi.Modules.TaskOperations.Application.Contracts
{
	public interface ITaskOperationsModule
	{
		Task<TResult> ExecuteCommandAsync<TResult>(ICommand<TResult> command);

		Task ExecuteCommandAsync(ICommand command);

		Task<TResult> ExecuteQueryAsync<TResult>(IQuery<TResult> query);
	}
}
