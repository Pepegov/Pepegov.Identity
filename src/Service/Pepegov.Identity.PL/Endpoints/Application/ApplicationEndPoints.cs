using MediatR;
using Microsoft.AspNetCore.Mvc;
using OpenIddict.EntityFrameworkCore.Models;
using Pepegov.Identity.PL.Endpoints.Application.Handlers.Queries;
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

        app.MapGet("~/api/admin/auth/application/", Get).WithOpenApi();
        return base.ConfigureApplicationAsync(context);
    }

    private async Task<IResult> Get(
        [FromServices] IMediator mediator,
        [FromQuery] string clientId)
    {
        var result = await mediator.Send(new GetApplicationRequest(clientId));
        return Results.Extensions.Custom(result);
    }
}