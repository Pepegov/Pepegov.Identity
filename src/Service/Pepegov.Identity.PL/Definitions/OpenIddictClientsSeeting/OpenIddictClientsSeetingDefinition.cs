using Microsoft.Extensions.Options;
using OpenIddict.Abstractions;
using Pepegov.Identity.DAL.Database;
using Pepegov.Identity.PL.Definitions.Options.Models;
using Pepegov.MicroserviceFramework.Definition;
using Pepegov.MicroserviceFramework.Definition.Context;

namespace Pepegov.Identity.PL.Definitions.OpenIddictClientsSeeting;

public class OpenIddictClientsSeetingDefinition : ApplicationDefinition
{
    public override async Task ConfigureApplicationAsync(IDefinitionApplicationContext context)
    { 
        using var scope = context.ServiceProvider.CreateScope();
        
        IConfiguration identityConfiguration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("identitysetting.json")
            .Build();

        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await dbContext.Database.EnsureCreatedAsync();

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