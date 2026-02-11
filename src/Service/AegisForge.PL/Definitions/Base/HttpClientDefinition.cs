using AegisForge.Infrastructure.Options;
using Pepegov.MicroserviceFramework.Definition;
using Pepegov.MicroserviceFramework.Definition.Context;

namespace TelecomService.QRON.ID.Identity.Api.Definitions.Service;

public class HttpClientDefinition : ApplicationDefinition
{
    public override Task ConfigureServicesAsync(IDefinitionServiceContext context)
    {
        var ipApiClientOption = context.Configuration.GetSection("HttpClient").GetSection("IpApi").Get<HttpClientIpApiOption>();
        ArgumentNullException.ThrowIfNull(ipApiClientOption);
        
        context.ServiceCollection.AddHttpClient("IpApiClient",client =>
        {
            client.BaseAddress = new Uri(ipApiClientOption.BaseUrl);
            client.Timeout = TimeSpan.FromMilliseconds(ipApiClientOption.TimeoutFromMicroseconds);
        });
        
        return base.ConfigureServicesAsync(context);
    }
}