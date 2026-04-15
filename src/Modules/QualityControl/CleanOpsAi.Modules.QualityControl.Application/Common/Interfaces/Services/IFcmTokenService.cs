using CleanOpsAi.Modules.QualityControl.Application.DTOs.Request;
using CleanOpsAi.Modules.QualityControl.Application.DTOs.Response;

namespace CleanOpsAi.Modules.QualityControl.Application.Common.Interfaces.Services
{
	public interface IFcmTokenService
	{
		Task<FcmTokenDto> RegisterAsync(FcmTokenRegisterDto dto, CancellationToken cancellationToken = default);

		Task DeactivateTokenAsync(string uniqueId, CancellationToken cancellationToken = default);

		Task RefreshTokenAsync(FcmTokenRefreshDto dto, CancellationToken cancellationToken = default);
	}
}
