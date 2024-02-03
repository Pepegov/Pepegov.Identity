using MediatR;
using Pepegov.Identity.DAL.Models.Identity;
using Pepegov.MicroserviceFramework.ApiResults;
using Pepegov.UnitOfWork;
using Pepegov.UnitOfWork.EntityFramework;

namespace Pepegov.Identity.PL.Endpoints.Permission.Queries;

public record class GetAllPermissionsRequest : IRequest<ApiResult<IList<ApplicationPermission>>>;

public class GetAllPermissionsRequestHandler : IRequestHandler<GetAllPermissionsRequest, ApiResult<IList<ApplicationPermission>>>
{
    private readonly ILogger<GetAllPermissionsRequestHandler> _logger;
    private readonly IUnitOfWorkManager _unitOfWork;
    
    public GetAllPermissionsRequestHandler(ILogger<GetAllPermissionsRequestHandler> logger, IUnitOfWorkManager unitOfWork)
    {
        _logger = logger;
        _unitOfWork = unitOfWork;
    }
    
    public async Task<ApiResult<IList<ApplicationPermission>>> Handle(GetAllPermissionsRequest request, CancellationToken cancellationToken)
    {
        var result = new ApiResult<IList<ApplicationPermission>>();
        var permissionRepository = _unitOfWork.GetInstance<IUnitOfWorkEntityFrameworkInstance>().GetRepository<ApplicationPermission>();
        result.Message = await permissionRepository.GetAllAsync(true);
        
        return result;
    }
}