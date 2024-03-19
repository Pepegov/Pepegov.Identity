using MediatR;
using Pepegov.MicroserviceFramework.ApiResults;

namespace Pepegov.Identity.PL.Endpoints.Application.Queries;

public class UpdateApplicationPermissionsCommand : IRequest<ApiResult>
{
    public List<string> Permissions { get; set; }
    public string ClientId { get; set; }
    
    public UpdateApplicationPermissionsCommand() {}

    public UpdateApplicationPermissionsCommand(string clientId, List<string> permissions)
    {
        ClientId = clientId;
        Permissions = permissions;
    }
}