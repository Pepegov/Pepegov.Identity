using AegisForge.Application.Query.Permission;
using AegisForge.Domain.Entity;
using MediatR;
using Microsoft.Extensions.Logging;
using Pepegov.MicroserviceFramework.ApiResults;
using Pepegov.MicroserviceFramework.Data.Exceptions;
using Pepegov.UnitOfWork;
using Pepegov.UnitOfWork.EntityFramework;

namespace AegisForge.Application.Handler.Permission;

public class PermissionUpdateCommandHandler(
    ILogger<PermissionUpdateCommandHandler> logger,
    IUnitOfWorkManager unitOfWork)
    : IRequestHandler<PermissionUpdateCommand, ApiResult>
{
    public async Task<ApiResult> Handle(PermissionUpdateCommand request, CancellationToken cancellationToken)
    {
        var result = new ApiResult();
        var permissionRepository = unitOfWork.GetInstance<IUnitOfWorkEntityFrameworkInstance>().GetRepository<ApplicationPermission>();
        
        var permission = await permissionRepository.GetFirstOrDefaultAsync(
            predicate: x => x.ApplicationPermissionId == request.Id,
            disableTracking: false, cancellationToken: cancellationToken);
        if (permission is null)
        {
            var message = $"permission by id {request.Id} not found";
            result.AddExceptions(new MicroserviceNotFoundException(message));
            logger.LogError(message);
            return result;
        }
        permissionRepository.Update(permission);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return result;
    }
}