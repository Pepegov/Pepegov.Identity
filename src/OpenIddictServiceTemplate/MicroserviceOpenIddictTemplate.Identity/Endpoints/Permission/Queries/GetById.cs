using MediatR;
using MicroserviceOpenIddictTemplate.DAL.Models.Identity;
using Pepegov.MicroserviceFramerwork.ResultWrapper;
using Pepegov.UnitOfWork.EntityFramework;

namespace MicroserviceOpenIddictTemplate.Identity.Endpoints.Permission.Queries;

public record class GetByIdRequest : IRequest<ResultWrapper<ApplicationPermission>>
{ 
    public Guid Model { get; }

    public GetByIdRequest(Guid model) => Model = model;
}

public class GetByIdRequestHandler : IRequestHandler<GetByIdRequest, ResultWrapper<ApplicationPermission>>
{
    private readonly ILogger<GetByIdRequestHandler> _logger;
    private readonly IUnitOfWorkEF _unitOfWork;
    
    public GetByIdRequestHandler(ILogger<GetByIdRequestHandler> logger, IUnitOfWorkEF unitOfWork)
    {
        _logger = logger;
        _unitOfWork = unitOfWork;
    }
    
    public async Task<ResultWrapper<ApplicationPermission>> Handle(GetByIdRequest request, CancellationToken cancellationToken)
    {
        var result = new ResultWrapper<ApplicationPermission>();
        var permissionReposytory = _unitOfWork.GetRepository<ApplicationPermission>();
        result.Message = await permissionReposytory.GetFirstOrDefaultAsync(predicate: x=> x.ApplicationPermissionId == request.Model );
        if (result.Message is null)
        {
            result.AddMetadatas(new Metadata("Permission not found", MetadataType.Error));
            _logger.LogWarning($"Permission {request.Model} not found");
        }

        return result;
    }
}