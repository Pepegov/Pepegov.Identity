using MediatR;
using MicroserviceOpenIddictTemplate.DAL.Models.Identity;
using Pepegov.MicroserviceFramerwork.Patterns.UnitOfWork;
using Pepegov.MicroserviceFramerwork.ResultWrapper;

namespace MicroserviceOpenIddictTemplate.Identity.Endpoints.Permission.Queries;

public record class GetAllRequest : IRequest<ResultWrapper<IList<ApplicationPermission>>>;

public class GetAllRequestHandler : IRequestHandler<GetAllRequest, ResultWrapper<IList<ApplicationPermission>>>
{
    private readonly ILogger<GetAllRequestHandler> _logger;
    private readonly IUnitOfWork _unitOfWork;
    
    public GetAllRequestHandler(ILogger<GetAllRequestHandler> logger, IUnitOfWork unitOfWork)
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