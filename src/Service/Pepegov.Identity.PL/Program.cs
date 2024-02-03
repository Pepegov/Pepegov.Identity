using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
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

            #region TEST

            IConfiguration testConfiguration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();
    
            Log.Logger.Information($"Connection string | {testConfiguration.GetConnectionString("DefaultConnection")}");
            Log.Logger.Information("$RABBITMQ_PASSWORD");
            Log.Logger.Information(Environment.GetEnvironmentVariable("$RABBITMQ_PASSWORD"));

            #endregion
            
            //Create builder
            var builder = WebApplication.CreateBuilder(args);
            
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