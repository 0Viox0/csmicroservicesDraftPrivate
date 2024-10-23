using FluentMigrator.Runner;

namespace Task1.BackgroundServices;

public class MigrationBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;

    public MigrationBackgroundService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using IServiceScope scope = _serviceProvider.CreateScope();

        IMigrationRunner migrationRunner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();

        migrationRunner.MigrateUp();

        return Task.CompletedTask;
    }
}