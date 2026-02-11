using System.Linq.Expressions;
using System.Net;
using AegisForge.Application.Query.Session;
using AegisForge.Domain.Aggregate;
using AegisForge.Infrastructure.Extension;
using MediatR;
using Microsoft.Extensions.Logging;
using Pepegov.MicroserviceFramework.ApiResults;
using Pepegov.MicroserviceFramework.Data.Exceptions;
using Pepegov.MicroserviceFramework.Exceptions;
using Pepegov.UnitOfWork;
using Pepegov.UnitOfWork.EntityFramework;
using TelecomService.QRON.ID.Identity.Domain.Enum;

namespace AegisForge.Application.Handler.Session;

public class SessionTerminateCommandHandler(IUnitOfWorkManager unitOfWorkManager, ILogger<SessionTerminateCommandHandler> logger) : IRequestHandler<SessionTerminateCommand, ApiResult>
{
    public async Task<ApiResult> Handle(SessionTerminateCommand request, CancellationToken cancellationToken)
    {
        var unitOfWorkEntityFrameworkInstance = unitOfWorkManager.GetInstance<IUnitOfWorkEntityFrameworkInstance>();
        var sessionRepository = unitOfWorkEntityFrameworkInstance.GetRepository<ApplicationSession>();

        Expression<Func<ApplicationSession, bool>> predicate = x => x.Id == request.Id;
        if (request.UserId != null)
        {
            predicate.AndAlso(x => x.ApplicationUserId == request.UserId);
        }

        var session = await sessionRepository.GetFirstOrDefaultAsync(
            predicate: predicate,
            cancellationToken: cancellationToken);
        if (session is null)
        {
            return new ApiResult(HttpStatusCode.NotFound, new MicroserviceNotFoundException("Session not found"));
        }

        if (session.SessionStatusType != SessionStatusType.Revoked)
        {
            session.SessionStatusType = SessionStatusType.Revoked;
            sessionRepository.Update(session);
            await unitOfWorkEntityFrameworkInstance.SaveChangesAsync(cancellationToken);
            if (!unitOfWorkEntityFrameworkInstance.LastSaveChangesResult.IsOk)
            {
                var exceptionMessage = $"Unable to save changes to database | errors: " +
                                       (unitOfWorkEntityFrameworkInstance.LastSaveChangesResult.Exception?.Message ??
                                        unitOfWorkEntityFrameworkInstance.LastSaveChangesResult.Exception?.InnerException?.Message ??
                                        "Unable to save changes to database");
                logger.LogError(exceptionMessage);
                throw new MicroserviceDatabaseException(exceptionMessage);
            }   
        }
        
        return new ApiResult(HttpStatusCode.OK);
    }
}