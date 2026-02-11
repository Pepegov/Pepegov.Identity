using AegisForge.Application;
using AegisForge.Domain.Database;
using Pepegov.Identity.BL;

namespace AegisForge.PL.Jobs;

public class DatabaseSeedingWorker(IServiceProvider serviceProvider) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetService<ApplicationDbContext>()!;

        await new DatabaseInitializer(serviceProvider, dbContext).SeedAsync(cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}