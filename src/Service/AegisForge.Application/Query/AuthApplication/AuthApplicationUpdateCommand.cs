using AegisForge.Application.Dto;
using MediatR;
using Pepegov.MicroserviceFramework.ApiResults;

namespace AegisForge.Application.Query.AuthApplication;

public record AuthApplicationUpdateCommand(string TargetClientId, ApplicationDataUpdateModel UpdateModel) : IRequest<ApiResult>;