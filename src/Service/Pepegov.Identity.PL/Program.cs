using Microsoft.IdentityModel.Logging;
using Pepegov.MicroserviceFramework.AspNetCore.WebApplicationDefinition;
using Serilog;
using Serilog.Events;

namespace Pepegov.Identity.PL;

internal class Program
{
    public static async Task Main(string[] args)
    {
        try
        {
            //Configure logging
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .CreateLogger();
            
            Log.Logger.Information("$BUILD_CONFIGURATION");
            Log.Logger.Information("%BUILD_CONFIGURATION");
            Log.Logger.Information("%BUILD_CONFIGURATION%");
            Log.Logger.Information("${{BUILD_CONFIGURATION}}");
             Log.Logger.Information("%{{BUILD_CONFIGURATION}}");
            
            //Create builder
            var builder = WebApplication.CreateBuilder(args);
            
            // Configure env
            builder.WebHost.ConfigureAppConfiguration((context, configurationBuilder) =>
            {
                var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";
                context.Configuration = configurationBuilder.SetBasePath(System.IO.Directory.GetCurrentDirectory())
                    .AddJsonFile($"appsettings.json", optional: false)
                    .AddJsonFile($"appsettings.{env}.json", optional: true)
                    .AddEnvironmentVariables()
                    .Build();
            });

            builder.Configuration.AddEnvironmentVariables().Build();
            
            Log.Logger.Information(builder.Configuration.GetConnectionString("DefaultConnection"));
            
            //Host logging  
            builder.Host.UseSerilog((context, configuration) =>
                configuration.ReadFrom.Configuration(context.Configuration));
        
            //Add definitions
            var currentAssembly = typeof(Program).Assembly;
            await builder.AddApplicationDefinitions(currentAssembly);
        
            //Create web application
            var app = builder.Build();
            
            //Use definitions
            await app.UseApplicationDefinitions();
            
            //Use logging
            if (app.Environment.IsDevelopment())
            {
                IdentityModelEventSource.ShowPII = true;
            }
            app.UseSerilogRequestLogging();
            
            //Run app
            await app.RunAsync();

            //return 1;
        }
        catch (Exception ex)
        {
            var type = ex.GetType().Name;
            if (type.Equals("HostAbortedException", StringComparison.Ordinal))
            {
                throw;
            }
        
            await Console.Out.WriteLineAsync(ex.Message);
            Log.Fatal(ex, "Unhandled exception");
        }
        finally
        {
            await Log.CloseAndFlushAsync();
        }
        //return 0;
    }
}