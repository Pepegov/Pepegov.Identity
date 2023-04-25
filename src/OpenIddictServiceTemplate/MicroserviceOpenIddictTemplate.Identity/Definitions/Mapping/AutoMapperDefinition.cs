using AutoMapper;
using MicroserviceOpenIddictTemplate.Identity.Base.Definition;

namespace MicroserviceOpenIddictTemplate.Identity.Definitions.Mapping;

public class AutoMapperDefinition : Definition
{
    public override void ConfigureServicesAsync(IServiceCollection services, WebApplicationBuilder builder)
    {
        var mappingConfig = new MapperConfiguration(mc =>
        {
            mc.AddProfile(new MappingProfile());
        });
        IMapper mapper = mappingConfig.CreateMapper();
        services.AddSingleton(mapper);
    }
}