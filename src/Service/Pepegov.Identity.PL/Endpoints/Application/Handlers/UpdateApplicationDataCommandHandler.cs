using System.Net;
using System.Text.Json;
using MediatR;
using OpenIddict.Abstractions;
using OpenIddict.EntityFrameworkCore.Models;
using Pepegov.Identity.PL.Endpoints.Application.Queries;
using Pepegov.MicroserviceFramework.ApiResults;
using Pepegov.MicroserviceFramework.Data.Exceptions;

namespace Pepegov.Identity.PL.Endpoints.Application.Handlers;

public class UpdateApplicationDataCommandHandler : IRequestHandler<UpdateApplicationDataCommand, ApiResult>
{
    private readonly ILogger<UpdateApplicationDataCommandHandler> _logger;
    private readonly IOpenIddictApplicationManager _applicationManager;
    
    public UpdateApplicationDataCommandHandler(ILogger<UpdateApplicationDataCommandHandler> logger, IOpenIddictApplicationManager applicationManager)
    {
        _logger = logger;
        _applicationManager = applicationManager;
    }

    public async Task<ApiResult> Handle(UpdateApplicationDataCommand request, CancellationToken cancellationToken)
    {
        //find application    
        var applicationObj =  await _applicationManager.FindByClientIdAsync(request.TargetClientId, cancellationToken);
        if (applicationObj is null)
        {
            return new ApiResult(HttpStatusCode.NotFound, new MicroserviceNotFoundException($"Application by ClientId {request.TargetClientId} not found"));
        }
        var application = (OpenIddictEntityFrameworkCoreApplication<Guid>)applicationObj;

        if (request.UpdateModel.RedirectUris is null)
        {
            application.RedirectUris = null;
        }
        else
        {
            application.RedirectUris = JsonSerializer.Serialize<List<string>>(request.UpdateModel.RedirectUris);
        }

        application.ClientId = request.UpdateModel.ClientId;
        application.DisplayName = request.UpdateModel.DisplayName;
        application.ConsentType = request.UpdateModel.ConsentType;
        application.Type = request.UpdateModel.Type;
        
        await _applicationManager.UpdateAsync(application, cancellationToken);
        _logger.LogInformation($"Successful update application {application.DisplayName} | ClientId:{application.ClientId} | Type:{application.Type} | ConsentType:{application.ConsentType} | RedirectUris:{application.RedirectUris}");
        return new ApiResult(HttpStatusCode.OK);
    }
}