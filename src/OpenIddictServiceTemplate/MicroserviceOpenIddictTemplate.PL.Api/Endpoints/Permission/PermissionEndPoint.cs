using MediatR;
using MicroserviceOpenIddictTemplate.DAL.Domain;
using MicroserviceOpenIddictTemplate.DAL.Models.Identity;
using MicroserviceOpenIddictTemplate.PL.Api.Endpoints.Permission.Queries;
using MicroserviceOpenIddictTemplate.PL.Api.Endpoints.Permission.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pepegov.MicroserviceFramework.AspNetCore.WebApi;
using Pepegov.MicroserviceFramework.AspNetCore.WebApplicationDefinition;
using Pepegov.MicroserviceFramework.Definition;
using Pepegov.MicroserviceFramework.Definition.Context;

namespace MicroserviceOpenIddictTemplate.PL.Api.Endpoints.Permission;

public class PermissionEndPoint : ApplicationDefinition
{
    public override Task ConfigureApplicationAsync(IDefinitionApplicationContext context)
    {
        var app = context.Parse<WebDefinitionApplicationContext>().WebApplication;
        
        app.MapGet("~/api/permission/admin/all", GetAll).WithOpenApi()
            .WithSummary("Get all list of permission (heavy load with big data)")
            .WithDescription($"Policy = {AppPermissions.PermissionsAdmin}");
        app.MapGet("~/api/permission/admin/by-id", GetById).WithOpenApi()
            .WithSummary("Get permission by id")
            .WithDescription($"Policy = {AppPermissions.PermissionsAdmin}");
        app.MapPost("~/api/permission/admin/create", Insert).WithOpenApi()
            .WithSummary("Create permission")
            .WithDescription($"Policy = {AppPermissions.PermissionsAdmin}");
        app.MapPost("~/api/permission/admin/update", Update).WithOpenApi()
            .WithSummary("Update permission")
            .WithDescription($"Policy = {AppPermissions.PermissionsAdmin}");
        app.MapPost("~/api/permission/admin/add-to-account", AddToAccount).WithOpenApi()
            .WithSummary("Add permission to user account by accountId")
            .WithDescription($"Policy = {AppPermissions.PermissionsAdmin}");;
        app.MapPost("~/api/permission/admin/remove-from-account", RemoveFromAccount).WithOpenApi()
            .WithSummary("Remove permission from account by accountId")
            .WithDescription($"Policy = {AppPermissions.PermissionsAdmin}");;
        
        return base.ConfigureApplicationAsync(context);
    }

    [ProducesResponseType(200, Type = typeof(IList<PermissionViewModel>))]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [Authorize(AuthenticationSchemes = AuthData.AuthenticationSchemes, Policy = AppPermissions.PermissionsAdmin)]
    private static async Task<IResult> GetAll(
        [FromServices] IMediator mediator)
    {
        var result = await mediator.Send(new GetAllPermissionsRequest());
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