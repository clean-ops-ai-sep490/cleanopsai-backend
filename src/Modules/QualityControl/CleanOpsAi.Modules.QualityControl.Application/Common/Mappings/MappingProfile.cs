using AutoMapper;
using CleanOpsAi.BuildingBlocks.Application.Common.Utils;
using CleanOpsAi.Modules.QualityControl.Application.DTOs.Request;
using CleanOpsAi.Modules.QualityControl.Application.DTOs.Response;
using CleanOpsAi.Modules.QualityControl.Domain.Entities;

namespace CleanOpsAi.Modules.QualityControl.Application.Common.Mappings
{
	public class MappingProfile : Profile
	{
		public MappingProfile()
		{
			CreateMap<NotificationRecipient, NotificationListItemDto>()
				.ForMember(d => d.NotificationId, o => o.MapFrom(s => s.AppNotification.Id))
				.ForMember(d => d.Title, o => o.MapFrom(s => s.AppNotification.Title))
				.ForMember(d => d.Body, o => o.MapFrom(s => s.AppNotification.Body))
				.ForMember(d => d.Priority, o => o.MapFrom(s => s.AppNotification.Priority.ToString()))
				.ForMember(d => d.SenderType, o => o.MapFrom(s => s.AppNotification.SenderType.ToString()))
				.ForMember(d => d.SenderId, o => o.MapFrom(s => s.AppNotification.SenderId))
				.ForMember(d => d.Created, o => o.MapFrom(s => s.AppNotification.Created));

			CreateMap<NotificationRecipient, NotificationDetailDto>()
				.ForMember(d => d.NotificationId, o => o.MapFrom(s => s.AppNotification.Id))
				.ForMember(d => d.Title, o => o.MapFrom(s => s.AppNotification.Title))
				.ForMember(d => d.Body, o => o.MapFrom(s => s.AppNotification.Body))
				.ForMember(d => d.Payload, o => o.MapFrom(s => s.AppNotification.Payload))
				.ForMember(d => d.Priority, o => o.MapFrom(s => s.AppNotification.Priority.ToString()))
				.ForMember(d => d.SenderType, o => o.MapFrom(s => s.AppNotification.SenderType.ToString()))
				.ForMember(d => d.SenderId, o => o.MapFrom(s => s.AppNotification.SenderId))
				.ForMember(d => d.Created, o => o.MapFrom(s => s.AppNotification.Created));

			CreateMap<NotificationCreateDto, AppNotification>()
				.ForMember(dest=> dest.Payload,
							opt=> opt.MapFrom(src=> src.Payload.GetRawText()));

			CreateMap<NotificationUpdateDto, AppNotification>()
				.ForMember(dest => dest.Payload,
							opt => opt.MapFrom(src => src.Payload.GetRawText()));

			CreateMap<AppNotification, NotificationDto>()
				.ForMember(
					dest => dest.Payload,
					opt => opt.MapFrom(src => JsonHelper.ToJsonElement(src.Payload))
				); ;
		}
	}
}
