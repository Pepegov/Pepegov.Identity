using MediatR;
using Pepegov.MicroserviceFramework.ApiResults;
using TelecomService.QRON.ID.Identity.Domain.Enum;

namespace AegisForge.Application.Query.Session;

public record SessionStatusRequest(string SessionState) : IRequest<ApiResult<SessionStatusType>>;