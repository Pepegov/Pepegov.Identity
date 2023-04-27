using MediatR;
using MicroserviceOpenIddictTemplate.DAL.Models.Identity;
using MicroserviceOpenIddictTemplate.Identity.Application.Services;
using Pepegov.MicroserviceFramerwork.ResultWrapper;

namespace MicroserviceOpenIddictTemplate.Identity.Endpoints.Account.Queries;

public record class GetAccountByIdRequest : IRequest<ResultWrapper<ApplicationUser>>
{ 
    public string Model { get; }

    public GetAccountByIdRequest(string model) => Model = model;
}

public class GetAccountByIdRequestHandler : IRequestHandler<GetAccountByIdRequest, ResultWrapper<ApplicationUser>>
{
    private readonly IAccountService _accountService;

    public GetAccountByIdRequestHandler(IAccountService accountService)
    {
        _accountService = accountService;
    }

    public async Task<ResultWrapper<ApplicationUser>> Handle(GetAccountByIdRequest request, CancellationToken cancellationToken)
        => await _accountService.GetByIdAsync(Guid.Parse(request.Model));
}