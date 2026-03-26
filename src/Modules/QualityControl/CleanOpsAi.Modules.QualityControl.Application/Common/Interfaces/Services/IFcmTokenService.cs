using CleanOpsAi.Modules.QualityControl.Application.DTOs.Request;
using CleanOpsAi.Modules.QualityControl.Application.DTOs.Response;

namespace CleanOpsAi.Modules.QualityControl.Application.Common.Interfaces.Services
{
	public interface IFcmTokenService
	{
		Task<FcmTokenDto> CreateOrUpdateAsync(Guid UserId, FcmTokenCreateDto dto, CancellationToken cancellationToken = default);

		Task DeactivateTokenAsync(Guid UserId, string uniqueId, CancellationToken cancellationToken = default);
	}
}
