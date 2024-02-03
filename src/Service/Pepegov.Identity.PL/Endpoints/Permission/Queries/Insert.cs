using AutoMapper;
using MediatR;
using Pepegov.Identity.DAL.Models.Identity;
using Pepegov.Identity.PL.Endpoints.Permission.ViewModel;
using Pepegov.MicroserviceFramework.ApiResults;
using Pepegov.UnitOfWork;
using Pepegov.UnitOfWork.EntityFramework;

namespace Pepegov.Identity.PL.Endpoints.Permission.Queries;

public record class InsertPermissionCommand : IRequest<ApiResult>
{
    public PermissionViewModel Model { get; set; }

    public InsertPermissionCommand(PermissionViewModel model)
    {
        Model = model;
    }
}

public class InsertPermissionCommandHandler : IRequestHandler<InsertPermissionCommand, ApiResult>
{
    private readonly ILogger<InsertPermissionCommandHandler> _logger;
    private readonly IUnitOfWorkManager _unitOfWork;
    private readonly IMapper _mapper;
    
    public InsertPermissionCommandHandler(ILogger<InsertPermissionCommandHandler> logger, IUnitOfWorkManager unitOfWork, IMapper mapper)
    {
        _logger = logger;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ApiResult> Handle(InsertPermissionCommand request, CancellationToken cancellationToken)
    {
        var result = new ApiResult<ApplicationPermission>();
        var permissionReposytory = _unitOfWork.GetInstance<IUnitOfWorkEntityFrameworkInstance>().GetRepository<ApplicationPermission>();
        await permissionReposytory.InsertAsync(_mapper.Map<ApplicationPermission>(request.Model), cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return result;
    }
}