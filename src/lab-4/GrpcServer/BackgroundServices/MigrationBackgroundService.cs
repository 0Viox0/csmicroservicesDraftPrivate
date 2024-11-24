using FluentMigrator.Runner;
using Task2.Service;

namespace GrpcServer.BackgroundServices;

public class MigrationBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;

    public MigrationBackgroundService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        await UpdateFirstConfiguration(cancellationToken);
        await base.StartAsync(cancellationToken);
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using IServiceScope scope = _serviceProvider.CreateScope();

        IMigrationRunner migrationRunner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();

        migrationRunner.MigrateUp();

        return Task.CompletedTask;
    }

    private async Task UpdateFirstConfiguration(CancellationToken cancellationToken)
    {
        using IServiceScope scope = _serviceProvider.CreateScope();

        ConfigurationUpdateService configurationUpdateService =
            scope.ServiceProvider.GetRequiredService<ConfigurationUpdateService>();

        await configurationUpdateService.UpdateConfiguration(10, string.Empty, cancellationToken);
    }
}