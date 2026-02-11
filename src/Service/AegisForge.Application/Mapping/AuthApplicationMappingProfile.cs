using System.Text.Json;
using AegisForge.Application.Dto;
using AutoMapper;
using OpenIddict.Abstractions;
using OpenIddict.EntityFrameworkCore.Models;

namespace AegisForge.Application.Mapping;

public class AuthApplicationMappingProfile : Profile
{
    public AuthApplicationMappingProfile()
    {
        CreateMap<OpenIddictEntityFrameworkCoreApplication<Guid>, ApplicationViewModel>()
            .ConvertUsing<OpenIddictEntityFrameworkCoreApplicationGuidToApplicationViewModelConverter>();
    }
}

public class OpenIddictEntityFrameworkCoreApplicationGuidToApplicationViewModelConverter : ITypeConverter<OpenIddictEntityFrameworkCoreApplication<Guid>, ApplicationViewModel>
{
    public ApplicationViewModel Convert(OpenIddictEntityFrameworkCoreApplication<Guid> source, ApplicationViewModel? destination, ResolutionContext context)
    {
        destination ??= new ApplicationViewModel();

        destination.Id = source.Id;
        destination.ClientId = source.ClientId;
        destination.ClientSecret = source.ClientSecret;
        destination.Type = source.ApplicationType;
        destination.ConsentType = source.ConsentType;

        if (source.RedirectUris != null)
            destination.RedirectUris = JsonSerializer.Deserialize<List<string>>(source.RedirectUris);
        
        if (source.Permissions != null)
        {
            var permissions = JsonSerializer.Deserialize<List<string>>(source.Permissions);
            destination.Permissions = new ApplicationPermissionsViewModel();
            foreach (var permission in permissions!)
            {
                var prefix = permission.Split(":").FirstOrDefault()+":";
                switch (prefix)
                {
                    case OpenIddictConstants.Permissions.Prefixes.Scope:
                        destination.Permissions.Scopes.Add(permission.Split(prefix)[1]);
                        break;
                    case OpenIddictConstants.Permissions.Prefixes.Endpoint:
                        destination.Permissions.Endpoints.Add(permission.Split(prefix)[1]);
                        break;
                    case OpenIddictConstants.Permissions.Prefixes.GrantType:
                        destination.Permissions.GrantTypes.Add(permission.Split(prefix)[1]);
                        break;
                    case OpenIddictConstants.Permissions.Prefixes.ResponseType:
                        destination.Permissions.ResponseTypes.Add(permission.Split(prefix)[1]);
                        break;
                }
            }
        }

        return destination;
    }
}