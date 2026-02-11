using System.Net;
using AegisForge.Application.Query.AuthApplication;
using MediatR;
using Microsoft.Extensions.Logging;
using OpenIddict.Abstractions;
using Pepegov.MicroserviceFramework.ApiResults;
using Pepegov.MicroserviceFramework.Data.Exceptions;

namespace AegisForge.Application.Handler.AuthApplication;

public class AuthApplicationDeleteCommandHandler(
    ILogger<AuthApplicationDeleteCommandHandler> logger,
    IOpenIddictApplicationManager applicationManager)
    : IRequestHandler<AuthApplicationDeleteCommand, ApiResult>
{
    public async Task<ApiResult> Handle(AuthApplicationDeleteCommand request, CancellationToken cancellationToken)
    {
        var application = await applicationManager.FindByClientIdAsync(request.ClientId, cancellationToken);
        if (application is null)
        {
            return new ApiResult(HttpStatusCode.NotFound, new MicroserviceNotFoundException($"Application by ClientId {request.ClientId} not found"));
        }
        
        await applicationManager.DeleteAsync(application, cancellationToken);
        logger.LogInformation($"Application by id {request.ClientId} is successfully deleted");
        return new ApiResult(HttpStatusCode.OK);
    }
}