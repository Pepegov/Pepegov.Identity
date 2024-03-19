using System.Net;
using AutoMapper;
using MediatR;
using OpenIddict.Abstractions;
using OpenIddict.EntityFrameworkCore.Models;
using Pepegov.Identity.PL.Endpoints.Application.Queries;
using Pepegov.Identity.PL.Endpoints.Application.ViewModel;
using Pepegov.MicroserviceFramework.ApiResults;
using Pepegov.MicroserviceFramework.Data.Exceptions;

namespace Pepegov.Identity.PL.Endpoints.Application.Handlers;

public class GetApplicationViewModelRequestHandler : IRequestHandler<GetApplicationViewModelRequest, ApiResult<ApplicationViewModel>>
{
    private readonly ILogger<GetApplicationViewModelRequestHandler> _logger;
    private readonly IOpenIddictApplicationManager _applicationManager;
    private readonly IMapper _mapper;

    public GetApplicationViewModelRequestHandler(ILogger<GetApplicationViewModelRequestHandler> logger, IOpenIddictApplicationManager applicationManager, IMapper mapper)
    {
        _logger = logger;
        _applicationManager = applicationManager;
        _mapper = mapper;
    }

    public async Task<ApiResult<ApplicationViewModel>> Handle(GetApplicationViewModelRequest request, CancellationToken cancellationToken)
    {
        var application = await _applicationManager.FindByClientIdAsync(request.ClientId, cancellationToken);
        if (application is null)
        {
            return new ApiResult<ApplicationViewModel>(HttpStatusCode.NotFound, new MicroserviceNotFoundException($"Application by ClientId {request.ClientId} not found"));
        }
        
        if (application is not OpenIddictEntityFrameworkCoreApplication<Guid>)
        {
            var message = $"The received application does not match the actual one ({typeof(OpenIddictEntityFrameworkCoreApplication<Guid>)})";
            _logger.LogCritical(message);
            return new ApiResult<ApplicationViewModel>(HttpStatusCode.InternalServerError, new MicroserviceMappingException(message));
        }

        var model = _mapper.Map<ApplicationViewModel>((OpenIddictEntityFrameworkCoreApplication<Guid>)application);
        return new ApiResult<ApplicationViewModel>(model, HttpStatusCode.OK);
        //IOpenIddictAuthorizationManager
    }
}