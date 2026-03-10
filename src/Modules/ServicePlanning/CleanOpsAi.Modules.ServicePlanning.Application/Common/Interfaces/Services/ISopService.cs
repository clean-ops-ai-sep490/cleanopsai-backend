namespace CleanOpsAi.Modules.ServicePlanning.Application.Common.Interfaces.Services
{
	public interface ISopService 
	{
		Task<SopDto?> GetSopByIdAsync(Guid id);

		Task<SopDto> CreateSopAsync(SopCreateDto dto);
	}
}
