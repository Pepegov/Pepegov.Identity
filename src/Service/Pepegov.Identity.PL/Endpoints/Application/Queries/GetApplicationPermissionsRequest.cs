using MediatR;
using Pepegov.MicroserviceFramework.ApiResults;

namespace Pepegov.Identity.PL.Endpoints.Application.Queries;

public class GetApplicationPermissionsRequest : IRequest<ApiResult<List<string>>?>
{
    public string ClientId { get; set; } 
    
    public GetApplicationPermissionsRequest() {}

    public GetApplicationPermissionsRequest(string clientId)
    {
        ClientId = clientId;
    }
}