using MicroserviceOpenIddictTemplate.DAL.Database;
using MicroserviceOpenIddictTemplate.Identity.Base.Definition;
using MicroserviceOpenIddictTemplate.Identity.Definitions.Options.Models;
using Microsoft.Extensions.Options;
using OpenIddict.Abstractions;

namespace MicroserviceOpenIddictTemplate.Identity.Definitions.OpenIddictClientsSeeting;

public class OpenIddictClientsSeetingDefinition : Definition
{
    public override bool Enabled => true;

    public override async void ConfigureApplicationAsync(WebApplication app)
    { 
        using var scope = app.Services.CreateScope();
        
        var configurationBuilder = new ConfigurationBuilder();
        configurationBuilder.SetBasePath(Directory.GetCurrentDirectory());
        configurationBuilder.AddJsonFile("identitysetting.json");
        IConfiguration identityConfiguration = configurationBuilder.Build();

        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await context.Database.EnsureCreatedAsync();

        var manager = scope.ServiceProvider.GetRequiredService<IOpenIddictApplicationManager>();

        //Get all clients
        var identityClients = identityConfiguration.GetSection("ClientsIdentity").Get<List<IdentityClientOption>>()!
                              ?? new List<IdentityClientOption>();
        
        //get current client and add all scopes in him 
        var currentIdentityClient = scope.ServiceProvider.GetService<IOptions<IdentityClientOption>>()!.Value;
        currentIdentityClient.Scopes = identityConfiguration.GetSection("Scopes").Get<List<IdentityScopeOption>>()!
            .Select(x => x.Name).ToList();
        identityClients.Add(currentIdentityClient); //current client add too other clients

        var url = scope.ServiceProvider.GetService<IOptions<IdentityAddressOption>>()!.Value.Authority;
        
        foreach (var clientOption in identityClients)
        {
            if (await manager.FindByClientIdAsync(clientOption.Id) is not null) //if the client exist then dont add him
            {
                continue;;
            }

            var client = new OpenIddictApplicationDescriptor
            {
                ClientId = clientOption.Id,
                ClientSecret = clientOption.Secret,
                DisplayName = clientOption.Name,
                ConsentType = clientOption.ConsentType,
            };
            
            client.AddScopes(clientOption.Scopes);
            client.AddGrandTypes(clientOption.GrandTypes);
            
            client.AddRedirectUris(clientOption.RedirectUris);
            client.AddRedirectUrisForTesting(url);
            
            client.AddResponseTypes();
            client.AddEndpoints();

            await manager.CreateAsync(client);
        }
    }
}