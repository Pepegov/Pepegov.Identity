using MediatR;
using Pepegov.Identity.PL.Endpoints.Account.ViewModel;
using Pepegov.MicroserviceFramework.ApiResults;

namespace Pepegov.Identity.PL.Endpoints.Account.Queries;

public record GetClaimsRequest : IRequest<ApiResult<IEnumerable<ClaimsViewModel>>>;