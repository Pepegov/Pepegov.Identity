using MediatR;
using Pepegov.MicroserviceFramework.ApiResults;

namespace AegisForge.Application.Query.AuthApplication;

public record AuthApplicationGetPermissionsRequest(string ClientId) : IRequest<ApiResult<List<string>>?>;
