using System.Security.Claims;
using OpenIddict.Abstractions;

namespace Pepegov.Identity.BL;

public static class ClaimsIdentityExtensions
{
    public static bool FindScope(this ClaimsIdentity identity, string scope)
    {
        if (identity is null)
        {
            throw new ArgumentNullException(nameof(identity));
        }

        if (string.IsNullOrEmpty(scope))
        {
            throw new ArgumentNullException(nameof(scope));
        }

        var scopeClaim = identity.Claims.FirstOrDefault(f => f.Type == OpenIddictConstants.Claims.Scope);
        if (scopeClaim is null)
            return false;

        return scopeClaim.Value.ToLower().Contains(scope.Split(OpenIddictConstants.Permissions.Prefixes.Scope)[1].ToLower());
    }
}