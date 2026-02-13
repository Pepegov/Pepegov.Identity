using AegisForge.Application.Query.Session;
using AegisForge.Application.Service.Interfaces;
using AegisForge.Infrastructure.Domain;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pepegov.MicroserviceFramework.AspNetCore.WebApi;
using Pepegov.MicroserviceFramework.AspNetCore.WebApplicationDefinition;
using Pepegov.MicroserviceFramework.Definition;
using Pepegov.MicroserviceFramework.Definition.Context;
using TelecomService.QRON.ID.Identity.Domain.Enum;

namespace AegisForge.PL.Endpoints;

public class SessionEndPoints : ApplicationDefinition
{
    public override Task ConfigureApplicationAsync(IDefinitionApplicationContext context)
    {
        var app = context.Parse<WebDefinitionApplicationContext>().WebApplication;
        
        RouteGroupBuilder group = app
            .MapGroup("~/api/sessions/")
            .WithTags("Session")
            .WithOpenApi();

        group.MapGet("status", Check);
        group.MapGet("paged", Paged);
        group.MapPost("{id}/terminate", Terminate);
        
        if (!app.Environment.IsProduction())
        {
            group.MapGet("/test/userconnectinfo", TestGetUserData);
        }
        
        return base.ConfigureApplicationAsync(context);
    }
    
    [ProducesResponseType(200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [Authorize(AuthenticationSchemes = AuthData.AuthenticationSchemes)]
    private async Task<IResult> Terminate(
        HttpContext httpContext,
        [FromHeader] bool? Bff,
        [FromServices] IMediator mediator,
        [FromServices] IAccountService accountService,
        Guid id)
    {
        var userId = accountService.GetCurrentUserId();
        var result = await mediator.Send(new SessionTerminateCommand(id, userId), httpContext.RequestAborted);
        return Results.Extensions.Custom(result);
    }

    [ProducesResponseType(200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [Authorize(AuthenticationSchemes = AuthData.AuthenticationSchemes)]
    private async Task<IResult> Paged(
        HttpContext httpContext,
        [FromHeader] bool? Bff,
        [FromServices] IMediator mediator,
        [FromServices] IAccountService accountService,
        [FromQuery] int pageIndex = 0,
        [FromQuery] int pageSize = 20,
        [FromQuery] SessionStatusType? sessionStatusType = null)
    {
        var userId = accountService.GetCurrentUserId();
        var result = await mediator.Send(new SessionGetPagedRequest(userId, pageIndex, pageSize, sessionStatusType), httpContext.RequestAborted);
        return Results.Extensions.Custom(result);
    }
    
    [ProducesResponseType(200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    private async Task<IResult> TestGetUserData(
        HttpContext httpContext,
        [FromHeader] bool? Bff,
        [FromServices] IUserConnectInfoService userConnectInfoService)
    {
        var response = await userConnectInfoService.GetUserConnectionInfoAsync();
        return Results.Ok(response);
    }
    
    [ProducesResponseType(200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    private async Task<IResult> Check(
        HttpContext httpContext,
        [FromHeader] bool? Bff,
        [FromServices] IMediator mediator,
        [FromServices] IAccountService accountService,
        [FromQuery] string session_state)
    {
        var result = await mediator.Send(new SessionStatusRequest(session_state), httpContext.RequestAborted);
        return Results.Ok(result.Message);
    }
}