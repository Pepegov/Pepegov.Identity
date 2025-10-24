using System.Net;
using MediatR;
using OpenIddict.Abstractions;
using Pepegov.Identity.BL;
using Pepegov.Identity.PL.Endpoints.Application.Queries;
using Pepegov.Identity.PL.Jobs;
using Pepegov.MicroserviceFramework.ApiResults;
using Pepegov.MicroserviceFramework.Exceptions;

namespace Pepegov.Identity.PL.Endpoints.Application.Handlers;

public class CreateApplicationCommandHandler(
    ILogger<CreateApplicationCommandHandler> logger,
    IOpenIddictApplicationManager applicationManager)
    : IRequestHandler<CreateApplicationCommand, ApiResult>
{
    public async Task<ApiResult> Handle(CreateApplicationCommand request, CancellationToken cancellationToken)
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