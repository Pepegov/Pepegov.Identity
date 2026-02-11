using AegisForge.Application.Dto;
using AegisForge.Domain.Entity;
using AutoMapper;

namespace AegisForge.Application.Mapping;

public class PermissionMappingProfile : Profile
{
    public PermissionMappingProfile()
    {
        CreateMap<PermissionViewModel, ApplicationPermission>()
            .ForMember(x => x.ApplicationPermissionId, o => o.Ignore())
            .ForMember(x => x.ApplicationUserProfiles, o => o.Ignore());

        CreateMap<ApplicationPermission, PermissionViewModel>();
    }
}