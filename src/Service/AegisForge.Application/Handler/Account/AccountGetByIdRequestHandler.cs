using System.Net;
using AegisForge.Application.Dto;
using AegisForge.Application.Query.Account;
using AegisForge.Application.Service.Interfaces;
using MediatR;
using Pepegov.MicroserviceFramework.ApiResults;
using Pepegov.MicroserviceFramework.Data.Exceptions;

namespace AegisForge.Application.Handler.Account;

public class AccountGetByIdRequestHandler(IAccountService accountService)
    : IRequestHandler<AccountGetByIdRequest, ApiResult<UserAccountViewModel>>
{
    public async Task<ApiResult<UserAccountViewModel>> Handle(AccountGetByIdRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await accountService.GetByIdAsync(Guid.Parse(request.Model));
            return new ApiResult<UserAccountViewModel>(result, HttpStatusCode.OK);
        }
        catch (MicroserviceNotFoundException ex)
        {
            return new ApiResult<UserAccountViewModel>(HttpStatusCode.NotFound, ex);
        }
    }
}