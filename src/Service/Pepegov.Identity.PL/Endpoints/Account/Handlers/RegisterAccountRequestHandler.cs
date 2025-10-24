using System.Net;
using MediatR;
using Pepegov.Identity.BL.Services.Interfaces;
using Pepegov.MicroserviceFramework.ApiResults;
using Pepegov.MicroserviceFramework.Data.Exceptions;

namespace Pepegov.Identity.PL.Endpoints.Account.Queries;

/// <summary>
/// Response: Register new account
/// </summary>
public class RegisterAccountRequestHandler(
    IAccountService accountService,
    ILogger<RegisterAccountRequestHandler> logger)
    : IRequestHandler<RegisterAccountRequest, ApiResult>
{
    public async Task<ApiResult> Handle(RegisterAccountRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            await accountService.RegisterAsync(request.Model, cancellationToken);
            return new ApiResult(HttpStatusCode.OK);
        }
        catch (MicroserviceNotFoundException ex)
        {
            logger.LogInformation(ex.Message);
            return new ApiResult(HttpStatusCode.NotFound, ex);
        }
    }
}