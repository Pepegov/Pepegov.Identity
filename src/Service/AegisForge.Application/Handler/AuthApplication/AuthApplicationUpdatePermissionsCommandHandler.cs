using System.Net;
using System.Text.Json;
using AegisForge.Application.Query.AuthApplication;
using MediatR;
using Microsoft.Extensions.Logging;
using OpenIddict.Abstractions;
using OpenIddict.EntityFrameworkCore.Models;
using Pepegov.MicroserviceFramework.ApiResults;
using Pepegov.MicroserviceFramework.Data.Exceptions;
using Pepegov.MicroserviceFramework.Infrastructure.Extensions;

namespace AegisForge.Application.Handler.AuthApplication;

public class UpdatePermissionsToApplicationCommandHandler(
    ILogger<UpdatePermissionsToApplicationCommandHandler> logger,
    IOpenIddictApplicationManager applicationManager)
    : IRequestHandler<AuthApplicationUpdatePermissionsCommand, ApiResult>
{
    public async Task<ApiResult> Handle(AuthApplicationUpdatePermissionsCommand request, CancellationToken cancellationToken)
    {
        //check allowed prefixes
        var checkResult = CheckPrefixes(request);
        if (checkResult is not null)
            return checkResult;
            
        //find application    
        var applicationObj =  await applicationManager.FindByClientIdAsync(request.ClientId, cancellationToken);
        if (applicationObj is null)
        {
            return new ApiResult(HttpStatusCode.NotFound, new MicroserviceNotFoundException($"Application by ClientId {request.ClientId} not found"));
        }
        var application = (OpenIddictEntityFrameworkCoreApplication<Guid>)applicationObj;
        
        application.Permissions = JsonSerializer.Serialize(request.Permissions);
        
        await applicationManager.UpdateAsync(application, cancellationToken);
        logger.LogInformation($"Add permission {application.Permissions} to application {application.DisplayName}");
        return new ApiResult(HttpStatusCode.OK);
    }
    
    private ApiResult? CheckPrefixes(AuthApplicationUpdatePermissionsCommand request)
    {
        var prefixes = typeof(OpenIddictConstants.Permissions.Prefixes).GetAllPublicConstantValues<string>();
        foreach (var currentPrefix in request.Permissions.Select(permission => permission.Split(":")[0]+":"))
        {
            if (currentPrefix is null)
            {
                var message = $"No prefix for application permission. clientID {request.ClientId}";
                logger.LogWarning(message);
                return new ApiResult(HttpStatusCode.BadRequest, new MicroserviceInvalidOperationException(message));
            }

            if (prefixes.All(x => x != currentPrefix))
            {
                var message = $"The permission prefix {currentPrefix} is not supported for all applications";
                logger.LogWarning(message);
                return new ApiResult(HttpStatusCode.Conflict, new MicroserviceInvalidOperationException(message));
            }
        }

        return null;
    }
}