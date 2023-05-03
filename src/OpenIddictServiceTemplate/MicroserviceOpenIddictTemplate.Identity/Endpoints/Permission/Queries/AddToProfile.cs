using MediatR;
using MicroserviceOpenIddictTemplate.DAL.Models.Identity;
using MicroserviceOpenIddictTemplate.Identity.Endpoints.Permission.ViewModel;
using Microsoft.EntityFrameworkCore;
using Pepegov.MicroserviceFramerwork.Exceptions;
using Pepegov.MicroserviceFramerwork.Patterns.UnitOfWork;
using Pepegov.MicroserviceFramerwork.ResultWrapper;

namespace MicroserviceOpenIddictTemplate.Identity.Endpoints.Permission.Queries;

public record class AddToProfileRequest : IRequest<ResultWrapper<ProfilePermissionViewModel>>
{
    public ProfilePermissionViewModel Model { get; set; }
    
    public AddToProfileRequest(ProfilePermissionViewModel model)
    {
        Model = model;
    }
}

public class AddToProfileRequestHandler : IRequestHandler<AddToProfileRequest, ResultWrapper<ProfilePermissionViewModel>>
{
    private readonly ILogger<UpdateRequestHandler> _logger;
    private readonly IUnitOfWork _unitOfWork;
    
    public AddToProfileRequestHandler(ILogger<UpdateRequestHandler> logger, IUnitOfWork unitOfWork)
    {
        _logger = logger;
        _unitOfWork = unitOfWork;
    }
    
    public async Task<ResultWrapper<ProfilePermissionViewModel>> Handle(AddToProfileRequest request, CancellationToken cancellationToken)
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
        
        profile.Permissions.Add(permission);
        profileReposytory.Update(profile);
        result.Message = request.Model;
        result.AddMetadatas(new Metadata($"Profile {profile.Id} is updated", MetadataType.Success));
        await _unitOfWork.SaveChangesAsync();

        return result;
    }
}