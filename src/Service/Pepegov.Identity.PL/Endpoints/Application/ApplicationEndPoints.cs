using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pepegov.Identity.DAL.Domain;
using Pepegov.Identity.PL.Endpoints.Application.Queries;
using Pepegov.Identity.PL.Endpoints.Application.ViewModel;
using Pepegov.MicroserviceFramework.ApiResults;
using Pepegov.MicroserviceFramework.AspNetCore.WebApi;
using Pepegov.MicroserviceFramework.AspNetCore.WebApplicationDefinition;
using Pepegov.MicroserviceFramework.Definition;
using Pepegov.MicroserviceFramework.Definition.Context;

namespace Pepegov.Identity.PL.Endpoints.Application;

public class ApplicationEndPoints : ApplicationDefinition 
{
    public override Task ConfigureApplicationAsync(IDefinitionApplicationContext context)
    {
        var app = context.Parse<WebDefinitionApplicationContext>().WebApplication;

        app.MapGet("~/api/admin/auth/application", Get)
            .WithOpenApi()
            .WithTags("Auth.Application");
        app.MapDelete("~/api/admin/auth/application", Delete)
            .WithOpenApi()
            .WithTags("Auth.Application");
        app.MapPut("~/api/admin/auth/application/data", UpdateApplication)
            .WithOpenApi()
            .WithTags("Auth.Application");
        app.MapPost("~/api/admin/auth/application/create", Create)
            .WithOpenApi()
            .WithTags("Auth.Application");
        app.MapPut("~/api/admin/auth/application/permission", UpdatePermissions)
            .WithOpenApi()
            .WithTags("Auth.Application");
        app.MapGet("~/api/admin/auth/application/permission/all", GetPermissions)
            .WithOpenApi()
            .WithTags("Auth.Application");
        
        return base.ConfigureApplicationAsync(context);
    }

    [ProducesResponseType(200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404, Type = typeof(MinimalExceptionData))]
    [Authorize(AuthenticationSchemes = AuthData.AuthenticationSchemes, Roles = UserRoles.Admin)]
    private async Task<IResult> UpdateApplication(
        HttpContext httpContext,
        [FromHeader] bool? Bff,
        [FromServices] IMediator mediator,
        [FromQuery] string clientId,
        [FromBody] ApplicationDataUpdateModel model)
    {
        var result = await mediator.Send(new UpdateApplicationDataCommand(clientId, model), httpContext.RequestAborted);
        return Results.Extensions.Custom(result);
    }
    
    [ProducesResponseType(200, Type = typeof(List<string>))]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404, Type = typeof(MinimalExceptionData))]
    [ProducesResponseType(500, Type = typeof(MinimalExceptionData))]
    [Authorize(AuthenticationSchemes = AuthData.AuthenticationSchemes, Roles = UserRoles.Admin)]
    private async Task<IResult> GetPermissions(
        HttpContext httpContext,
        [FromHeader] bool? Bff,
        [FromServices] IMediator mediator,
        [FromQuery] string clientId)
    {
        var result = await mediator.Send(new GetApplicationPermissionsRequest(clientId), httpContext.RequestAborted);
        return Results.Extensions.Custom(result);
    }
    
    [ProducesResponseType(200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(409, Type = typeof(MinimalExceptionData))]
    [Authorize(AuthenticationSchemes = AuthData.AuthenticationSchemes, Roles = UserRoles.Admin)]
    private async Task<IResult> UpdatePermissions(
        HttpContext httpContext,
        [FromHeader] bool? Bff,
        [FromServices] IMediator mediator,
        [FromQuery] string clientId,
        [FromBody] List<string> permissions)
    {
        var result = await mediator.Send(new UpdateApplicationPermissionsCommand(clientId, permissions), httpContext.RequestAborted);
        return Results.Extensions.Custom(result);
    }
    
    [ProducesResponseType(200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(409, Type = typeof(MinimalExceptionData))]
    [Authorize(AuthenticationSchemes = AuthData.AuthenticationSchemes, Roles = UserRoles.Admin)]
    private async Task<IResult> Create(
        HttpContext httpContext,
        [FromHeader] bool? Bff,
        [FromServices] IMediator mediator,
        [FromBody] CreateApplicationCommand application)
    {
        var result = await mediator.Send(application, httpContext.RequestAborted);
        return Results.Extensions.Custom(result);
    }

    [ProducesResponseType(200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404, Type = typeof(MinimalExceptionData))]
    [Authorize(AuthenticationSchemes = AuthData.AuthenticationSchemes, Roles = UserRoles.Admin)]
    private async Task<IResult> Delete(
        HttpContext httpContext,
        [FromHeader] bool? Bff,
        [FromServices] IMediator mediator,
        [FromQuery] string clientId)
    {
        var result = await mediator.Send(new DeleteApplicationCommand(clientId), httpContext.RequestAborted);
        return Results.Extensions.Custom(result);
    }
    
    [ProducesResponseType(200, Type = typeof(ApplicationViewModel))]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404, Type = typeof(MinimalExceptionData))]
    [Authorize(AuthenticationSchemes = AuthData.AuthenticationSchemes, Roles = UserRoles.Admin)]
    private async Task<IResult> Get(
        HttpContext httpContext,
        [FromHeader] bool? Bff,
        [FromServices] IMediator mediator,
        [FromQuery] string clientId)
    {
        var result = await mediator.Send(new GetApplicationViewModelRequest(clientId), httpContext.RequestAborted);
        return Results.Extensions.Custom(result);
    }
}