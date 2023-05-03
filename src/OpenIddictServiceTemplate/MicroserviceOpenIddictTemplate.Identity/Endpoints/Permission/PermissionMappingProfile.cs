using AutoMapper;
using MicroserviceOpenIddictTemplate.DAL.Models.Identity;
using MicroserviceOpenIddictTemplate.Identity.Endpoints.Permission.ViewModel;

namespace MicroserviceOpenIddictTemplate.Identity.Endpoints.Permission;

public class PermissionMappingProfile : Profile
{
    public PermissionMappingProfile()
    {
        CreateMap<PermissionViewModel, ApplicationPermission>()
            .ForMember(x => x.ApplicationPermissionId, o => o.Ignore())
            .ForMember(x => x.ApplicationUserProfiles, o => o.Ignore());
    }
}