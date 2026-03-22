using AutoMapper;
using CleanOpsAi.Modules.TaskOperations.Application.DTOs.Request;
using CleanOpsAi.Modules.TaskOperations.Application.DTOs.Response;
using CleanOpsAi.Modules.TaskOperations.Domain.Entities;

namespace CleanOpsAi.Modules.TaskOperations.Application.Common.Mappings
{
	public class MappingProfile : Profile
	{
		public MappingProfile() 
		{
			CreateMap<TaskAssignmentUpdateDto, TaskAssignment>(); 
			CreateMap<TaskAssignment, TaskAssignmentDto>();

			CreateMap<TaskSwapRequest, TaskSwapRequestDto>();
			CreateMap<TaskSwapRequestCreateDto, TaskSwapRequest>();

			CreateMap<TaskSwapRequest, SwapRequestDto>()
			.ForMember(dest => dest.RequesterId,
				opt => opt.MapFrom(src => src.RequesterId))
			.ForMember(dest => dest.RequesterTask,
				opt => opt.MapFrom(src => src.TaskAssignment))
			.ForMember(dest => dest.TargetTask,
				opt => opt.MapFrom(src => src.TargetTaskAssignment));

			CreateMap<TaskAssignment, SwapTaskInfoDto>();
		}
	}
}
