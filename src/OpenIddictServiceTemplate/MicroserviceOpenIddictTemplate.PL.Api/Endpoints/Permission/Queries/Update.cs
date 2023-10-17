using MediatR;
using MicroserviceOpenIddictTemplate.DAL.Models.Identity;
using MicroserviceOpenIddictTemplate.PL.Api.Endpoints.Permission.ViewModel;
using Pepegov.MicroserviceFramework.ApiResults;
using Pepegov.MicroserviceFramework.Data.Exceptions;
using Pepegov.UnitOfWork;
using Pepegov.UnitOfWork.EntityFramework;

namespace MicroserviceOpenIddictTemplate.PL.Api.Endpoints.Permission.Queries;

public record class UpdatePermissionCommand : IRequest<ApiResult>
{
    public Guid Id { get; set; }
    public PermissionViewModel Model { get; set; }
    
    public  UpdatePermissionCommand (Guid id, PermissionViewModel model)
    {
        Id = id;
        Model = model;
    }
}

public class UpdatePermissionCommandHandler : IRequestHandler<UpdatePermissionCommand, ApiResult>
{
    private readonly ILogger<UpdatePermissionCommandHandler> _logger;
    private readonly IUnitOfWorkManager _unitOfWork;
    
    public UpdatePermissionCommandHandler(ILogger<UpdatePermissionCommandHandler> logger, IUnitOfWorkManager unitOfWork)
    {
        _logger = logger;
        _unitOfWork = unitOfWork;
    }

    public async Task<ApiResult> Handle(UpdatePermissionCommand request, CancellationToken cancellationToken)
    {
        var result = new ApiResult();
        var permissionRepository = _unitOfWork.GetInstance<IUnitOfWorkEntityFrameworkInstance>().GetRepository<ApplicationPermission>();
        
        var permission = await permissionRepository.GetFirstOrDefaultAsync(
            predicate: x => x.ApplicationPermissionId == request.Id,
            disableTracking: false);
        if (permission is null)
        {
            var message = $"permission by id {request.Id} not found";
            result.AddExceptions(new MicroserviceNotFoundException(message));
            _logger.LogError(message);
            return result;
        }
        permissionRepository.Update(permission);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return result;
    }
}