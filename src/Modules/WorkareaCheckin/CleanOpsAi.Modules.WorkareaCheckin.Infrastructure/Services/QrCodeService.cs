using CleanOpsAi.Modules.WorkareaCheckin.Application.Common.Interfaces.Services;
using QRCoder;
using System.Text.Json;

namespace CleanOpsAi.Modules.WorkareaCheckin.Infrastructure.Services
{
	public class QrCodeService : IQrCodeService
	{
		public byte[] GeneratePng(string content, int pixelsPerModule = 8)
		{
			using var qrGenerator = new QRCodeGenerator();
			var qrData = qrGenerator.CreateQrCode(content, QRCodeGenerator.ECCLevel.Q);

			var qrCode = new PngByteQRCode(qrData);
			return qrCode.GetGraphic(pixelsPerModule);
		}

		public byte[] GeneratePngFromObject<T>(T data, int pixelsPerModule = 8)
		{
			var json = JsonSerializer.Serialize(data, new JsonSerializerOptions
			{
				PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
				WriteIndented = false
			});
			return GeneratePng(json, pixelsPerModule);
		}

		public string GenerateBase64(string content)
		{
			var bytes = GeneratePng(content);
			return Convert.ToBase64String(bytes);
		}
	}
}
