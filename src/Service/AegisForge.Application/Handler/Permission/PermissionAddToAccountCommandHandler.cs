using AegisForge.Application.Query.Permission;
using AegisForge.Domain.Entity;
using AegisForge.Domain.Models.Identity;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Pepegov.MicroserviceFramework.ApiResults;
using Pepegov.MicroserviceFramework.Data.Exceptions;
using Pepegov.UnitOfWork;
using Pepegov.UnitOfWork.EntityFramework;

namespace AegisForge.Application.Handler.Permission;

public class PermissionAddToAccountCommandHandler(
    ILogger<PermissionUpdateCommandHandler> logger,
    IUnitOfWorkManager unitOfWork)
    : IRequestHandler<PermissionAddToAccountCommand, ApiResult>
{
    public async Task<ApiResult> Handle(PermissionAddToAccountCommand request, CancellationToken cancellationToken)
    {
        var result = new ApiResult();
        var unitOfWorkInstance = unitOfWork.GetInstance<IUnitOfWorkEntityFrameworkInstance>();
        var permissionRepository = unitOfWorkInstance.GetRepository<ApplicationPermission>();
        var profileRepository = unitOfWorkInstance.GetRepository<ApplicationUserProfile>();

        var profile = await profileRepository.GetFirstOrDefaultAsync(
            include: x => x.Include(i => i.Permissions)!,
            predicate: x => x.Id == request.Model.ProfileId,
            disableTracking: false, cancellationToken: cancellationToken);
        if (profile is null)
        {
            var message = $"profile by id {request.Model.ProfileId} not found";
            result.AddExceptions(new MicroserviceNotFoundException(message));
            logger.LogError(message);
            return result;
        }

        var permission = await permissionRepository.GetFirstOrDefaultAsync(predicate: x =>
                x.ApplicationPermissionId == request.Model.PermissionId,
            disableTracking: false, cancellationToken: cancellationToken);
        if (permission is null)
        {
            var message = $"permission by id {request.Model.PermissionId} not found";
            result.AddExceptions(new MicroserviceNotFoundException(message));
            logger.LogError(message);
            return result;
        }

        profile.Permissions ??= [];
        profile.Permissions.Add(permission);
        profileRepository.Update(profile);
        result.AddMetadata(new Metadata($"Profile {profile.Id} is updated", MetadataType.Success));
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return result;
    }
}