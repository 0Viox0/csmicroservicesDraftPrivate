using Microsoft.Extensions.Options;
using Task1.ConfigurationModels;
using Task3.Bll;

namespace Task1.BackgroundServices;

public class TestBackgroundService : BackgroundService
{
    private readonly IOptionsMonitor<DatabaseSettings> _databaseSettings;
    private readonly IOptions<ExternalConnectionInfo> _externalConnectionInfo;

    public TestBackgroundService(
        IOptionsMonitor<DatabaseSettings> databaseSettings,
        IOptions<ExternalConnectionInfo> externalConnectionInfo)
    {
        _databaseSettings = databaseSettings;
        _externalConnectionInfo = externalConnectionInfo;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(1));

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            Console.Out.WriteLine($"External connection info:\n" +
                                  $"Host: {_externalConnectionInfo.Value.Host}\n" +
                                  $"Port: {_externalConnectionInfo.Value.Port}");

            Console.Out.WriteLine($"Database connection info:\n" +
                                  $"Host: {_databaseSettings.CurrentValue.Host}\n" +
                                  $"Port: {_databaseSettings.CurrentValue.Port}");
        }
    }
}