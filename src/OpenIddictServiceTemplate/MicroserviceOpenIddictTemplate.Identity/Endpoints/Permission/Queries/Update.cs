using MediatR;
using MicroserviceOpenIddictTemplate.DAL.Models.Identity;
using MicroserviceOpenIddictTemplate.Identity.Endpoints.Permission.ViewModel;
using Pepegov.MicroserviceFramerwork.Exceptions;
using Pepegov.MicroserviceFramerwork.Patterns.UnitOfWork;
using Pepegov.MicroserviceFramerwork.ResultWrapper;

namespace MicroserviceOpenIddictTemplate.Identity.Endpoints.Permission.Queries;

public record class UpdateRequest : IRequest<ResultWrapper<ApplicationPermission>>
{
    public Guid Id { get; set; }
    public PermissionViewModel Model { get; set; }
    
    public  UpdateRequest (Guid id, PermissionViewModel model)
    {
        Id = id;
        Model = model;
    }
}

public class UpdateRequestHandler : IRequestHandler<UpdateRequest, ResultWrapper<ApplicationPermission>>
{
    private readonly ILogger<UpdateRequestHandler> _logger;
    private readonly IUnitOfWork _unitOfWork;
    
    public UpdateRequestHandler(ILogger<UpdateRequestHandler> logger, IUnitOfWork unitOfWork)
    {
        _logger = logger;
        _unitOfWork = unitOfWork;
    }

    public async Task<ResultWrapper<ApplicationPermission>> Handle(UpdateRequest request, CancellationToken cancellationToken)
    {
        var result = new ResultWrapper<ApplicationPermission>();
        var permissionReposytory = _unitOfWork.GetRepository<ApplicationPermission>();
        
        var permission = await permissionReposytory.GetFirstOrDefaultAsync(
            predicate: x => x.ApplicationPermissionId == request.Id,
            disableTracking: false);
        if (permission is null)
        {
            var message = $"permission by id {request.Id} not found";
            result.AddException(new MicroserviceNotFoundException(message));
            _logger.LogError(message);
            return result;
        }
        
        permission.PolicyName = request.Model.PolicyName;
        permission.Description = request.Model.Description;
        permissionReposytory.Update(permission);
        await _unitOfWork.SaveChangesAsync();

        result.Message = permission;
        return result;
    }
}