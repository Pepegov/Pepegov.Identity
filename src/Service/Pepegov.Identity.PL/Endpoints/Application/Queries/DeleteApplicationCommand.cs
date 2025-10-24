using MediatR;
using Pepegov.MicroserviceFramework.ApiResults;

namespace Pepegov.Identity.PL.Endpoints.Application.Queries;

public record DeleteApplicationCommand(string ClientId) : IRequest<ApiResult>;
