using Microsoft.AspNetCore.Authorization;

namespace MicroserviceOpenIddictTemplate.PL.Definitions.OpenIddict;

public class PermissionRequirement : IAuthorizationRequirement
{
    public string PermissionName { get; }
    public PermissionRequirement(string permissionName) => PermissionName = permissionName;
}