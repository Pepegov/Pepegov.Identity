using MediatR;
using MicroserviceOpenIddictTemplate.DAL.Models.Identity;
using MicroserviceOpenIddictTemplate.Identity.Application.Services;

namespace MicroserviceOpenIddictTemplate.Identity.Endpoints.Account.Queries;

public record class GetAccountByIdRequest : IRequest<ApplicationUser>
{ 
    public string Model { get; }

    public GetAccountByIdRequest(string model) => Model = model;
}

public class GetAccountByIdRequestHandler : IRequestHandler<GetAccountByIdRequest, ApplicationUser>
{
    private readonly IAccountService _accountService;

    public GetAccountByIdRequestHandler(IAccountService accountService)
    {
        _accountService = accountService;
    }

    public async Task<ApplicationUser> Handle(GetAccountByIdRequest request, CancellationToken cancellationToken)
        => await _accountService.GetByIdAsync(Guid.Parse(request.Model));
}