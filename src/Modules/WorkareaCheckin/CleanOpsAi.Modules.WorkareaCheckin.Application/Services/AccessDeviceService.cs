using CleanOpsAi.Modules.WorkareaCheckin.Application.Common.Interfaces.Repositories;
using CleanOpsAi.Modules.WorkareaCheckin.Application.Common.Interfaces.Services;

namespace CleanOpsAi.Modules.WorkareaCheckin.Application.Services
{
	public class AccessDeviceService : IAccessDeviceService
	{
		private readonly IAccessDeviceRepository _accessDeviceRepository;

		public AccessDeviceService(IAccessDeviceRepository accessDeviceRepository)
		{
			_accessDeviceRepository = accessDeviceRepository;
		}
	}
}
