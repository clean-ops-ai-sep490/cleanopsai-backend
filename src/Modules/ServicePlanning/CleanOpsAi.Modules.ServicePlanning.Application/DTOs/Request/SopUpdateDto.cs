using CleanOpsAi.Modules.ServicePlanning.Domain.Enums;

namespace CleanOpsAi.Modules.ServicePlanning.Application.DTOs.Request
{
	public class SopUpdateDto
	{
		/// <summary>
		/// Version number returned by the client.  Caller should send the value they read previously.
		/// This is used to detect concurrent modifications and avoid lost updates.
		/// </summary>
		public int? Version { get; set; }
		public string? Name { get; set; }
		public string? Description { get; set; }
		public ServiceType? ServiceType { get; set; }
		public EnvironmentType? EnvironmentType { get; set; } 
		public List<SopStepUpdateDto>? Steps { get; set; }
	}
}
