namespace CleanOpsAi.BuildingBlocks.Application
{
	public interface IUserContext
	{
		Guid UserId { get; }
		string Role { get; }
		string Email { get; }
		string FullName { get; }
		bool IsAuthenticated { get; }
	}
}
