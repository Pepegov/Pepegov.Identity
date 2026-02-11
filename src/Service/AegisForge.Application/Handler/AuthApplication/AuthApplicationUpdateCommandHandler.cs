using System.Net;
using System.Text.Json;
using AegisForge.Application.Query.AuthApplication;
using MediatR;
using Microsoft.Extensions.Logging;
using OpenIddict.Abstractions;
using OpenIddict.EntityFrameworkCore.Models;
using Pepegov.MicroserviceFramework.ApiResults;
using Pepegov.MicroserviceFramework.Data.Exceptions;

namespace AegisForge.Application.Handler.AuthApplication;

public class AuthApplicationUpdateCommandHandler(
    ILogger<AuthApplicationUpdateCommandHandler> logger,
    IOpenIddictApplicationManager applicationManager)
    : IRequestHandler<AuthApplicationUpdateCommand, ApiResult>
{
    public async Task<ApiResult> Handle(AuthApplicationUpdateCommand request, CancellationToken cancellationToken)
    {
        //find application    
        var applicationObj =  await applicationManager.FindByClientIdAsync(request.TargetClientId, cancellationToken);
        if (applicationObj is null)
        {
            return new ApiResult(HttpStatusCode.NotFound, new MicroserviceNotFoundException($"Application by ClientId {request.TargetClientId} not found"));
        }
        var application = (OpenIddictEntityFrameworkCoreApplication<Guid>)applicationObj;

        application.RedirectUris = request.UpdateModel.RedirectUris is null ? null : JsonSerializer.Serialize(request.UpdateModel.RedirectUris);

        application.ClientId = request.UpdateModel.ClientId;
        application.DisplayName = request.UpdateModel.DisplayName;
        application.ConsentType = request.UpdateModel.ConsentType;
        application.ApplicationType = request.UpdateModel.Type;
        
        await applicationManager.UpdateAsync(application, cancellationToken);
        logger.LogInformation($"Successful update application {application.DisplayName} | ClientId:{application.ClientId} | Type:{application.ApplicationType} | ConsentType:{application.ConsentType} | RedirectUris:{application.RedirectUris}");
        return new ApiResult(HttpStatusCode.OK);
    }
}