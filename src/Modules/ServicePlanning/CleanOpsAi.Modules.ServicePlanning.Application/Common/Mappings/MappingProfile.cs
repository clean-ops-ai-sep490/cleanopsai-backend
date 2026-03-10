using AutoMapper; 
using CleanOpsAi.Modules.ServicePlanning.Domain.Entities;
using System.Text.Json;

namespace CleanOpsAi.Modules.ServicePlanning.Application.Common.Mappings
{
	public class MappingProfile : Profile
	{
		public MappingProfile()
		{
			CreateMap<StepCreateDto, Step>().ForMember(
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
	.ForMember(dest => dest.ConfigSchema, opt => opt.MapFrom(src =>
		JsonSerializer.Deserialize<JsonElement>(src.ConfigSchema, (JsonSerializerOptions?)null)));
		}
	}
}
