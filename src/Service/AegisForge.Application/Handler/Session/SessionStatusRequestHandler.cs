using System.Net;
using AegisForge.Application.Query;
using AegisForge.Application.Query.Session;
using AegisForge.Domain.Aggregate;
using MediatR;
using Pepegov.MicroserviceFramework.ApiResults;
using Pepegov.MicroserviceFramework.Data.Exceptions;
using Pepegov.UnitOfWork;
using Pepegov.UnitOfWork.EntityFramework;
using TelecomService.QRON.ID.Identity.Domain.Enum;

namespace AegisForge.Application.Handler.Session;

public class SessionStatusRequestHandler(IUnitOfWorkManager unitOfWorkManager) : IRequestHandler<SessionStatusRequest, ApiResult<SessionStatusType>>
{
    public async Task<ApiResult<SessionStatusType>> Handle(SessionStatusRequest request, CancellationToken cancellationToken)
    {
        var unitOfWorkEntityFrameworkInstance = unitOfWorkManager.GetInstance<IUnitOfWorkEntityFrameworkInstance>();
        var sessionRepository = unitOfWorkEntityFrameworkInstance.GetRepository<ApplicationSession>();
        var session = await sessionRepository.GetFirstOrDefaultAsync(
            predicate: x => x.SessionState == request.SessionState,
            cancellationToken: cancellationToken,
            disableTracking: true);

        if (session is null)
        {
            return new ApiResult<SessionStatusType>(HttpStatusCode.NotFound, new MicroserviceNotFoundException("Session not found"));
        }

        return new ApiResult<SessionStatusType>(session.SessionStatusType, HttpStatusCode.OK);
    }
}