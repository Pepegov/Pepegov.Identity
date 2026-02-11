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

public class AuthApplicationGetPermissionsRequestHandler(
    ILogger<AuthApplicationGetPermissionsRequestHandler> logger,
    IOpenIddictApplicationManager applicationManager)
    : IRequestHandler<AuthApplicationGetPermissionsRequest, ApiResult<List<string>?>>
{
    public async Task<ApiResult<List<string>?>> Handle(AuthApplicationGetPermissionsRequest request, CancellationToken cancellationToken)
    {
        var application = await applicationManager.FindByClientIdAsync(request.ClientId, cancellationToken);
        if (application is null)
        {
            return new ApiResult<List<string>?>(HttpStatusCode.NotFound, new MicroserviceNotFoundException($"Application by ClientId {request.ClientId} not found"));
        }
        
        if (application is not OpenIddictEntityFrameworkCoreApplication<Guid> coreApplication)
        {
            var message = $"The received application does not match the actual one ({typeof(OpenIddictEntityFrameworkCoreApplication<Guid>)})";
            logger.LogCritical(message);
            return new ApiResult<List<string>?>(HttpStatusCode.InternalServerError, new MicroserviceMappingException(message));
        }
        
        List<string>? model = null;
        if (coreApplication.Permissions is not null)
        {
            model = JsonSerializer.Deserialize<List<string>>(coreApplication.Permissions!);
        }
        return new ApiResult<List<string>?>(model, HttpStatusCode.OK);   
    }
}