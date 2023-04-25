using MicroserviceOpenIddictTemplate.DAL.Models.Options;
using MicroserviceOpenIddictTemplate.Identity.Base.Definition;
using MicroserviceOpenIddictTemplate.Identity.Definitions.Options.Models;

namespace MicroserviceOpenIddictTemplate.Identity.Definitions.Options;

public class OptionsDefinition : Definition
{
    public override void ConfigureServicesAsync(IServiceCollection services, WebApplicationBuilder builder)
    {
        var configurationBuilder = new ConfigurationBuilder();
        configurationBuilder.SetBasePath(Directory.GetCurrentDirectory());
        configurationBuilder.AddJsonFile("identitysetting.json");
        IConfiguration identityConfiguration = configurationBuilder.Build();

        services.Configure<IdentityAddressOption>(identityConfiguration.GetSection("IdentityServerUrl"));
        services.Configure<IdentityClientOption>(identityConfiguration.GetSection("CurrentIdentityClient"));
        services.Configure<AdminUser>(builder.Configuration.GetSection("AdminUser"));
    }
}
