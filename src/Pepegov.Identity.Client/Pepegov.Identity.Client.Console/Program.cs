
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenIddict.Abstractions;
using OpenIddict.Client;
using Pepegov.Identity.Client.Console;

var host = new HostBuilder()
    // Note: applications for which a single instance is preferred can reference
    // the Dapplo.Microsoft.Extensions.Hosting.AppServices package and call this
    // method to automatically close extra instances based on the specified identifier:
    //
    // .ConfigureSingleInstance(options => options.MutexId = "{C9E0D6B4-8142-4BC5-813B-12064CF4238C}")
    //
    .ConfigureLogging(options => options.AddDebug())
    .ConfigureServices(services =>
    {
        services.AddOpenIddict()
            // Register the OpenIddict client components.
            .AddClient(options =>
            {
                // Note: this sample uses the device authorization flow,
                // but you can enable the other flows if necessary.
                options.AllowDeviceCodeFlow();

                // Disable token storage, which is not necessary for the device authorization flow.
                options.DisableTokenStorage();

                // Register the signing and encryption credentials used to protect
                // sensitive data like the state tokens produced by OpenIddict.
                options.AddDevelopmentEncryptionCertificate()
                       .AddDevelopmentSigningCertificate();

                // Register the System.Net.Http integration and use the identity of the current
                // assembly as a more specific user agent, which can be useful when dealing with
                // providers that use the user agent as a way to throttle requests (e.g Reddit).
                options.UseSystemNetHttp()
                       .SetProductInformation(typeof(Program).Assembly);

                // Add a client registration matching the client application definition in the server project.
                options.AddRegistration(new OpenIddictClientRegistration
                {
                    Issuer = new Uri("https://localhost:10001/", UriKind.Absolute),

                    ClientId = "MicroserviceDevice-ID",
                    ClientSecret = "MicroserviceDevice-SECRET",
                    Scopes = { OpenIddictConstants.Scopes.Email, OpenIddictConstants.Scopes.Profile, "offline_access" }
                });
            });

        services.AddLogging(builder => builder.AddConsole().AddDebug());
        
        // Register the background service responsible for handling the console interactions.
        services.AddHostedService<Worker>();

        // Prevent the console lifetime manager from writing status messages to the output stream.
        services.Configure<ConsoleLifetimeOptions>(options => options.SuppressStatusMessages = true);
    })
    .UseConsoleLifetime()
    .Build();

await host.RunAsync();
