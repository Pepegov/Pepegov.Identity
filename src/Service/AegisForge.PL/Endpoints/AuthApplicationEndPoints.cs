using AegisForge.Application.Dto;
using AegisForge.Application.Query.AuthApplication;
using AegisForge.Domain.Domain;
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

public class AuthApplicationEndPoints : ApplicationDefinition 
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
        var result = await mediator.Send(new AuthApplicationUpdateCommand(clientId, model), httpContext.RequestAborted);
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
        var result = await mediator.Send(new AuthApplicationGetPermissionsRequest(clientId), httpContext.RequestAborted);
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
        var result = await mediator.Send(new AuthApplicationUpdatePermissionsCommand(clientId, permissions), httpContext.RequestAborted);
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
        [FromBody] AuthApplicationCreationCommand authApplicationCreation)
    {
        var result = await mediator.Send(authApplicationCreation, httpContext.RequestAborted);
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
        var result = await mediator.Send(new AuthApplicationDeleteCommand(clientId), httpContext.RequestAborted);
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
        var result = await mediator.Send(new AuthApplicationGetRequest(clientId), httpContext.RequestAborted);
        return Results.Extensions.Custom(result);
    }
}