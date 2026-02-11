using AegisForge.Application.AuthorizationStrategy;
using AegisForge.Application.GrandType.Infrastructure;
using Pepegov.MicroserviceFramework.Definition;
using Pepegov.MicroserviceFramework.Definition.Context;

namespace AegisForge.PL.Definitions.Identity;

public class GrantTypeConnectionDefinition : ApplicationDefinition
{
    public override Task ConfigureServicesAsync(IDefinitionServiceContext context)
    {
        var grantTypeConnectionHandlerTypes = typeof(IGrantTypeConnection).Assembly.GetTypes()
            .Where(x =>
            x.GetInterfaces().Contains(typeof(IGrantTypeConnection))  && x is { IsClass: true, IsAbstract: false }).ToList();

        foreach (var type in grantTypeConnectionHandlerTypes)
        {
            context.ServiceCollection.AddScoped(typeof(IGrantTypeConnection), type);
        }
        
        context.ServiceCollection.AddScoped<GrantTypeConnectionManager>(provider =>
            new GrantTypeConnectionManager(provider.GetServices<IGrantTypeConnection>()));
        context.ServiceCollection.AddScoped<AuthorizationStrategy>();
        
        return base.ConfigureServicesAsync(context);
    }
}