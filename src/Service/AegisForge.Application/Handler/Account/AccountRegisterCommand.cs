using System.Net;
using AegisForge.Application.Query.Account;
using AegisForge.Application.Service.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using Pepegov.MicroserviceFramework.ApiResults;
using Pepegov.MicroserviceFramework.Data.Exceptions;

namespace AegisForge.Application.Handler.Account;

/// <summary>
/// Response: Register new account
/// </summary>
public class AccountRegisterCommand(
    IAccountService accountService,
    ILogger<AccountRegisterCommand> logger)
    : IRequestHandler<Query.Account.AccountRegisterCommand, ApiResult>
{
    public async Task<ApiResult> Handle(Query.Account.AccountRegisterCommand request,
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