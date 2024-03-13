using System.Net;
using MediatR;
using Microsoft.EntityFrameworkCore;
using OpenIddict.Abstractions;
using OpenIddict.Core;
using OpenIddict.EntityFrameworkCore.Models;
using Pepegov.Identity.PL.Endpoints.Application.Handlers.Queries;
using Pepegov.MicroserviceFramework.ApiResults;
using Pepegov.MicroserviceFramework.Data.Exceptions;

namespace Pepegov.Identity.PL.Endpoints.Application.Handlers;

public class GetApplicationRequestHandler : IRequestHandler<GetApplicationRequest, ApiResult<OpenIddictEntityFrameworkCoreApplication<Guid>>>
{
    private readonly ILogger<GetApplicationRequestHandler> _logger;
    private readonly IOpenIddictApplicationManager _applicationManager;

    public GetApplicationRequestHandler(ILogger<GetApplicationRequestHandler> logger, IOpenIddictApplicationManager applicationManager)
    {
        _logger = logger;
        _applicationManager = applicationManager;
    }

    public async Task<ApiResult<OpenIddictEntityFrameworkCoreApplication<Guid>>> Handle(GetApplicationRequest request, CancellationToken cancellationToken)
    {
        var application = await _applicationManager.FindByClientIdAsync(request.ClientId, cancellationToken);
        if (application is null)
        {
            return new ApiResult<OpenIddictEntityFrameworkCoreApplication<Guid>>(HttpStatusCode.NotFound, new MicroserviceNotFoundException($"Application by ClientId {request.ClientId} not found"));
        }

        _logger.LogInformation($"Application by id {request.ClientId} is successfully deleted");
        var result = new ApiResult<OpenIddictEntityFrameworkCoreApplication<Guid>>((OpenIddictEntityFrameworkCoreApplication<Guid>)application, HttpStatusCode.OK);
        //IOpenIddictAuthorizationManager
        return result;
    }
}