using System.Net;
using AegisForge.Application.Query.Permission;
using AegisForge.Domain.Entity;
using MediatR;
using Pepegov.MicroserviceFramework.ApiResults;
using Pepegov.UnitOfWork;
using Pepegov.UnitOfWork.EntityFramework;

namespace AegisForge.Application.Handler.Permission;

public class PermissionGetAllRequestHandler(
    IUnitOfWorkManager unitOfWork)
    : IRequestHandler<PermissionGetAllRequest, ApiResult<IList<ApplicationPermission>>>
{
    public async Task<ApiResult<IList<ApplicationPermission>>> Handle(PermissionGetAllRequest request, CancellationToken cancellationToken)
    {
        var result = new ApiResult<IList<ApplicationPermission>>();
        var permissionRepository = unitOfWork.GetInstance<IUnitOfWorkEntityFrameworkInstance>().GetRepository<ApplicationPermission>();
        result.Message = await permissionRepository.GetAllAsync(cancellationToken);
        result.StatusCode = (int)HttpStatusCode.OK;
        
        return result;
    }
}