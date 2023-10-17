using MediatR;
using MicroserviceOpenIddictTemplate.DAL.Models.Identity;
using MicroserviceOpenIddictTemplate.PL.Api.Endpoints.Permission.ViewModel;
using Microsoft.EntityFrameworkCore;
using Pepegov.MicroserviceFramework.ApiResults;
using Pepegov.MicroserviceFramework.Data.Exceptions;
using Pepegov.UnitOfWork;
using Pepegov.UnitOfWork.EntityFramework;

namespace MicroserviceOpenIddictTemplate.PL.Api.Endpoints.Permission.Queries;

public record class AddPermissionToAccountCommand : IRequest<ApiResult>
{
    public ProfilePermissionViewModel Model { get; set; }
    
    public AddPermissionToAccountCommand(ProfilePermissionViewModel model)
    {
        Model = model;
    }
}

public class AddPermissionToAccountCommandHandler : IRequestHandler<AddPermissionToAccountCommand, ApiResult>
{
    private readonly ILogger<UpdatePermissionCommandHandler> _logger;
    private readonly IUnitOfWorkManager _unitOfWork;
    
    public AddPermissionToAccountCommandHandler(ILogger<UpdatePermissionCommandHandler> logger, IUnitOfWorkManager unitOfWork)
    {
        _logger = logger;
        _unitOfWork = unitOfWork;
    }
    
    public async Task<ApiResult> Handle(AddPermissionToAccountCommand request, CancellationToken cancellationToken)
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
        
        profile.Permissions.Add(permission);
        profileRepository.Update(profile);
        result.AddMetadata(new Metadata($"Profile {profile.Id} is updated", MetadataType.Success));
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return result;
    }
}