using System.Security.Claims;
using MediatR;
using MicroserviceOpenIddictTemplate.PL.Api.Endpoints.Account.ViewModel;
using Pepegov.MicroserviceFramework.ApiResults;

namespace MicroserviceOpenIddictTemplate.PL.Api.Endpoints.Account.Queries;

public record GetClaimsRequest : IRequest<ApiResult<IEnumerable<ClaimsViewModel>>>;

public class GetClaimsRequestHandler : IRequestHandler<GetClaimsRequest, ApiResult<IEnumerable<ClaimsViewModel>>>
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<GetClaimsRequestHandler> _logger;
    
    public GetClaimsRequestHandler(IHttpContextAccessor httpContextAccessor, ILogger<GetClaimsRequestHandler> logger)
    {
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    public Task<ApiResult<IEnumerable<ClaimsViewModel>>> Handle(GetClaimsRequest request, CancellationToken cancellationToken)
    {
        var user = _httpContextAccessor.HttpContext!.User;
        var claims = ((ClaimsIdentity)user.Identity!).Claims;
        var resultClaims = claims.Select(x => new ClaimsViewModel { Type = x.Type, ValueType = x.ValueType, Value = x.Value });
        var result = new ApiResult<IEnumerable<ClaimsViewModel>>(resultClaims);
        
        _logger.LogInformation($"Current user {user.Identity.Name} have following climes {result}");
        return Task.FromResult(result);
    }
}