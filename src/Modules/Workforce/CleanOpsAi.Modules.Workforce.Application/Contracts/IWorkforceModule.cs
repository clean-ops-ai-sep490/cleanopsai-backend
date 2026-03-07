namespace CleanOpsAi.Modules.Workforce.Application.Contracts
{
	public interface IWorkforceModule
	{
		Task<TResult> ExecuteCommandAsync<TResult>(ICommand<TResult> command);

		Task ExecuteCommandAsync(ICommand command);

		Task<TResult> ExecuteQueryAsync<TResult>(IQuery<TResult> query);
	}
}
