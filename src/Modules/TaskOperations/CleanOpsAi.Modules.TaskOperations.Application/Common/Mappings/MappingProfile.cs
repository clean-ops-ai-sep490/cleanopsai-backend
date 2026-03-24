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

			CreateMap<TaskSwapRequest, SwapRequestDto>();

			CreateMap<TaskAssignment, SwapTaskInfoDto>();

            // EquipmentRequest mappings
            CreateMap<EquipmentRequest, EquipmentRequestDto>();
            CreateMap<CreateEquipmentRequestDto, EquipmentRequest>();
            CreateMap<UpdateEquipmentRequestDto, EquipmentRequest>()
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
        }
    }
}
