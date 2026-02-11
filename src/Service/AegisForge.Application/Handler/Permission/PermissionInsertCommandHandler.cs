using AegisForge.Application.Query.Permission;
using AegisForge.Domain.Entity;
using AutoMapper;
using MediatR;
using Pepegov.MicroserviceFramework.ApiResults;
using Pepegov.UnitOfWork;
using Pepegov.UnitOfWork.EntityFramework;

namespace AegisForge.Application.Handler.Permission;

public class PermissionInsertCommandHandler(
    IUnitOfWorkManager unitOfWork,
    IMapper mapper)
    : IRequestHandler<PermissionInsertCommand, ApiResult>
{
    public async Task<ApiResult> Handle(PermissionInsertCommand request, CancellationToken cancellationToken)
    {
        var result = new ApiResult<ApplicationPermission>();
        var repository = unitOfWork.GetInstance<IUnitOfWorkEntityFrameworkInstance>().GetRepository<ApplicationPermission>();
        await repository.InsertAsync(mapper.Map<ApplicationPermission>(request.Model), cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return result;
    }
}