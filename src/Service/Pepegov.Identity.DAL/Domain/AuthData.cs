using Microsoft.AspNetCore.Authentication.Cookies;
using OpenIddict.Server.AspNetCore;
using OpenIddict.Validation.AspNetCore;

namespace Pepegov.Identity.DAL.Domain;

public class AuthData
{
    public const string AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme + "," + OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme;
    
    public const string SingInScheme = OpenIddictServerAspNetCoreDefaults.AuthenticationScheme;
    public const string SuperAdminArea = "SuperAdminArea";
}