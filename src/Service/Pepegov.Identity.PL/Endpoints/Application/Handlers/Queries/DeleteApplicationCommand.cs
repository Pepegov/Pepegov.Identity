using MediatR;
using Pepegov.MicroserviceFramework.ApiResults;

namespace Pepegov.Identity.PL.Endpoints.Application.Handlers.Queries;

public class DeleteApplicationCommand : IRequest<ApiResult>
{
    public string ClientId { get; set; } = null!;
}