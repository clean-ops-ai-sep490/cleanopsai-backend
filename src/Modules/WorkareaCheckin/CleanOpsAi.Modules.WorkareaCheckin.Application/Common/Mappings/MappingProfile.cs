using AutoMapper;
using CleanOpsAi.Modules.WorkareaCheckin.Application.DTOs.Request;
using CleanOpsAi.Modules.WorkareaCheckin.Application.DTOs.Response;
using CleanOpsAi.Modules.WorkareaCheckin.Domain.Entities;

namespace CleanOpsAi.Modules.WorkareaCheckin.Application.Common.Mappings
{
	public class MappingProfile : Profile
	{
		public MappingProfile()
		{
			CreateMap<WorkareaCheckinPointCreateDto, WorkareaCheckinPoint>();
			CreateMap<WorkareaCheckinPointUpdateDto, WorkareaCheckinPoint>();

			CreateMap<WorkareaCheckinPoint, WorkareaCheckinPointDto>();
		}
	}
}
