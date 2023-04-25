using MediatR;
using MicroserviceOpenIddictTemplate.DAL.Domain;
using MicroserviceOpenIddictTemplate.DAL.Models.Identity;
using MicroserviceOpenIddictTemplate.Identity.Application.Services;
using MicroserviceOpenIddictTemplate.Identity.Base.Attributes;
using MicroserviceOpenIddictTemplate.Identity.Base.Definition;
using MicroserviceOpenIddictTemplate.Identity.Endpoints.Account.Queries;
using MicroserviceOpenIddictTemplate.Identity.Endpoints.Account.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace MicroserviceOpenIddictTemplate.Identity.Endpoints.Account;

public class AccountEndPoint : Definition
{
    public override void ConfigureApplicationAsync(WebApplication app)
    {
        app.MapPost("~/api/account/register", Register).WithOpenApi();
        app.MapGet("~/api/account/getuserbyid", GetAccountById).WithOpenApi();
        
        app.MapGet("~/api/account/getcurrentaccountclaims", GetClaims).WithOpenApi();
        app.MapGet("~/api/account/getcurrentaccount", GetCurrentAccount).WithOpenApi();
        app.MapGet("~/api/account/getcurrentaccountid", GetCurrentAccountId).WithOpenApi();
    }

    [ProducesResponseType(200)]
    [ProducesResponseType(401)]
    [FeatureGroupName("Account")]
    private async Task<IResult> Register(
        HttpContext httpContext,
        [FromBody] RegisterViewModel model,
        [FromServices] IMediator mediator)
    {
        var result = await mediator.Send(new RegisterAccountRequest(model), httpContext.RequestAborted);
        Log.Information($"{result.FirstName} {result.LastName} has be registered");
        return Results.Ok(result);
    }

    [ProducesResponseType(200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [FeatureGroupName("Account")]
    [Authorize(AuthenticationSchemes = AuthData.AuthenticationSchemes, Roles = UserRoles.Admin)]
    private async Task<ApplicationUser> GetAccountById(
        HttpContext httpContext,
        [FromQuery] string id,
        [FromServices] IMediator mediator)
        => await mediator.Send(new GetAccountByIdRequest(id));

    [ProducesResponseType(200)]
    [ProducesResponseType(401)]
    [FeatureGroupName("Account")]
    [Authorize(AuthenticationSchemes = AuthData.AuthenticationSchemes)]
    private async Task<IEnumerable<ClaimsViewModel>> GetClaims(HttpContext context, [FromServices] IMediator mediator)
        => await mediator.Send(new GetClaimsRequest(), context.RequestAborted);

    [ProducesResponseType(200)]
    [ProducesResponseType(401)]
    [FeatureGroupName("Account")]
    [Authorize(AuthenticationSchemes = AuthData.AuthenticationSchemes)]
    private async Task<ApplicationUser> GetCurrentAccount(IAccountService accountService)
        => await accountService.GetCurrentUserAsync();
    
    [ProducesResponseType(200)]
    [ProducesResponseType(401)]
    [FeatureGroupName("Account")]
    [Authorize(AuthenticationSchemes = AuthData.AuthenticationSchemes)]
    private async Task<Guid> GetCurrentAccountId(IAccountService accountService)
        => accountService.GetCurrentUserId(); 
}