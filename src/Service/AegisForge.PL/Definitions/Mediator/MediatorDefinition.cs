using AegisForge.Application.Handler;
using AegisForge.Application.Handler.Session;
using Pepegov.MicroserviceFramework.Definition;
using Pepegov.MicroserviceFramework.Definition.Context;

namespace AegisForge.PL.Definitions.Mediator;

/// <summary>
/// Register Mediator as application definition
/// </summary>
public class MediatorDefinition : ApplicationDefinition
{
    public override Task ConfigureServicesAsync(IDefinitionServiceContext context)
    {
        context.ServiceCollection.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<SessionGetPagedRequestHandler>());
        return base.ConfigureServicesAsync(context);
    }
}