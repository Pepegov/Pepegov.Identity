using MediatR;
using MicroserviceOpenIddictTemplate.Identity.Application.Services;
using MicroserviceOpenIddictTemplate.Identity.Endpoints.Account.ViewModel;

namespace MicroserviceOpenIddictTemplate.Identity.Endpoints.Account.Queries;

/// <summary>
/// Request: Register new account
/// </summary>
public class RegisterAccountRequest : IRequest<UserAccountViewModel>
{
    public RegisterAccountRequest(RegisterViewModel model) => Model = model;

    public RegisterViewModel Model { get; }
}

/// <summary>
/// Response: Register new account
/// </summary>
public class RegisterAccountRequestHandler : IRequestHandler<RegisterAccountRequest, UserAccountViewModel>
{
    private readonly IAccountService _accountService;

    public RegisterAccountRequestHandler(IAccountService accountService)
        => _accountService = accountService;

    public async Task<UserAccountViewModel> Handle(RegisterAccountRequest request, CancellationToken cancellationToken)
        => await _accountService.RegisterAsync(request.Model, cancellationToken);
}