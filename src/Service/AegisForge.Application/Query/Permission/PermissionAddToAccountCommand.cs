using AegisForge.Application.Dto;
using MediatR;
using Pepegov.MicroserviceFramework.ApiResults;

namespace AegisForge.Application.Query.Permission;

public record PermissionAddToAccountCommand(ProfilePermissionViewModel Model) : IRequest<ApiResult>;