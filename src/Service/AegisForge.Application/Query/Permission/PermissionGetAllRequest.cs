using AegisForge.Domain.Entity;
using MediatR;
using Pepegov.MicroserviceFramework.ApiResults;

namespace AegisForge.Application.Query.Permission;

public record PermissionGetAllRequest : IRequest<ApiResult<IList<ApplicationPermission>>>;