using MediatR;
using Pepegov.MicroserviceFramework.ApiResults;

namespace AegisForge.Application.Query.Session;

public record SessionTerminateCommand(Guid Id, Guid? UserId = null) :  IRequest<ApiResult>;