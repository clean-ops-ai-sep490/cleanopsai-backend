namespace CleanOpsAi.Modules.WorkareaCheckin.Application.Common.Interfaces.Services
{
	public interface IQrCodeService
	{
		byte[] GeneratePng(string content, int pixelsPerModule = 8);

		byte[] GeneratePngFromObject<T>(T data, int pixelsPerModule = 8);

		string GenerateBase64(string content);
	}
}
