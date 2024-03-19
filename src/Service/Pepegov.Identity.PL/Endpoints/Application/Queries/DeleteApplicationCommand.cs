using MediatR;
using Pepegov.MicroserviceFramework.ApiResults;

namespace Pepegov.Identity.PL.Endpoints.Application.Queries;

public class DeleteApplicationCommand : IRequest<ApiResult>
{
    public string ClientId { get; set; } = null!;
    public DeleteApplicationCommand() {}


    public DeleteApplicationCommand(string clientId)
    {
        ClientId = clientId;
    }
}