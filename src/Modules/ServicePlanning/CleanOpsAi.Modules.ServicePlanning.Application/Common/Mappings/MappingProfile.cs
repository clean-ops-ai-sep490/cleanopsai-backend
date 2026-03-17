using AutoMapper;
using CleanOpsAi.BuildingBlocks.Application.Common.Utils;
using CleanOpsAi.Modules.ServicePlanning.Application.DTOs;
using CleanOpsAi.Modules.ServicePlanning.Domain.Entities;
using System.Text.Json;

namespace CleanOpsAi.Modules.ServicePlanning.Application.Common.Mappings
{
	public class MappingProfile : Profile
	{
		public MappingProfile()
		{
			//Step mapping
			CreateMap<StepCreateDto, Step>()
				.ForMember(
					dest => dest.ConfigSchema, 
					opt => opt.MapFrom(src => src.ConfigSchema.GetRawText()
				));

			CreateMap<StepUpdateDto, Step>()
				.ForMember(
					dest => dest.ConfigSchema,
					opt => opt.MapFrom(src =>
						src.ConfigSchema.HasValue
							? src.ConfigSchema.Value.GetRawText()
							: null))
				.ForAllMembers(opt =>
					opt.Condition((
						src, dest, srcMember) => srcMember != null)
			);

			CreateMap<Step, StepDto>()
				.ForMember(
					dest => dest.ConfigSchema, 
					opt => opt.MapFrom(src =>
					JsonSerializer.Deserialize<JsonElement>(src.ConfigSchema, (JsonSerializerOptions?)null)));


			//Sop mapping
			CreateMap<SopCreateDto, Sop>()
				.ForMember(dest => dest.SopSteps, opt => opt.Ignore());

			CreateMap<Sop, SopDto>()
				.ForMember(dest => dest.RequiredSkillIds,
					opt => opt.MapFrom(src => src.SopRequiredSkills.Select(x => x.SkillId)))
				.ForMember(dest => dest.RequiredCertificationIds,
					opt => opt.MapFrom(src => src.SopRequiredCertifications.Select(x => x.CertificationId)));

			CreateMap<SopStep, SopStepDto>()
				.ForMember(dest => dest.ConfigDetail, opt => opt.MapFrom(src =>
					JsonSerializer.Deserialize<JsonElement>(src.ConfigDetail, (JsonSerializerOptions?)null)));


			CreateMap<SopUpdateDto, Sop>()
				.ForMember(dest => dest.SopSteps, opt => opt.Ignore())
				.ForMember(dest => dest.Version, opt => opt.Ignore())
				.ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));

			//Schedule mapping
			CreateMap<TaskScheduleCreateDto, TaskSchedule>()
				.ForMember(
					dest => dest.RecurrenceConfig,
					opt => opt.MapFrom(src => src.RecurrenceConfig.GetRawText())
				);
			CreateMap<TaskScheduleUpdateDto, TaskSchedule>()
				.ForMember(
					dest => dest.RecurrenceConfig,
					opt => opt.MapFrom(src => src.RecurrenceConfig.GetRawText())
				);

			CreateMap<TaskSchedule, TaskScheduleDto>()
				.ForMember(
				dest => dest.RecurrenceConfig,
					opt => opt.MapFrom(src =>
						JsonSerializer.Deserialize<JsonElement>(src.RecurrenceConfig, (JsonSerializerOptions?)null))
				)
				.ForMember(
					dest => dest.Metadata,
					opt => opt.MapFrom(src =>
						JsonSerializer.Deserialize<JsonElement>(src.Metadata, (JsonSerializerOptions?)null))
				);

			CreateMap<TaskSchedule, ActiveTaskScheduleDto>()
				.ForMember(
					dest => dest.RecurrenceConfig,
					opt => opt.MapFrom(src => JsonHelper.ToJsonElement(src.RecurrenceConfig))
				);
	 


		}
	}
}
