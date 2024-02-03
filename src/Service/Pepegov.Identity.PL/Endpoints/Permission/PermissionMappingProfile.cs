using AutoMapper;
using Pepegov.Identity.DAL.Models.Identity;
using Pepegov.Identity.PL.Endpoints.Permission.ViewModel;

namespace Pepegov.Identity.PL.Endpoints.Permission;

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