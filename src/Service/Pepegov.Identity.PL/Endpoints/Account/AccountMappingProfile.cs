using System.Security.Claims;
using AutoMapper;
using OpenIddict.Abstractions;
using Pepegov.Identity.DAL.Models.Identity;
using Pepegov.Identity.DAL.ViewModel;
using Pepegov.MicroserviceFramework.Infrastructure.Helpers;

namespace Pepegov.Identity.PL.Endpoints.Account;

public class AccountMappingProfile : Profile
{
    public AccountMappingProfile()
    {
        CreateMap<ApplicationUser, UserAccountViewModel>()
            .ForMember(x => x.Roles, o => o.Ignore())
            .ForMember(x => x.PositionName, o => o.Ignore());
        
        CreateMap<UserAccountViewModel, ApplicationUser>()
            .ConvertUsing<UserAccountViewModelToApplicationUserConvert>();
        
        CreateMap<RegisterViewModel, ApplicationUser>()
            .ConvertUsing<RegisterViewModelToApplicationUserConvert>();

        CreateMap<ClaimsIdentity, UserAccountViewModel>()
            .ConvertUsing<ClaimsIdentityToUserAccountViewModel>();
    }
}

public class UserAccountViewModelToApplicationUserConvert : ITypeConverter<UserAccountViewModel, ApplicationUser>
{
    public ApplicationUser Convert(UserAccountViewModel source, ApplicationUser destination, ResolutionContext context)
    {
        if (destination is null)
        {
            destination = new ApplicationUser();
        }

        destination.UserName = source.UserName;
        destination.FirstName = source.FirstName;
        destination.LastName = source.LastName;
        destination.MiddleName = source.MiddleName;
        destination.Email = source.Email;
        destination.PhoneNumber = source.PhoneNumber;

        return destination;
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
        destination.BirthDate = source.DateOfBirth;
        
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