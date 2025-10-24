using MediatR;
using Pepegov.Identity.DAL.ViewModel;
using Pepegov.MicroserviceFramework.ApiResults;

namespace Pepegov.Identity.PL.Endpoints.Account.Queries;

public record GetAccountByIdRequest(string Model) : IRequest<ApiResult<UserAccountViewModel>>;