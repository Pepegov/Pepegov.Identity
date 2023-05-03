using MediatR;
using MicroserviceOpenIddictTemplate.DAL.Models.Identity;
using MicroserviceOpenIddictTemplate.Identity.Endpoints.Permission.ViewModel;
using Microsoft.EntityFrameworkCore;
using Pepegov.MicroserviceFramerwork.Exceptions;
using Pepegov.MicroserviceFramerwork.Patterns.UnitOfWork;
using Pepegov.MicroserviceFramerwork.ResultWrapper;

namespace MicroserviceOpenIddictTemplate.Identity.Endpoints.Permission.Queries;

public record class RemoveInProfileRequest : IRequest<ResultWrapper<ProfilePermissionViewModel>>
{
    public ProfilePermissionViewModel Model { get; set; }
    
    public RemoveInProfileRequest(ProfilePermissionViewModel model)
    {
        Model = model;
    }
}

public class RemoveInProfileRequestHandler : IRequestHandler<RemoveInProfileRequest, ResultWrapper<ProfilePermissionViewModel>>
{
    private readonly ILogger<RemoveInProfileRequestHandler> _logger;
    private readonly IUnitOfWork _unitOfWork;
    
    public RemoveInProfileRequestHandler(ILogger<RemoveInProfileRequestHandler> logger, IUnitOfWork unitOfWork)
    {
        _logger = logger;
        _unitOfWork = unitOfWork;
    }

    public async Task<ResultWrapper<ProfilePermissionViewModel>> Handle(RemoveInProfileRequest request, CancellationToken cancellationToken)
    {
        var result = new ResultWrapper<ProfilePermissionViewModel>();
        var permissionReposytory = _unitOfWork.GetRepository<ApplicationPermission>();
        var profileReposytory = _unitOfWork.GetRepository<ApplicationUserProfile>();

        var profile = await profileReposytory.GetFirstOrDefaultAsync(
            include: x => x.Include(i => i.Permissions),
            predicate: x => x.Id == request.Model.ProfileId,
            disableTracking: false);
        if (profile is null)
        {
            var message = $"profile by id {request.Model.ProfileId} not found";
            result.AddException(new MicroserviceNotFoundException(message));
            _logger.LogError(message);
            return result;
        }

        var permission = await permissionReposytory.GetFirstOrDefaultAsync(predicate: x =>
                x.ApplicationPermissionId == request.Model.PermissionId,
            disableTracking: false);
        if (permission is null)
        {
            var message = $"permission by id {request.Model.PermissionId} not found";
            result.AddException(new MicroserviceNotFoundException(message));
            _logger.LogError(message);
            return result;
        }
        
        var answer = profile.Permissions.Remove(permission);
        if (!answer)
        {
            var message = $"permission {request.Model.PermissionId} dont remove in profile {request.Model.ProfileId}";
            result.AddException(new MicroserviceInvalidOperationException(message));
            _logger.LogError(message);
            return result;
        }
        
        profileReposytory.Update(profile);
        result.Message = request.Model;
        result.AddMetadatas(new Metadata($"Profile {profile.Id} is updated", MetadataType.Success));
        await _unitOfWork.SaveChangesAsync();

        return result;
    }
}