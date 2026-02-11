using AegisForge.Application.Mapping;
using Pepegov.MicroserviceFramework.AspNetCore.WebApplicationDefinition;
using Pepegov.MicroserviceFramework.Definition;
using Pepegov.MicroserviceFramework.Definition.Context;

namespace AegisForge.PL.Definitions.Mapping;

public class AutoMapperDefinition : ApplicationDefinition
{
    public override Task ConfigureApplicationAsync(IDefinitionApplicationContext context)
    {
        var app = context.Parse<WebDefinitionApplicationContext>().WebApplication;
        
        var mapper = app.Services.GetRequiredService<AutoMapper.IConfigurationProvider>();
        if (!app.Environment.IsProduction())
        {
            mapper.AssertConfigurationIsValid();
        }
        else
        {
            mapper.CompileMappings();
        }
        
        return base.ConfigureApplicationAsync(context);
    }

    public override Task ConfigureServicesAsync(IDefinitionServiceContext context)
    {
        context.ServiceCollection.AddAutoMapper(typeof(SessionMappingProfile));
        context.ServiceCollection.AddAutoMapper(typeof(PagedListMappingProfile));
        return base.ConfigureServicesAsync(context);
    }
}