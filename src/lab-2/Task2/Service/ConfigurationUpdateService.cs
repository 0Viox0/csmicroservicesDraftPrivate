using Task1.Interfaces;
using Task1.Models.ApplicationConfigurationModels;
using Task2.ConfigurationProviders;

namespace Task2.Service;

public class ConfigurationUpdateService : IDisposable
{
    private readonly CustomConfigurationProvider _customConfigurationProvider;
    private readonly IConfigurationClient _configurationClient;
    private readonly PeriodicTimer _periodicTimer;

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
        _periodicTimer = new PeriodicTimer(periodicTimeInterval);
    }

    public async Task StartUpdateConfigurationServiceAsync(
        int pageSize,
        string pageToken,
        CancellationToken cancellationToken)
    {
        while (await _periodicTimer.WaitForNextTickAsync(cancellationToken).ConfigureAwait(false))
        {
            await UpdateConfiguration(pageSize, pageToken, cancellationToken).ConfigureAwait(false);
        }
    }

    public async Task UpdateConfiguration(int pageSize, string pageToken, CancellationToken cancellationToken)
    {
        ConfigurationKeyValueCollectionWIthPageToken response = await _configurationClient
            .GetConfigurationsAsync(pageSize, pageToken, cancellationToken)
            .ConfigureAwait(false);

        _customConfigurationProvider.UpdateConfiguration(response.ConfigurationKeyValueItems);
    }
}