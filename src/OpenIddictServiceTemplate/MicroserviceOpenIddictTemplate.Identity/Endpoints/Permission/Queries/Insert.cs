using AutoMapper;
using MediatR;
using MicroserviceOpenIddictTemplate.DAL.Models.Identity;
using MicroserviceOpenIddictTemplate.Identity.Endpoints.Permission.ViewModel;
using Pepegov.MicroserviceFramerwork.ResultWrapper;
using Pepegov.UnitOfWork.EntityFramework;

namespace MicroserviceOpenIddictTemplate.Identity.Endpoints.Permission.Queries;

public record class InsertRequest : IRequest<ResultWrapper<ApplicationPermission>>
{
    public PermissionViewModel Model { get; set; }

    public InsertRequest(PermissionViewModel model)
    {
        Model = model;
    }
}

public class InsertRequestHandler : IRequestHandler<InsertRequest, ResultWrapper<ApplicationPermission>>
{
    private readonly ILogger<InsertRequestHandler> _logger;
    private readonly IUnitOfWorkEF _unitOfWork;
    private readonly IMapper _mapper;
    
    public InsertRequestHandler(ILogger<InsertRequestHandler> logger, IUnitOfWorkEF unitOfWork, IMapper mapper)
    {
        _logger = logger;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ResultWrapper<ApplicationPermission>> Handle(InsertRequest request, CancellationToken cancellationToken)
    {
        var result = new ResultWrapper<ApplicationPermission>();
        var permissionReposytory = _unitOfWork.GetRepository<ApplicationPermission>();
        var perrmision = await permissionReposytory.InsertAsync(_mapper.Map<ApplicationPermission>(request.Model));
        await _unitOfWork.SaveChangesAsync();
        result.Message = perrmision.Entity;

        return result;
    }
}