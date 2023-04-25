using System.Security.Claims;
using MediatR;
using MicroserviceOpenIddictTemplate.Identity.Endpoints.Account.ViewModel;

namespace MicroserviceOpenIddictTemplate.Identity.Endpoints.Account.Queries;

public record GetClaimsRequest : IRequest<IEnumerable<ClaimsViewModel>>;

public class GetClaimsRequestHandler : IRequestHandler<GetClaimsRequest, IEnumerable<ClaimsViewModel>>
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<GetClaimsRequestHandler> _logger;
    
    public GetClaimsRequestHandler(IHttpContextAccessor httpContextAccessor, ILogger<GetClaimsRequestHandler> logger)
    {
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    public Task<IEnumerable<ClaimsViewModel>> Handle(GetClaimsRequest request, CancellationToken cancellationToken)
    {
        var user = _httpContextAccessor.HttpContext!.User;
        var claims = ((ClaimsIdentity)user.Identity!).Claims;
        var result = claims.Select(x => new ClaimsViewModel { Type = x.Type, ValueType = x.ValueType, Value = x.Value });
        _logger.LogInformation($"Current user {user.Identity.Name} have following climes {result}");
        return Task.FromResult(result);
    }
}