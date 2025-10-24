using MediatR;
using Pepegov.Identity.PL.Endpoints.Application.ViewModel;
using Pepegov.MicroserviceFramework.ApiResults;

namespace Pepegov.Identity.PL.Endpoints.Application.Queries;

public record GetApplicationViewModelRequest(string ClientId) : IRequest<ApiResult<ApplicationViewModel>>;