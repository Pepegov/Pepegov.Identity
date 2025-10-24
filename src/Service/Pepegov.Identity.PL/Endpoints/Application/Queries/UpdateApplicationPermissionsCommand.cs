using MediatR;
using Pepegov.MicroserviceFramework.ApiResults;

namespace Pepegov.Identity.PL.Endpoints.Application.Queries;

public record UpdateApplicationPermissionsCommand(string ClientId, List<string> Permissions) : IRequest<ApiResult>;