using AegisForge.Application.Dto;
using MediatR;
using Pepegov.MicroserviceFramework.ApiResults;

namespace AegisForge.Application.Query.Permission;

public record PermissionUpdateCommand(Guid Id, PermissionViewModel Model) : IRequest<ApiResult>;