using CleanOpsAi.Modules.WorkareaCheckin.Domain.Enums;

namespace CleanOpsAi.Modules.WorkareaCheckin.Application.DTOs.Response
{
	public class CheckinResponseDto
	{
		public Guid Id { get; set; }
		public DateTime CheckinAt { get; set; }
		public CheckinType Type { get; set; }
	}
}
