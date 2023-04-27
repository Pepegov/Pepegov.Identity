using System.Security.Claims;
using MediatR;
using MicroserviceOpenIddictTemplate.Identity.Endpoints.Account.ViewModel;
using Pepegov.MicroserviceFramerwork.ResultWrapper;

namespace MicroserviceOpenIddictTemplate.Identity.Endpoints.Account.Queries;

public record GetClaimsRequest : IRequest<ResultWrapper<IEnumerable<ClaimsViewModel>>>;

public class GetClaimsRequestHandler : IRequestHandler<GetClaimsRequest, ResultWrapper<IEnumerable<ClaimsViewModel>>>
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<GetClaimsRequestHandler> _logger;
    
    public GetClaimsRequestHandler(IHttpContextAccessor httpContextAccessor, ILogger<GetClaimsRequestHandler> logger)
    {
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    public Task<ResultWrapper<IEnumerable<ClaimsViewModel>>> Handle(GetClaimsRequest request, CancellationToken cancellationToken)
    {
        var user = _httpContextAccessor.HttpContext!.User;
        var claims = ((ClaimsIdentity)user.Identity!).Claims;
        var resultClaims = claims.Select(x => new ClaimsViewModel { Type = x.Type, ValueType = x.ValueType, Value = x.Value });
        var result = new ResultWrapper<IEnumerable<ClaimsViewModel>>(resultClaims);
        
        _logger.LogInformation($"Current user {user.Identity.Name} have following climes {result}");
        return Task.FromResult(result);
    }
}