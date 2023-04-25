using System.Security.Claims;
using AutoMapper;
using MicroserviceOpenIddictTemplate.DAL.Models.Identity;
using MicroserviceOpenIddictTemplate.Identity.Base.Helpers;
using MicroserviceOpenIddictTemplate.Identity.Endpoints.Account.ViewModel;

namespace MicroserviceOpenIddictTemplate.Identity.Definitions.Mapping;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<ApplicationUser, UserAccountViewModel>();
        CreateMap<UserAccountViewModel, ApplicationUser>();

        CreateMap<RegisterViewModel, ApplicationUser>()
            .ConvertUsing<RegisterViewModelToApplicationUserConvert>();

        CreateMap<ClaimsIdentity, UserAccountViewModel>()
            .ConvertUsing<ClaimsIdentityToUserAccountViewModel>();
    }
}

public class RegisterViewModelToApplicationUserConvert : ITypeConverter<RegisterViewModel, ApplicationUser>
{

    public ApplicationUser Convert(RegisterViewModel source, ApplicationUser destination, ResolutionContext context)
    {
        if (destination is null)
        {
            destination = new ApplicationUser();
        }
        
        destination.FirstName = source.FirstName;
        destination.LastName = source.LastName;
        destination.Email = source.Email;
        destination.UserName = source.Email;
        
        return destination;
    }
}

public class ClaimsIdentityToUserAccountViewModel : ITypeConverter<ClaimsIdentity, UserAccountViewModel>
{
    public UserAccountViewModel Convert(ClaimsIdentity source, UserAccountViewModel destination, ResolutionContext context)
    {
        if (destination is null)
        {
            destination = new UserAccountViewModel();
        }

        destination.FirstName = ClaimsHelper.GetValue<string>(source, ClaimTypes.GivenName);
        destination.LastName = ClaimsHelper.GetValue<string>(source, ClaimTypes.Surname);
        destination.Email = ClaimsHelper.GetValue<string>(source, ClaimTypes.Email);
        destination.PhoneNumber = ClaimsHelper.GetValue<string>(source, ClaimTypes.MobilePhone);

        return destination;
    }
} 