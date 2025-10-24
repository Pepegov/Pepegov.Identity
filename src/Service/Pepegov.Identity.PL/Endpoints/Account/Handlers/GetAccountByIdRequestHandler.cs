using System.Net;
using MediatR;
using Pepegov.Identity.BL.Services.Interfaces;
using Pepegov.Identity.DAL.ViewModel;
using Pepegov.MicroserviceFramework.ApiResults;
using Pepegov.MicroserviceFramework.Data.Exceptions;

namespace Pepegov.Identity.PL.Endpoints.Account.Queries;

public class GetAccountByIdRequestHandler(IAccountService accountService)
    : IRequestHandler<GetAccountByIdRequest, ApiResult<UserAccountViewModel>>
{
    public async Task<ApiResult<UserAccountViewModel>> Handle(GetAccountByIdRequest request,
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