namespace CleanOpsAi.Modules.Workforce.Application.Dtos.Workers
{
	public class WorkerByUserIdResponse
	{
		public Guid UserId { get; set; }
		public Guid WorkerId { get; set; }
		public string FullName { get; set; } = null!;
	}
}
