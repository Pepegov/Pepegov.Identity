using MediatR;
using OpenIddict.EntityFrameworkCore.Models;
using Pepegov.MicroserviceFramework.ApiResults;

namespace Pepegov.Identity.PL.Endpoints.Application.Handlers.Queries;

public class GetApplicationRequest : IRequest<ApiResult<OpenIddictEntityFrameworkCoreApplication<Guid>>>
{
    public GetApplicationRequest() {}

    public GetApplicationRequest(string clientId)
    {
        ClientId = clientId;
    }
    
    public string ClientId { get; set; } = null!;
}