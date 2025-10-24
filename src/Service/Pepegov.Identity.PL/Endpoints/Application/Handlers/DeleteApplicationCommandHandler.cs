using System.Net;
using MediatR;
using OpenIddict.Abstractions;
using Pepegov.Identity.PL.Endpoints.Application.Queries;
using Pepegov.MicroserviceFramework.ApiResults;
using Pepegov.MicroserviceFramework.Data.Exceptions;
using Pepegov.MicroserviceFramework.Exceptions;

namespace Pepegov.Identity.PL.Endpoints.Application.Handlers;

public class DeleteApplicationCommandHandler(
    ILogger<DeleteApplicationCommandHandler> logger,
    IOpenIddictApplicationManager applicationManager)
    : IRequestHandler<DeleteApplicationCommand, ApiResult>
{
    public async Task<ApiResult> Handle(DeleteApplicationCommand request, CancellationToken cancellationToken)
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