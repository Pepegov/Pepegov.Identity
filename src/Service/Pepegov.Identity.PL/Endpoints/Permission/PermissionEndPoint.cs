using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pepegov.Identity.DAL.Domain;
using Pepegov.Identity.PL.Endpoints.Permission.Queries;
using Pepegov.Identity.PL.Endpoints.Permission.ViewModel;
using Pepegov.MicroserviceFramework.AspNetCore.WebApi;
using Pepegov.MicroserviceFramework.AspNetCore.WebApplicationDefinition;
using Pepegov.MicroserviceFramework.Definition;
using Pepegov.MicroserviceFramework.Definition.Context;

namespace Pepegov.Identity.PL.Endpoints.Permission;

public class PermissionEndPoint : ApplicationDefinition
{
    public override Task ConfigureApplicationAsync(IDefinitionApplicationContext context)
    {
        var app = context.Parse<WebDefinitionApplicationContext>().WebApplication;
        
        app.MapGet("~/api/permission/admin/all", GetAll)
            .WithOpenApi()
            .WithTags("Accesses.Permission")
            .WithSummary("Get all list of permission (heavy load with big data)")
            .WithDescription($"Policy = {AppPermissions.PermissionsAdmin}");
        app.MapGet("~/api/permission/admin/by-id", GetById)
            .WithOpenApi()
            .WithTags("Accesses.Permission")
            .WithSummary("Get permission by id")
            .WithDescription($"Policy = {AppPermissions.PermissionsAdmin}");
        app.MapPost("~/api/permission/admin/create", Insert)
            .WithOpenApi()
            .WithTags("Accesses.Permission")
            .WithSummary("Create permission")
            .WithDescription($"Policy = {AppPermissions.PermissionsAdmin}");
        app.MapPost("~/api/permission/admin/update", Update)
            .WithOpenApi()
            .WithTags("Accesses.Permission")
            .WithSummary("Update permission")
            .WithDescription($"Policy = {AppPermissions.PermissionsAdmin}");
        app.MapPost("~/api/permission/admin/add-to-account", AddToAccount)
            .WithOpenApi()
            .WithTags("Accesses.Permission")
            .WithSummary("Add permission to user account by accountId")
            .WithDescription($"Policy = {AppPermissions.PermissionsAdmin}");;
        app.MapPost("~/api/permission/admin/remove-from-account", RemoveFromAccount)
            .WithOpenApi()
            .WithTags("Accesses.Permission")
            .WithSummary("Remove permission from account by accountId")
            .WithDescription($"Policy = {AppPermissions.PermissionsAdmin}");;
        
        return base.ConfigureApplicationAsync(context);
    }

    [ProducesResponseType(200, Type = typeof(IList<PermissionViewModel>))]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [Authorize(AuthenticationSchemes = AuthData.AuthenticationSchemes, Policy = AppPermissions.PermissionsAdmin)]
    private static async Task<IResult> GetAll(
        HttpContext httpContent,
        [FromServices] IMediator mediator)
    {
        var result = await mediator.Send(new GetAllPermissionsRequest(), httpContent.RequestAborted);
        return Results.Extensions.Custom(result);
    }

    [ProducesResponseType(200, Type = typeof(PermissionViewModel))]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [Authorize(AuthenticationSchemes = AuthData.AuthenticationSchemes, Policy = AppPermissions.PermissionsAdmin)]
    private static async Task<IResult> GetById(
        [FromQuery] Guid id,
        [FromServices] IMediator mediator)
    {
        var result = await mediator.Send(new GetPermissionByIdRequest(id));
        return Results.Extensions.Custom(result);
    }

    [ProducesResponseType(200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [Authorize(AuthenticationSchemes = AuthData.AuthenticationSchemes, Policy = AppPermissions.PermissionsAdmin)]
    private static async Task<IResult> Insert(
        [FromBody] PermissionViewModel model,
        [FromServices] IMediator mediator)
    {
        var result = await mediator.Send(new InsertPermissionCommand(model));
        return Results.Extensions.Custom(result);
    }

    [ProducesResponseType(200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [Authorize(AuthenticationSchemes = AuthData.AuthenticationSchemes, Policy = AppPermissions.PermissionsAdmin)]
    private static async Task<IResult> Update(
        [FromQuery] Guid id,
        [FromBody] PermissionViewModel model,
        [FromServices] IMediator mediator)
    {
        var result = await mediator.Send(new UpdatePermissionCommand(id, model));
        return Results.Extensions.Custom(result);
    }

    [ProducesResponseType(200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [Authorize(AuthenticationSchemes = AuthData.AuthenticationSchemes, Policy = AppPermissions.PermissionsAdmin)]
    private static async Task<IResult> AddToAccount(
        [FromBody] ProfilePermissionViewModel model,
        [FromServices] IMediator mediator)
    {
        var result = await mediator.Send(new AddPermissionToAccountCommand(model));
        return Results.Extensions.Custom(result);
    }

    [ProducesResponseType(200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [Authorize(AuthenticationSchemes = AuthData.AuthenticationSchemes, Policy = AppPermissions.PermissionsAdmin)]
    private static async Task<IResult> RemoveFromAccount(
        [FromBody] ProfilePermissionViewModel model,
        [FromServices] IMediator mediator)
    {
        var result = await mediator.Send(new RemovePermissionFromAccountCommand(model));
        return Results.Extensions.Custom(result);
    }
}