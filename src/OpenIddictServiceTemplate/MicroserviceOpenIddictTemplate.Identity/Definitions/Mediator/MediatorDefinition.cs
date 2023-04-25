using MediatR;
using MicroserviceOpenIddictTemplate.Identity.Base.Definition;

namespace MicroserviceOpenIddictTemplate.Identity.Definitions.Mediator;

/// <summary>
/// Register Mediator as application definition
/// </summary>
public class MediatorDefinition : Definition
{
    public override void ConfigureServicesAsync(IServiceCollection services, WebApplicationBuilder builder)
    {
        //services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidatorBehavior<,>));
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<Program>());
    }
}