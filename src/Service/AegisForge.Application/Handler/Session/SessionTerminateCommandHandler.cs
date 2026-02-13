using System.Net;
using AegisForge.Application.Query.Session;
using AegisForge.Application.Service.Interfaces;
using MediatR;
using Pepegov.MicroserviceFramework.ApiResults;
using Pepegov.MicroserviceFramework.Data.Exceptions;

namespace AegisForge.Application.Handler.Session;

public class SessionTerminateCommandHandler(ISessionService sessionService) : IRequestHandler<SessionTerminateCommand, ApiResult>
{
    public async Task<ApiResult> Handle(SessionTerminateCommand request, CancellationToken cancellationToken)
    {
        try
        {
            await sessionService.TerminateSession(request.Id, request.UserId, cancellationToken);
            return new ApiResult(HttpStatusCode.OK);
        }
        catch (MicroserviceNotFoundException e)
        {
            return new ApiResult(HttpStatusCode.NotFound, e);
        }
    }
}