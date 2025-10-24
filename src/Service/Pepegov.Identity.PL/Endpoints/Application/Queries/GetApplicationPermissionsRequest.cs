using MediatR;
using Pepegov.MicroserviceFramework.ApiResults;

namespace Pepegov.Identity.PL.Endpoints.Application.Queries;

public record GetApplicationPermissionsRequest(string ClientId) : IRequest<ApiResult<List<string>>?>;
