using AegisForge.Application.Dto;
using AegisForge.Application.Query.Permission;
using AegisForge.Domain.Entity;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using Pepegov.MicroserviceFramework.ApiResults;
using Pepegov.UnitOfWork;
using Pepegov.UnitOfWork.EntityFramework;

namespace AegisForge.Application.Handler.Permission;

public class PermissionGetByIdRequestHandler(
    ILogger<PermissionGetByIdRequestHandler> logger,
    IUnitOfWorkManager unitOfWork,
    IMapper mapper)
    : IRequestHandler<PermissionGetByIdRequest, ApiResult<PermissionViewModel>>
{
    public async Task<ApiResult<PermissionViewModel>> Handle(PermissionGetByIdRequest request, CancellationToken cancellationToken)
    {
        var result = new ApiResult<PermissionViewModel>();
        var permissionRepository = unitOfWork.GetInstance<IUnitOfWorkEntityFrameworkInstance>().GetRepository<ApplicationPermission>();
        var entity = await permissionRepository.GetFirstOrDefaultAsync(
            predicate: x=> x.ApplicationPermissionId == request.Model, 
            cancellationToken: cancellationToken);
        if (entity is null)
        {
            result.AddMetadata(new Metadata("Permission not found", MetadataType.Error));
            logger.LogWarning($"Permission {request.Model} not found");
        }

        result.Message = mapper.Map<PermissionViewModel>(entity);
        
        return result;
    }
}