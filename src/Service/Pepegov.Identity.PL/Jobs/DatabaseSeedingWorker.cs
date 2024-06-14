using Pepegov.Identity.BL;
using Pepegov.Identity.DAL.Database;

namespace Pepegov.Identity.PL.Jobs;

public class DatabaseSeedingWorker : IHostedService
{
    private readonly IServiceProvider _serviceProvider;

    public DatabaseSeedingWorker(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetService<ApplicationDbContext>()!;

        await new DatabaseInitializer(_serviceProvider, dbContext).SeedAsync(cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}