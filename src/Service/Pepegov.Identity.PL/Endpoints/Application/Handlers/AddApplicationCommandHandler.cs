using System.Net;
using MediatR;
using OpenIddict.Abstractions;
using Pepegov.Identity.PL.Definitions.OpenIddictClientsSeeting;
using Pepegov.Identity.PL.Endpoints.Application.Handlers.Queries;
using Pepegov.MicroserviceFramework.ApiResults;
using Pepegov.MicroserviceFramework.Exceptions;

namespace Pepegov.Identity.PL.Endpoints.Application.Handlers;

public class AddApplicationCommandHandler : IRequestHandler<AddApplicationCommand, ApiResult>
{
    private readonly ILogger<AddApplicationCommandHandler> _logger;
    private readonly IOpenIddictApplicationManager _applicationManager;
    
    public AddApplicationCommandHandler(ILogger<AddApplicationCommandHandler> logger, IOpenIddictApplicationManager applicationManager)
    {
        _logger = logger;
        _applicationManager = applicationManager;
    }

    public async Task<ApiResult> Handle(AddApplicationCommand request, CancellationToken cancellationToken)
    {
        if (await _applicationManager.FindByClientIdAsync(request.ClientId, cancellationToken) is not null) //if the client exist then dont add it
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

        await _applicationManager.CreateAsync(client, cancellationToken);

        _logger.LogInformation($"Application DisplayName: {request.DisplayName} | ClientId {request.ClientId} successfully created");
        return new ApiResult(HttpStatusCode.OK);
    }
}