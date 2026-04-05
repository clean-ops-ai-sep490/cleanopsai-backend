using AutoMapper;
using CleanOpsAi.Modules.TaskOperations.Application.DTOs.Request;
using CleanOpsAi.Modules.TaskOperations.Application.DTOs.Response;
using CleanOpsAi.Modules.TaskOperations.Domain.Entities;
using System.Text.Json;

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

            CreateMap<TaskAssignment, SwapTaskInfoDto>()
                .ForMember(dest => dest.TaskAssignmentId, opt => opt.MapFrom(src => src.Id));

            CreateMap<TaskAssignment, SwapCandidateDto>()
                .ForMember(dest => dest.WorkerId, opt => opt.MapFrom(src => src.AssigneeId))
                .ForMember(dest => dest.Task, opt => opt.MapFrom(src => src));

            // EquipmentRequest mappings
            CreateMap<EquipmentRequest, EquipmentRequestDto>();
            CreateMap<CreateEquipmentRequestDto, EquipmentRequest>();
            CreateMap<UpdateEquipmentRequestDto, EquipmentRequest>()
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));

            // IssueReport mappings
            CreateMap<IssueReport, IssueReportDto>();
            CreateMap<CreateIssueReportDto, IssueReport>();
            CreateMap<UpdateIssueReportDto, IssueReport>()
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));

            // EmergencyLeaveRequest mappings
            CreateMap<EmergencyLeaveRequest, EmergencyLeaveRequestDto>();
            CreateMap<CreateEmergencyLeaveRequestDto, EmergencyLeaveRequest>()
                .ForMember(dest => dest.AudioUrl, opt => opt.Ignore())
                .ForMember(dest => dest.Transcription, opt => opt.Ignore())
                .ForSourceMember(src => src.AudioStream, opt => opt.DoNotValidate())
                .ForSourceMember(src => src.AudioFileName, opt => opt.DoNotValidate());

            // UpdateEmergencyLeaveRequestDto mappings
            CreateMap<AdHocRequest, AdHocRequestDto>();
            CreateMap<CreateAdHocRequestDto, AdHocRequest>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Status, opt => opt.Ignore())
                .ForMember(dest => dest.RequestedByWorkerId, opt => opt.Ignore())
                .ForMember(dest => dest.Created, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.LastModified, opt => opt.Ignore())
                .ForMember(dest => dest.LastModifiedBy, opt => opt.Ignore())
                .ForMember(dest => dest.ReviewedByUserId, opt => opt.Ignore())
                .ForMember(dest => dest.ApprovedAt, opt => opt.Ignore());
            CreateMap<UpdateAdHocRequestDto, AdHocRequest>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));


            CreateMap<TaskStepExecution, TaskStepExecutionDetailDto>()
                .ForMember(
                    dest => dest.ConfigSnapshot,
                    opt => opt.MapFrom(src =>
                    JsonSerializer.Deserialize<JsonElement>(src.ConfigSnapshot, (JsonSerializerOptions?)null)))
                .ForMember(
                    dest => dest.ResultData,
                    opt => opt.MapFrom(src =>
                    JsonSerializer.Deserialize<JsonElement>(src.ResultData, (JsonSerializerOptions?)null)));
        }
	}
}
