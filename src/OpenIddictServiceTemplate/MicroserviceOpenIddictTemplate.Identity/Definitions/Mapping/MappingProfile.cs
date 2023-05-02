using System.Security.Claims;
using AutoMapper;
using MicroserviceOpenIddictTemplate.DAL.Models.Identity;
using MicroserviceOpenIddictTemplate.Identity.Endpoints.Account.ViewModel;
using OpenIddict.Abstractions;
using Pepegov.MicroserviceFramerwork.Helpers;

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

        destination.UserName = source.UserName;
        destination.FirstName = source.FirstName;
        destination.LastName = source.LastName;
        destination.Email = source.Email;
        destination.PhoneNumber = source.PhoneNumber;
        
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

        destination.Id = ClaimsHelper.GetValue<Guid>(source, OpenIddictConstants.Claims.Subject);
        destination.UserName = ClaimsHelper.GetValue<string>(source, OpenIddictConstants.Claims.Name);
        destination.FirstName = ClaimsHelper.GetValue<string>(source, OpenIddictConstants.Claims.GivenName);
        destination.LastName = ClaimsHelper.GetValue<string>(source, OpenIddictConstants.Claims.FamilyName);
        destination.Email = ClaimsHelper.GetValue<string>(source, OpenIddictConstants.Claims.Email);
        destination.PhoneNumber = ClaimsHelper.GetValue<string>(source, OpenIddictConstants.Claims.PhoneNumber);
        destination.Roles = ClaimsHelper.GetValues<string>(source, OpenIddictConstants.Claims.Role);

        return destination;
    }
} 