using System.Net;
using System.Text.Json;
using MediatR;
using OpenIddict.Abstractions;
using OpenIddict.EntityFrameworkCore.Models;
using Pepegov.Identity.PL.Endpoints.Application.Queries;
using Pepegov.MicroserviceFramework.ApiResults;
using Pepegov.MicroserviceFramework.Data.Exceptions;

namespace Pepegov.Identity.PL.Endpoints.Application.Handlers;

public class GetApplicationPermissionsRequestHandler : IRequestHandler<GetApplicationPermissionsRequest, ApiResult<List<string>?>>
{
    private readonly ILogger<GetApplicationPermissionsRequestHandler> _logger;
    private readonly IOpenIddictApplicationManager _applicationManager;

    public GetApplicationPermissionsRequestHandler(ILogger<GetApplicationPermissionsRequestHandler> logger, IOpenIddictApplicationManager applicationManager)
    {
        _logger = logger;
        _applicationManager = applicationManager;
    }

    public async Task<ApiResult<List<string>?>> Handle(GetApplicationPermissionsRequest request, CancellationToken cancellationToken)
    {
        var application = await _applicationManager.FindByClientIdAsync(request.ClientId, cancellationToken);
        if (application is null)
        {
            return new ApiResult<List<string>?>(HttpStatusCode.NotFound, new MicroserviceNotFoundException($"Application by ClientId {request.ClientId} not found"));
        }
        
        if (application is not OpenIddictEntityFrameworkCoreApplication<Guid>)
        {
            var message = $"The received application does not match the actual one ({typeof(OpenIddictEntityFrameworkCoreApplication<Guid>)})";
            _logger.LogCritical(message);
            return new ApiResult<List<string>?>(HttpStatusCode.InternalServerError, new MicroserviceMappingException(message));
        }
        
        List<string>? model = null;
        if (((OpenIddictEntityFrameworkCoreApplication<Guid>)application).Permissions is not null)
        {
            model = JsonSerializer.Deserialize<List<string>>(((OpenIddictEntityFrameworkCoreApplication<Guid>)application).Permissions!);
        }
        return new ApiResult<List<string>?>(model, HttpStatusCode.OK);   
    }
}