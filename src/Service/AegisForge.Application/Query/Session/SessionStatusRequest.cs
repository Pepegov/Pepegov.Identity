using AegisForge.Domain.Enum;
using MediatR;
using Pepegov.MicroserviceFramework.ApiResults;

namespace AegisForge.Application.Query.Session;

public record SessionStatusRequest(string SessionState) : IRequest<ApiResult<SessionStatusType>>;