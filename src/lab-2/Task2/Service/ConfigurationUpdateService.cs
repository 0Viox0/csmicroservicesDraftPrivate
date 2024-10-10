using Task1.Interfaces;
using Task1.Models;
using Task2.ConfigurationProviders;

namespace Task2.Service;

public class ConfigurationUpdateService : IDisposable
{
    private readonly CustomConfigurationProvider _customConfigurationProvider;
    private readonly IConfigurationClient _configurationClient;
    private readonly PeriodicTimer _periodicTimer;
    private readonly TimeSpan _periodicTimeInterval;

    public void Dispose()
    {
        _periodicTimer.Dispose();
    }

    public ConfigurationUpdateService(
        CustomConfigurationProvider customConfigurationProvider,
        IConfigurationClient configurationClient,
        TimeSpan periodicTimeInterval)
    {
        _customConfigurationProvider = customConfigurationProvider;
        _configurationClient = configurationClient;
        _periodicTimeInterval = periodicTimeInterval;
        _periodicTimer = new PeriodicTimer(_periodicTimeInterval);
    }

    public async Task StartUpdateConfigurationServiceAsync(
        int pageSize,
        string pageToken,
        CancellationToken cancellationToken)
    {
        while (await _periodicTimer.WaitForNextTickAsync().ConfigureAwait(false))
        {
            await UpdateConfiguration(pageSize, pageToken, cancellationToken).ConfigureAwait(false);
        }
    }

    private async Task UpdateConfiguration(int pageSize, string pageToken, CancellationToken cancellationToken)
    {
        QueryConfigurationsResponse respose = await _configurationClient
            .GetConfigurationsAsync(pageSize, pageToken, cancellationToken)
            .ConfigureAwait(false);

        _customConfigurationProvider.UpdateConfiguration(respose.Items);
    }
}