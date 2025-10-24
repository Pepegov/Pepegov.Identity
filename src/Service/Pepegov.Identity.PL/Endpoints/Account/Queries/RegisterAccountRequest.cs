using MediatR;
using Pepegov.Identity.DAL.ViewModel;
using Pepegov.MicroserviceFramework.ApiResults;

namespace Pepegov.Identity.PL.Endpoints.Account.Queries;

/// <summary>
/// Request: Register new account
/// </summary>
public record RegisterAccountRequest(RegisterViewModel Model) : IRequest<ApiResult>;