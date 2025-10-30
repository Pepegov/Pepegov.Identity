using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using OpenIddict.Server;

namespace Pepegov.Identity.BL.OpenIddictHandlers;

/// <summary>
/// A handler that is triggered when a response to an authorization request is generated, for example, when an authorization code or token is successfully issued (/connect/authorize endpoint).
/// </summary>
public class AttachSessionStateServerHandler : IOpenIddictServerHandler<OpenIddictServerEvents.ApplyAuthorizationResponseContext>
{
    public ValueTask HandleAsync(OpenIddictServerEvents.ApplyAuthorizationResponseContext context)
    {
        // If request has a clientId, an absolute redirectUri and a sessionId in the Cookie  
        if (context.Request?.ClientId is string clientId
            && Uri.TryCreate(context.Request.RedirectUri, UriKind.Absolute, out var redirectUri)
            && context.Transaction.GetHttpRequest()?.GetSessionId() is string sessionId)
        {
            var origin = redirectUri.GetLeftPart(UriPartial.Authority);
            var salt = RandomNumberGenerator.GetHexString(8);
            
            //Generating data for a hash
            var utf8Bytes = Encoding.UTF8.GetBytes(clientId + origin + sessionId + salt);
            var hashBytes = SHA256.HashData(utf8Bytes);
            var hashBase64Url = Base64UrlTextEncoder.Encode(hashBytes);

            // Note: The session_state parameter is used so that the frontend client (an SPA or another browser client)
            // can track whether the user's session has changed on the server (for example, if the user has logged out).
            context.Response.SetParameter("session_state", hashBase64Url + "." + salt);
        }

        return default;
    }
}