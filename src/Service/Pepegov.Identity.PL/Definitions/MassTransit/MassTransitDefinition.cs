using System.Reflection;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Pepegov.MicroserviceFramework.Definition;
using Pepegov.MicroserviceFramework.Definition.Context;
using Pepegov.MicroserviceFramework.Exceptions;

namespace Pepegov.Identity.PL.Definitions.MassTransit;

public class MassTransitDefinition : ApplicationDefinition
{
    public override bool Enabled => false;

    public override Task ConfigureServicesAsync(IDefinitionServiceContext context)
    {
        context.ServiceCollection.AddMassTransit(x =>
        {
            x.SetKebabCaseEndpointNameFormatter();
            x.SetInMemorySagaRepositoryProvider();
            
            var assembly = Assembly.GetEntryAssembly();
            var setting = context.Configuration.GetSection("RabbitMQ").Get<MassTransitOption>();
            if (setting is null)
            {
                throw new MicroserviceArgumentNullException("MassTransit setting is null");
            }
            
            x.AddConsumers(assembly);
            x.AddSagaStateMachines(assembly);
            x.AddSagas(assembly);
            x.AddActivities(assembly);
            
            x.UsingRabbitMq((context, cfg) =>
            {

                cfg.Host($"rabbitmq://{setting.Url}/{setting.Host}", h =>
                {
                    h.Username(setting.User);
                    h.Password(setting.Password); 
                });

                cfg.ConfigureEndpoints(context);
            });

        });
        return base.ConfigureServicesAsync(context);
    }
}