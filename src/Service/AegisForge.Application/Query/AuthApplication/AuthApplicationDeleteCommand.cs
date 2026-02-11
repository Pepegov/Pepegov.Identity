using MediatR;
using Pepegov.MicroserviceFramework.ApiResults;

namespace AegisForge.Application.Query.AuthApplication;

public record AuthApplicationDeleteCommand(string ClientId) : IRequest<ApiResult>;
