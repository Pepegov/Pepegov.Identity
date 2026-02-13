using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using OpenIddict.Server;

namespace AegisForge.Application.OpenIddictHandlers;

/// <summary>
/// Handler for the OpenIddict event in ASP.NET The OpenID OpenID Core, which adds the check_session_iframe parameter to the OpenID Connect Discovery document (.well-known/openid-configuration).
/// </summary>
// ReSharper disable once ClassNeverInstantiated.Global
public class AttachCheckSessionIframeEndpointServerHandler(IHttpContextAccessor httpContextAccessor, LinkGenerator linkGenerator) : IOpenIddictServerHandler<OpenIddictServerEvents.HandleConfigurationRequestContext>
{
    public ValueTask HandleAsync(OpenIddictServerEvents.HandleConfigurationRequestContext context)
    {
        var httpContext = httpContextAccessor.HttpContext 
                          ?? throw new InvalidOperationException("No HTTP context available");
        
        var checkSessionUrl = linkGenerator.GetUriByPage(httpContext, page: "/Connect/CheckSession");
        context.Metadata["check_session_iframe"] = checkSessionUrl;

        return default;   
    }
}