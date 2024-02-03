using MediatR;
using Microsoft.EntityFrameworkCore;
using Pepegov.Identity.DAL.Models.Identity;
using Pepegov.Identity.PL.Endpoints.Permission.ViewModel;
using Pepegov.MicroserviceFramework.ApiResults;
using Pepegov.MicroserviceFramework.Data.Exceptions;
using Pepegov.UnitOfWork;
using Pepegov.UnitOfWork.EntityFramework;

namespace Pepegov.Identity.PL.Endpoints.Permission.Queries;

public record class RemovePermissionFromAccountCommand : IRequest<ApiResult>
{
    public ProfilePermissionViewModel Model { get; set; }
    
    public RemovePermissionFromAccountCommand(ProfilePermissionViewModel model)
    {
        Model = model;
    }
}

public class RemovePermissionFromAccountCommandHandler : IRequestHandler<RemovePermissionFromAccountCommand, ApiResult>
{
    private readonly ILogger<RemovePermissionFromAccountCommandHandler> _logger;
    private readonly IUnitOfWorkManager _unitOfWork;
    
    public RemovePermissionFromAccountCommandHandler(ILogger<RemovePermissionFromAccountCommandHandler> logger, IUnitOfWorkManager unitOfWork)
    {
        _logger = logger;
        _unitOfWork = unitOfWork;
    }

    public async Task<ApiResult> Handle(RemovePermissionFromAccountCommand request, CancellationToken cancellationToken)
    {
        var result = new ApiResult();
        var unitOfWorkInstance = _unitOfWork.GetInstance<IUnitOfWorkEntityFrameworkInstance>();
        var permissionRepository = unitOfWorkInstance.GetRepository<ApplicationPermission>();
        var profileRepository = unitOfWorkInstance.GetRepository<ApplicationUserProfile>();

        var profile = await profileRepository.GetFirstOrDefaultAsync(
            include: x => x.Include(i => i.Permissions),
            predicate: x => x.Id == request.Model.ProfileId,
            disableTracking: false);
        if (profile is null)
        {
            var message = $"profile by id {request.Model.ProfileId} not found";
            result.AddExceptions(new MicroserviceNotFoundException(message));
            _logger.LogError(message);
            return result;
        }

        var permission = await permissionRepository.GetFirstOrDefaultAsync(predicate: x =>
                x.ApplicationPermissionId == request.Model.PermissionId,
            disableTracking: false);
        if (permission is null)
        {
            var message = $"permission by id {request.Model.PermissionId} not found";
            result.AddExceptions(new MicroserviceNotFoundException(message));
            _logger.LogError(message);
            return result;
        }
        
        var answer = profile.Permissions!.Remove(permission);
        if (!answer)
        {
            var message = $"permission {request.Model.PermissionId} dont remove in profile {request.Model.ProfileId}";
            result.AddExceptions(new MicroserviceInvalidOperationException(message));
            _logger.LogError(message);
            return result;
        }
        
        profileRepository.Update(profile);
        result.AddMetadata(new Metadata($"Profile {profile.Id} is updated", MetadataType.Success));
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return result;
    }
}