using AegisForge.Application.Dto;
using AegisForge.Domain.Aggregate;
using AegisForge.Domain.Entity;
using AutoMapper;

namespace AegisForge.Application.Mapping;

public class SessionMappingProfile : Profile
{
    public SessionMappingProfile()
    {
        CreateMap<ApplicationSession, SessionViewModel>()
            .ForMember(x => x.Id, o => o.MapFrom(s => s.Id))
            .ForMember(x => x.SessionId, o => o.MapFrom(s => s.SessionId))
            .ForMember(x => x.StatusType, o => o.MapFrom(s => s.SessionStatusType))
            .ForMember(x => x.UserConnectionInfo, o => o.MapFrom(s => s.UserConnectionInfo))
            .ReverseMap();

        CreateMap<UserConnectionInfo, UserConnectionInfoDto>()
            .ForMember(x => x.Ip, o => o.MapFrom(s => s.Ip))
            .ForMember(x => x.GeoData, o => o.MapFrom(s => s.GeoData))
            .ForMember(x => x.UserAgent, o => o.MapFrom(s => s.UserAgent))
            .ReverseMap();

        CreateMap<GeoInfo, GeoDataDto>()
            .ForMember(x => x.City, o => o.MapFrom(s => s.City))
            .ForMember(x => x.Country, o => o.MapFrom(s => s.Country))
            .ForMember(x => x.CountryCode, o => o.MapFrom(s => s.CountryCode))
            .ForMember(x => x.Region, o => o.MapFrom(s => s.Region))
            .ReverseMap();

        CreateMap<UserAgentInfo, UserAgentDataDto>()
            .ForMember(x => x.UserAgent, o => o.MapFrom(s => s.UserAgent))
            .ForMember(x => x.Browser, o => o.MapFrom(s => s.Browser))
            .ForMember(x => x.Device, o => o.MapFrom(s => s.Device))
            .ForMember(x => x.Os, o => o.MapFrom(s => s.Os))
            .ReverseMap();
    }
}