using AutoMapper;
using MediatR;
using MicroserviceOpenIddictTemplate.DAL.Models.Identity;
using MicroserviceOpenIddictTemplate.PL.Endpoints.Permission.ViewModel;
using Microsoft.Extensions.Logging;
using Pepegov.MicroserviceFramework.ApiResults;
using Pepegov.UnitOfWork;
using Pepegov.UnitOfWork.EntityFramework;

namespace MicroserviceOpenIddictTemplate.PL.Endpoints.Permission.Queries;

public sealed class GetPermissionByIdRequest : IRequest<ApiResult<PermissionViewModel>>
{ 
    public Guid Model { get; }

    public GetPermissionByIdRequest(Guid model) => Model = model;
}

public class GetPermissionByIdRequestHandler : IRequestHandler<GetPermissionByIdRequest, ApiResult<PermissionViewModel>>
{
    private readonly ILogger<GetPermissionByIdRequestHandler> _logger;
    private readonly IUnitOfWorkManager _unitOfWork;
    private readonly IMapper _mapper;
    
    public GetPermissionByIdRequestHandler(ILogger<GetPermissionByIdRequestHandler> logger, IUnitOfWorkManager unitOfWork, IMapper mapper)
    {
        _logger = logger;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }
    
    public async Task<ApiResult<PermissionViewModel>> Handle(GetPermissionByIdRequest request, CancellationToken cancellationToken)
    {
        var result = new ApiResult<PermissionViewModel>();
        var permissionReposytory = _unitOfWork.GetInstance<IUnitOfWorkEntityFrameworkInstance>().GetRepository<ApplicationPermission>();
        var entity = await permissionReposytory.GetFirstOrDefaultAsync(predicate: x=> x.ApplicationPermissionId == request.Model );
        if (entity is null)
        {
            result.AddMetadata(new Metadata("Permission not found", MetadataType.Error));
            _logger.LogWarning($"Permission {request.Model} not found");
        }

        result.Message = _mapper.Map<PermissionViewModel>(entity);
        
        return result;
    }
}