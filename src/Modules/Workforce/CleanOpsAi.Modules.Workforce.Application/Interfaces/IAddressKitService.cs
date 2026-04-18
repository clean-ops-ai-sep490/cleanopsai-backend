namespace CleanOpsAi.Modules.Workforce.Application.Interfaces
{
	public interface IAddressKitService
	{
		Task<string> GetProvincesAsync(CancellationToken ct);
		Task<string> GetCommunesAsync(string provinceCode, CancellationToken ct);
	}
}