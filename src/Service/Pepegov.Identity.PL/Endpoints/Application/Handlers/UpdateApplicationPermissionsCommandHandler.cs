using System.Net;
using System.Text.Json;
using MediatR;
using OpenIddict.Abstractions;
using OpenIddict.EntityFrameworkCore.Models;
using Pepegov.Identity.PL.Endpoints.Application.Queries;
using Pepegov.MicroserviceFramework.ApiResults;
using Pepegov.MicroserviceFramework.Data.Exceptions;
using Pepegov.MicroserviceFramework.Exceptions;
using Pepegov.MicroserviceFramework.Infrastructure.Extensions;

namespace Pepegov.Identity.PL.Endpoints.Application.Handlers;

public class UpdatePermissionsToApplicationCommandHandler : IRequestHandler<UpdateApplicationPermissionsCommand, ApiResult>
{
    private ILogger<UpdatePermissionsToApplicationCommandHandler> _logger;
    private readonly IOpenIddictApplicationManager _applicationManager;

    public UpdatePermissionsToApplicationCommandHandler(ILogger<UpdatePermissionsToApplicationCommandHandler> logger, IOpenIddictApplicationManager applicationManager)
    {
        _logger = logger;
        _applicationManager = applicationManager;
    }

    public async Task<ApiResult> Handle(UpdateApplicationPermissionsCommand request, CancellationToken cancellationToken)
    {
        //check allowed prefixes
        var checkResult = CheckPrefixes(request);
        if (checkResult is not null)
            return checkResult;
            
        //find application    
        var applicationObj =  await _applicationManager.FindByClientIdAsync(request.ClientId, cancellationToken);
        if (applicationObj is null)
        {
            return new ApiResult(HttpStatusCode.NotFound, new MicroserviceNotFoundException($"Application by ClientId {request.ClientId} not found"));
        }
        var application = (OpenIddictEntityFrameworkCoreApplication<Guid>)applicationObj;
        
        application.Permissions = JsonSerializer.Serialize<List<string>>(request.Permissions);
        
        await _applicationManager.UpdateAsync(application, cancellationToken);
        _logger.LogInformation($"Add permission {application.Permissions} to application {application.DisplayName}");
        return new ApiResult(HttpStatusCode.OK);
    }
    
    private ApiResult? CheckPrefixes(UpdateApplicationPermissionsCommand request)
    {
        var prefixes = typeof(OpenIddictConstants.Permissions.Prefixes).GetAllPublicConstantValues<string>();
        foreach (var permission in request.Permissions)
        {
            var currentPrefix = permission.Split(":")[0]+":";
            if (currentPrefix is null)
            {
                var message = $"No prefix for application permission. clientID {request.ClientId}";
                _logger.LogWarning(message);
                return new ApiResult(HttpStatusCode.BadRequest, new MicroserviceInvalidOperationException(message));
            }

            if (prefixes.All(x => x != currentPrefix))
            {
                var message = $"The permission prefix {currentPrefix} is not supported for all applications";
                _logger.LogWarning(message);
                return new ApiResult(HttpStatusCode.Conflict, new MicroserviceInvalidOperationException(message));
            }
        }

        return null;
    }
}