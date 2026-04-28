using AutoMapper;
using CleanOpsAi.Modules.WorkareaCheckin.Application.DTOs.Request;
using CleanOpsAi.Modules.WorkareaCheckin.Application.DTOs.Response;
using CleanOpsAi.Modules.WorkareaCheckin.Domain.Entities;
using CleanOpsAi.Modules.WorkareaCheckin.Domain.Owned;

namespace CleanOpsAi.Modules.WorkareaCheckin.Application.Common.Mappings
{
	public class MappingProfile : Profile
	{
		public MappingProfile()
		{
			CreateMap<WorkareaCheckinPointCreateDto, WorkareaCheckinPoint>();
			CreateMap<WorkareaCheckinPointUpdateDto, WorkareaCheckinPoint>();

			CreateMap<WorkareaCheckinPoint, WorkareaCheckinPointDto>();

			CreateMap<AccessDevice, AccessDeviceDto>();
			CreateMap<AccessDeviceCreateDto, AccessDevice>();

			CreateMap<BleBeaconInfoDto, BleBeaconInfo>();
			CreateMap<BleBeaconInfo, BleBeaconInfoDto>();

		}
	}
}
