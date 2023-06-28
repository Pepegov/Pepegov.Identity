using MediatR;
using MicroserviceOpenIddictTemplate.DAL.Models.Identity;
using Pepegov.MicroserviceFramerwork.ResultWrapper;
using Pepegov.UnitOfWork.EntityFramework;

namespace MicroserviceOpenIddictTemplate.Identity.Endpoints.Permission.Queries;

public record class GetAllRequest : IRequest<ResultWrapper<IList<ApplicationPermission>>>;

public class GetAllRequestHandler : IRequestHandler<GetAllRequest, ResultWrapper<IList<ApplicationPermission>>>
{
    private readonly ILogger<GetAllRequestHandler> _logger;
    private readonly IUnitOfWorkEF _unitOfWork;
    
    public GetAllRequestHandler(ILogger<GetAllRequestHandler> logger, IUnitOfWorkEF unitOfWork)
    {
        _logger = logger;
        _unitOfWork = unitOfWork;
    }
    
    public async Task<ResultWrapper<IList<ApplicationPermission>>> Handle(GetAllRequest request, CancellationToken cancellationToken)
    {
        var result = new ResultWrapper<IList<ApplicationPermission>>();
        var permissionReposytory = _unitOfWork.GetRepository<ApplicationPermission>();
        result.Message = await permissionReposytory.GetAllAsync(true);
        
        return result;
    }
}