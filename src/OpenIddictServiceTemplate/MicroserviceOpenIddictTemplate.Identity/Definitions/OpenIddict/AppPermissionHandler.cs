using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using MicroserviceOpenIddictTemplate.Identity.Base.Helpers;

namespace MicroserviceOpenIddictTemplate.Identity.Definitions.OpenIddict;

public class AppPermissionHandler : AuthorizationHandler<PermissionRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
    {
        if (context.User.Identity is null)
        {
            return Task.CompletedTask;
        }

        var identity = (ClaimsIdentity)context.User.Identity;
        var claim = ClaimsHelper.GetValue<string>(identity, requirement.PermissionName);
        if (claim == null)
        {
            return Task.CompletedTask;
        }

        context.Succeed(requirement);
        return Task.CompletedTask;
    }
}