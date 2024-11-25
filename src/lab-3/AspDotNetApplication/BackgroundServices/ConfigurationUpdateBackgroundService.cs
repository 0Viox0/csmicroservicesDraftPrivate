using Task2.Service;

namespace Task1.BackgroundServices;

public class ConfigurationUpdateBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;

    public ConfigurationUpdateBackgroundService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using IServiceScope scope = _serviceProvider.CreateScope();
        ConfigurationUpdateService configurationUpdateService =
            scope.ServiceProvider.GetRequiredService<ConfigurationUpdateService>();

        if (!stoppingToken.IsCancellationRequested)
        {
            await configurationUpdateService.StartUpdateConfigurationServiceAsync(10, string.Empty, stoppingToken);
        }
    }
}