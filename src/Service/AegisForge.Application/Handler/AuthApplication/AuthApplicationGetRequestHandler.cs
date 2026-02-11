using System.Net;
using AegisForge.Application.Dto;
using AegisForge.Application.Query.AuthApplication;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using OpenIddict.Abstractions;
using OpenIddict.EntityFrameworkCore.Models;
using Pepegov.MicroserviceFramework.ApiResults;
using Pepegov.MicroserviceFramework.Data.Exceptions;

namespace AegisForge.Application.Handler.AuthApplication;

public class AuthApplicationGetRequestHandler(
    ILogger<AuthApplicationGetRequestHandler> logger,
    IOpenIddictApplicationManager applicationManager,
    IMapper mapper)
    : IRequestHandler<AuthApplicationGetRequest, ApiResult<ApplicationViewModel>>
{
    public async Task<ApiResult<ApplicationViewModel>> Handle(AuthApplicationGetRequest request, CancellationToken cancellationToken)
    {
        var application = await applicationManager.FindByClientIdAsync(request.ClientId, cancellationToken);
        if (application is null)
        {
            return new ApiResult<ApplicationViewModel>(HttpStatusCode.NotFound, new MicroserviceNotFoundException($"Application by ClientId {request.ClientId} not found"));
        }
        
        if (application is not OpenIddictEntityFrameworkCoreApplication<Guid> coreApplication)
        {
            var message = $"The received application does not match the actual one ({typeof(OpenIddictEntityFrameworkCoreApplication<Guid>)})";
            logger.LogCritical(message);
            return new ApiResult<ApplicationViewModel>(HttpStatusCode.InternalServerError, new MicroserviceMappingException(message));
        }

        var model = mapper.Map<ApplicationViewModel>(coreApplication);
        return new ApiResult<ApplicationViewModel>(model, HttpStatusCode.OK);
        //IOpenIddictAuthorizationManager
    }
}