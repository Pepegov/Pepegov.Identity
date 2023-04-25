using Microsoft.AspNetCore.Authentication.Cookies;
using OpenIddict.Validation.AspNetCore;

namespace MicroserviceOpenIddictTemplate.DAL.Domain;

public class AuthData
{
    public const string AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme + "," + OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme;
}