using System.Net;
using MediatR;
using MicroserviceOpenIddictTemplate.BL.Services.Interfaces;
using MicroserviceOpenIddictTemplate.DAL.ViewModel;
using Pepegov.MicroserviceFramework.ApiResults;
using Pepegov.MicroserviceFramework.Data.Exceptions;

namespace MicroserviceOpenIddictTemplate.PL.Api.Endpoints.Account.Queries;

public record class GetAccountByIdRequest : IRequest<ApiResult<UserAccountViewModel>>
{ 
    public string Model { get; }

    public GetAccountByIdRequest(string model) => Model = model;
}

public class GetAccountByIdRequestHandler : IRequestHandler<GetAccountByIdRequest, ApiResult<UserAccountViewModel>>
{
    private readonly IAccountService _accountService;

    public GetAccountByIdRequestHandler(IAccountService accountService)
    {
        _accountService = accountService;
    }

    public async Task<ApiResult<UserAccountViewModel>> Handle(GetAccountByIdRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _accountService.GetByIdAsync(Guid.Parse(request.Model));
            return new ApiResult<UserAccountViewModel>(result, HttpStatusCode.OK);
        }
        catch (MicroserviceNotFoundException ex)
        {
            return new ApiResult<UserAccountViewModel>(HttpStatusCode.NotFound, ex);
        }
    }
}