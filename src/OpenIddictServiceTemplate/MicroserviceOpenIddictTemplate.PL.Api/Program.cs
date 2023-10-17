using Microsoft.IdentityModel.Logging;
using Pepegov.MicroserviceFramework.AspNetCore.WebApplicationDefinition;
using Serilog;
using Serilog.Events;

try
{
    //Configure logging
    Log.Logger = new LoggerConfiguration()
        .MinimumLevel.Debug()
        .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
        .Enrich.FromLogContext()
        .WriteTo.Console()
        .CreateLogger();
    
    //Create builder
    var builder = WebApplication.CreateBuilder(args);
    
    //Host logging  
    builder.Host.UseSerilog((context, configuration) =>
        configuration.ReadFrom.Configuration(context.Configuration));

    //Add definitions
    var currentAssembly = typeof(Program).Assembly;
    builder.AddApplicationDefinitions(currentAssembly);

    //Create web application
    var app = builder.Build();
    
    //Use definitions
    app.UseApplicationDefinitions();
    
    //Use logging
    if (app.Environment.IsDevelopment())
    {
        IdentityModelEventSource.ShowPII = true;
    }
    app.UseSerilogRequestLogging();
    
    //Run app
    app.Run();

    return 0;
}
catch (Exception ex)
{
    var type = ex.GetType().Name;
    if (type.Equals("HostAbortedException", StringComparison.Ordinal))
    {
        throw;
    }

    Console.WriteLine(ex.Message);
    Log.Fatal(ex, "Unhandled exception");
    return 1;
}
finally
{
    Log.CloseAndFlush();
}