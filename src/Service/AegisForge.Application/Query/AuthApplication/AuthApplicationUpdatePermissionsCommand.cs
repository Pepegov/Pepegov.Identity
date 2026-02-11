using MediatR;
using Pepegov.MicroserviceFramework.ApiResults;

namespace AegisForge.Application.Query.AuthApplication;

public record AuthApplicationUpdatePermissionsCommand(string ClientId, List<string> Permissions) : IRequest<ApiResult>;