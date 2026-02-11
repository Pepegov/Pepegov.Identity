using System.Net;
using AegisForge.Application.Dto;
using AegisForge.Application.Query.Account;
using AegisForge.Application.Service.Interfaces;
using AegisForge.Infrastructure.Domain;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pepegov.MicroserviceFramework.ApiResults;
using Pepegov.MicroserviceFramework.AspNetCore.WebApi;
using Pepegov.MicroserviceFramework.AspNetCore.WebApplicationDefinition;
using Pepegov.MicroserviceFramework.Definition;
using Pepegov.MicroserviceFramework.Definition.Context;

namespace AegisForge.PL.Endpoints;

public class AccountEndPoint : ApplicationDefinition
{
    public override Task ConfigureApplicationAsync(IDefinitionApplicationContext context)
    {
        var app = context.Parse<WebDefinitionApplicationContext>().WebApplication;
        
        app.MapPost("~/api/account/user/register", Register).WithOpenApi()
            .WithTags("Account")
            .WithSummary("Register a new user")
            .WithDescription("Do not have policy");
        
        app.MapGet("~/api/account/admin/by-id", GetAccountById).WithOpenApi()
            .WithTags("Account")
            .WithSummary("Get user by id for admin")
            .WithDescription($"Policy = {AppPermissions.AccountAdmin}");
        
        app.MapGet("~/api/account/current/claims", GetClaims).WithOpenApi()
            .WithTags("Account")
            .WithSummary("Helped get your current claims");
        app.MapGet("~/api/account/current/", GetCurrentAccount).WithOpenApi()
            .WithTags("Account")
            .WithSummary("Get current user");
        app.MapGet("~/api/account/current/id", GetCurrentAccountId).WithOpenApi()
            .WithTags("Account")
            .WithSummary("Get current user id");   
        
        return base.ConfigureApplicationAsync(context);
    }

    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(500)]
    private async Task<IResult> Register(
        HttpContext httpContext,
        [FromBody] RegisterViewModel model,
        [FromServices] IMediator mediator)
    {
        var result = await mediator.Send(new AccountRegisterCommand(model), httpContext.RequestAborted);
        return Results.Extensions.Custom(result);
    }

    [ProducesResponseType(200, Type = typeof(UserAccountViewModel))]
    [ProducesResponseType(401)]
    [ProducesResponseType(404)]
    [ProducesResponseType(403)]
    [Authorize(AuthenticationSchemes = AuthData.AuthenticationSchemes, Policy = AppPermissions.AccountAdmin)]
    private async Task<IResult> GetAccountById(
        HttpContext httpContext,
        [FromQuery] string id,
        [FromServices] IMediator mediator)
    {
        var result = await mediator.Send(new AccountGetByIdRequest(id));
        return Results.Extensions.Custom(result);
    }

    [ProducesResponseType(200, Type = typeof(IEnumerable<ClaimsViewModel>))]
    [ProducesResponseType(401)]
    [Authorize(AuthenticationSchemes = AuthData.AuthenticationSchemes)]
    private async Task<ApiResult<IEnumerable<ClaimsViewModel>>> GetClaims(HttpContext context, [FromServices] IMediator mediator)
        => await mediator.Send(new AccountClaimsGetRequest(), context.RequestAborted);

    [ProducesResponseType(200, Type = typeof(UserAccountViewModel))]
    [ProducesResponseType(401)]
    [Authorize(AuthenticationSchemes = AuthData.AuthenticationSchemes)]
    private async Task<IResult> GetCurrentAccount(IAccountService accountService)
    {
        var user = await accountService.GetCurrentUserAsync();
        return Results.Extensions.Custom(user, HttpStatusCode.OK);
    }
    
    [ProducesResponseType(200, Type = typeof(Guid))]
    [ProducesResponseType(401)]
    [Authorize(AuthenticationSchemes = AuthData.AuthenticationSchemes)]
    private Guid GetCurrentAccountId(IAccountService accountService)
        => accountService.GetCurrentUserId(); 
}