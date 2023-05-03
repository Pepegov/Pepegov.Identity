using MediatR;
using MicroserviceOpenIddictTemplate.DAL.Domain;
using MicroserviceOpenIddictTemplate.DAL.Models.Identity;
using MicroserviceOpenIddictTemplate.Identity.Endpoints.Permission.Queries;
using MicroserviceOpenIddictTemplate.Identity.Endpoints.Permission.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pepegov.MicroserviceFramerwork.AspNetCore.Definition;
using Pepegov.MicroserviceFramerwork.Attributes;
using Pepegov.MicroserviceFramerwork.ResultWrapper;

namespace MicroserviceOpenIddictTemplate.Identity.Endpoints.Permission;

public class PermissionEndPoint : Definition
{
    public override void ConfigureApplicationAsync(WebApplication app)
    {
        app.MapGet("~/api/permission/getall", GetAll).WithOpenApi();
        app.MapGet("~/api/permission/getbyid", GetById).WithOpenApi();
        app.MapPost("~/api/permission/insert", Insert).WithOpenApi();
        app.MapPost("~/api/permission/update", Update).WithOpenApi();
        app.MapPost("~/api/permission/addtoprofile", AddToProfile).WithOpenApi();
        app.MapPost("~/api/permission/removeinprofile", RemoveInProfile).WithOpenApi();
    }

    [ProducesResponseType(200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [FeatureGroupName("Permission")]
    [Authorize(AuthenticationSchemes = AuthData.AuthenticationSchemes, Policy = "Permission:Get")]
    private static async Task<ResultWrapper<IList<ApplicationPermission>>> GetAll(
        [FromServices] IMediator mediator)
        => await mediator.Send(new GetAllRequest());

    [ProducesResponseType(200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [FeatureGroupName("Permission")]
    [Authorize(AuthenticationSchemes = AuthData.AuthenticationSchemes, Policy = "Permission:Get")]
    private static async Task<ResultWrapper<ApplicationPermission>> GetById(
        [FromQuery] Guid id,
        [FromServices] IMediator mediator)
    => await mediator.Send(new GetByIdRequest(id));
    
    [ProducesResponseType(200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [FeatureGroupName("Permission")]
    [Authorize(AuthenticationSchemes = AuthData.AuthenticationSchemes, Policy = "Permission:Change")]
    private static async Task<ResultWrapper<ApplicationPermission>> Insert(
        [FromBody] PermissionViewModel model,
        [FromServices] IMediator mediator)
    => await mediator.Send(new InsertRequest(model));

    [ProducesResponseType(200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [FeatureGroupName("Permission")]
    [Authorize(AuthenticationSchemes = AuthData.AuthenticationSchemes, Policy = "Permission:Change")]
    private static async Task<ResultWrapper<ApplicationPermission>> Update(
        [FromQuery] Guid id,
        [FromBody] PermissionViewModel model,
        [FromServices] IMediator mediator)
        => await mediator.Send(new UpdateRequest(id, model));

    [ProducesResponseType(200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [FeatureGroupName("Permission")]
    [Authorize(AuthenticationSchemes = AuthData.AuthenticationSchemes, Policy = "Permission:Profile")]
    private static async Task<ResultWrapper<ProfilePermissionViewModel>> AddToProfile(
        [FromBody] ProfilePermissionViewModel model,
        [FromServices] IMediator mediator)
        => await mediator.Send(new AddToProfileRequest(model));

    [ProducesResponseType(200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [FeatureGroupName("Permission")]
    [Authorize(AuthenticationSchemes = AuthData.AuthenticationSchemes, Policy = "Permission:Profile")]
    private static async Task<ResultWrapper<ProfilePermissionViewModel>> RemoveInProfile(
        [FromBody] ProfilePermissionViewModel model,
        [FromServices] IMediator mediator)
        => await mediator.Send(new RemoveInProfileRequest(model));
}