using System.Security.Claims;
using AegisForge.Application.Dto;
using AegisForge.Application.Query.Account;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Pepegov.MicroserviceFramework.ApiResults;

namespace AegisForge.Application.Handler.Account;

public class AccountClaimsGetRequestHandler : IRequestHandler<AccountClaimsGetRequest, ApiResult<IEnumerable<ClaimsViewModel>>>
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<AccountClaimsGetRequestHandler> _logger;
    
    public AccountClaimsGetRequestHandler(IHttpContextAccessor httpContextAccessor, ILogger<AccountClaimsGetRequestHandler> logger)
    {
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    public Task<ApiResult<IEnumerable<ClaimsViewModel>>> Handle(AccountClaimsGetRequest request, CancellationToken cancellationToken)
    {
        var user = _httpContextAccessor.HttpContext!.User;
        var claims = ((ClaimsIdentity)user.Identity!).Claims;
        var resultClaims = claims.Select(x => new ClaimsViewModel { Type = x.Type, ValueType = x.ValueType, Value = x.Value });
        var result = new ApiResult<IEnumerable<ClaimsViewModel>>(resultClaims);
        
        _logger.LogInformation($"Current user {user.Identity.Name} have following climes {result}");
        return Task.FromResult(result);
    }
}