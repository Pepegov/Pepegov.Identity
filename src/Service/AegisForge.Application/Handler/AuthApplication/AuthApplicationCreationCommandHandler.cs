using System.Net;
using AegisForge.Application.Query.AuthApplication;
using MediatR;
using Microsoft.Extensions.Logging;
using OpenIddict.Abstractions;
using Pepegov.Identity.BL;
using Pepegov.MicroserviceFramework.ApiResults;
using Pepegov.MicroserviceFramework.Exceptions;

namespace AegisForge.Application.Handler.AuthApplication;

public class AuthApplicationCreationCommandHandler(
    ILogger<AuthApplicationCreationCommandHandler> logger,
    IOpenIddictApplicationManager applicationManager)
    : IRequestHandler<AuthApplicationCreationCommand, ApiResult>
{
    public async Task<ApiResult> Handle(AuthApplicationCreationCommand request, CancellationToken cancellationToken)
    {
        if (await applicationManager.FindByClientIdAsync(request.ClientId, cancellationToken) is not null) //if the client exist then dont add it
        {
            return new ApiResult(HttpStatusCode.Conflict, new MicroserviceAlreadyExistsException($"Application by ClientId {request.ClientId} already exist"));
        }

        var client = new OpenIddictApplicationDescriptor
        {
            ClientId = request.ClientId,
            ClientSecret = request.ClientSecret,
            DisplayName = request.DisplayName,
            ConsentType = request.ConsentType,
        };   
        
        client.AddScopes(request.Scopes);
        client.AddGrandTypes(request.GrandTypes);
        client.AddRedirectUris(request.RedirectUris);
        
        client.AddResponseTypes();
        client.AddEndpoints();

        await applicationManager.CreateAsync(client, cancellationToken);

        logger.LogInformation($"Application DisplayName: {request.DisplayName} | ClientId {request.ClientId} successfully created");
        return new ApiResult(HttpStatusCode.OK);
    }
}