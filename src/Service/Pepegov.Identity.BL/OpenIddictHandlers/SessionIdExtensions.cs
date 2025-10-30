using System.Security.Cryptography;
using Microsoft.AspNetCore.Http;

namespace Pepegov.Identity.BL.OpenIddictHandlers;

public static class SessionIdExtensions
{
    public const string CookieName = "oidc_session";

    public static void IssueSessionCookie(this HttpResponse response)
    {
        var sessionId = RandomNumberGenerator.GetHexString(32);
        response.Cookies.Append(
            CookieName,
            sessionId,
            new CookieOptions
            {
                SameSite = SameSiteMode.None,
                Secure = true,
                HttpOnly = false,
            });
    }

    public static void DeleteSessionCookie(this HttpResponse response) => response.Cookies.Delete(CookieName);

    public static string? GetSessionId(this HttpRequest request) => request.Cookies[CookieName];
}
