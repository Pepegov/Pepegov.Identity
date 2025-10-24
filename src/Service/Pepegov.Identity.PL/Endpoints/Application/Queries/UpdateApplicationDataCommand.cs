using MediatR;
using Pepegov.Identity.PL.Endpoints.Application.ViewModel;
using Pepegov.MicroserviceFramework.ApiResults;

namespace Pepegov.Identity.PL.Endpoints.Application.Queries;

public record UpdateApplicationDataCommand(string TargetClientId, ApplicationDataUpdateModel UpdateModel) : IRequest<ApiResult>;