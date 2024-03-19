using MediatR;
using Pepegov.Identity.PL.Endpoints.Application.ViewModel;
using Pepegov.MicroserviceFramework.ApiResults;

namespace Pepegov.Identity.PL.Endpoints.Application.Queries;

public class GetApplicationViewModelRequest : IRequest<ApiResult<ApplicationViewModel>>
{
    public GetApplicationViewModelRequest() {}

    public GetApplicationViewModelRequest(string clientId)
    {
        ClientId = clientId;
    }
    
    public string ClientId { get; set; } = null!;
}