using Task1.Models.ApplicationConfigurationModels;

namespace Task1.Interfaces;

public interface IConfigurationClient
{
    Task<ConfigurationKeyValueCollectionWIthPageToken> GetConfigurationsAsync(
        int pageSize, string? pageToken, CancellationToken cancellationToken);
}