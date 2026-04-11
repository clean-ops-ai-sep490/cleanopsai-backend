using CleanOpsAi.Modules.WorkareaCheckin.Application.DTOs.Request;
using CleanOpsAi.Modules.WorkareaCheckin.Application.DTOs.Response;

namespace CleanOpsAi.Modules.WorkareaCheckin.Application.Common.Interfaces.Services
{
	public interface ICheckinRecordService
	{
		Task<CheckinResponseDto> Checkin(CheckinRequestDto request, CancellationToken ct = default);
	}
}
