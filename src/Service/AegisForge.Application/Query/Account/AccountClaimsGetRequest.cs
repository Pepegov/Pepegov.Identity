using AegisForge.Application.Dto;
using MediatR;
using Pepegov.MicroserviceFramework.ApiResults;

namespace AegisForge.Application.Query.Account;

public record AccountClaimsGetRequest : IRequest<ApiResult<IEnumerable<ClaimsViewModel>>>;