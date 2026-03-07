namespace CleanOpsAi.Modules.ClientManagement.Application.Contracts
{
	public interface IClientManagementModule
	{
		Task<TResult> ExecuteCommandAsync<TResult>(ICommand<TResult> command);

		Task ExecuteCommandAsync(ICommand command);

		Task<TResult> ExecuteQueryAsync<TResult>(IQuery<TResult> query);
	}
}
