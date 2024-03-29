﻿using System.Net;
using MediatR;
using Pepegov.Identity.BL.Services.Interfaces;
using Pepegov.Identity.DAL.ViewModel;
using Pepegov.MicroserviceFramework.ApiResults;
using Pepegov.MicroserviceFramework.Data.Exceptions;

namespace Pepegov.Identity.PL.Endpoints.Account.Queries;

/// <summary>
/// Request: Register new account
/// </summary>
public class RegisterAccountRequest : IRequest<ApiResult>
{
    public RegisterAccountRequest(RegisterViewModel model) => Model = model;

    public RegisterViewModel Model { get; }
}

/// <summary>
/// Response: Register new account
/// </summary>
public class RegisterAccountRequestHandler : IRequestHandler<RegisterAccountRequest, ApiResult>
{
    private readonly IAccountService _accountService;
    private readonly ILogger<RegisterAccountRequestHandler> _logger;

    public RegisterAccountRequestHandler(IAccountService accountService, ILogger<RegisterAccountRequestHandler> logger)
    {
        _accountService = accountService;
        _logger = logger;
    }

    public async Task<ApiResult> Handle(RegisterAccountRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            await _accountService.RegisterAsync(request.Model, cancellationToken);
            return new ApiResult(HttpStatusCode.OK);
        }
        catch (MicroserviceNotFoundException ex)
        {
            _logger.LogInformation(ex.Message);
            return new ApiResult(HttpStatusCode.NotFound, ex);
        }
    }
}