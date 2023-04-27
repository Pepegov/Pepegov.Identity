using System.Reflection;
using MassTransit;
using Pepegov.MicroserviceFramerwork.AspNetCore.Definition;

namespace MicroserviceOpenIddictTemplate.Identity.Definitions.MassTransit;

public class MassTransitDefinition : Definition
{
    public override void ConfigureServicesAsync(IServiceCollection services, WebApplicationBuilder builder)
    {
        services.AddMassTransit(x =>
        {
            x.SetKebabCaseEndpointNameFormatter();
            x.SetInMemorySagaRepositoryProvider();
            
            var assembly = Assembly.GetEntryAssembly();
            
            x.AddConsumers(assembly);
            x.AddSagaStateMachines(assembly);
            x.AddSagas(assembly);
            x.AddActivities(assembly);
            
            //x.AddConsumer<GetAccountByIdConsumer>();
            //x.AddConsumer<RegisterAccountConsumer>();

            x.UsingRabbitMq((context, cfg) =>
            {

                cfg.Host("rabbitmq://localhost/ ", h =>
                {
                    h.Username("rmuser");
                    h.Password("rmpassword"); 
                });
                /*cfg.ReceiveEndpoint("AccountQueue", e =>
                {
                    e.PrefetchCount = 20;
                    e.UseMessageRetry(r => r.Interval(2, 100));

                    e.Consumer<GetAccountByIdConsumer>(context);
                    e.Consumer<RegisterAccountConsumer>(context);
                });*/
                cfg.ConfigureEndpoints(context);
            });

        });
    }
}