using Task1.Interfaces;
using Task1.Mappers;
using Task1.Models.ApplicationConfigurationModels;
using Task1.Models.ClientResponseModels;

namespace Task1.Implementations;

public class RefitConfigurationClient : IConfigurationClient
{
    private readonly IRefitConfigurationClient _refitConfigurationClient;
    private readonly ApiApplicationModelConfigurationMapper _apiApplicationModelConfigurationMapper;

    public RefitConfigurationClient(
        IRefitConfigurationClient refitConfigurationClient,
        ApiApplicationModelConfigurationMapper apiApplicationModelConfigurationMapper)
    {
        _refitConfigurationClient = refitConfigurationClient;
        _apiApplicationModelConfigurationMapper = apiApplicationModelConfigurationMapper;
    }

    public async Task<ConfigurationKeyValueCollectionWIthPageToken> GetConfigurationsAsync(
        int pageSize, string? pageToken, CancellationToken cancellationToken)
    {
        ExternalQueryConfigurationsResponse response = await _refitConfigurationClient
            .GetConfigurationsAsync(pageSize, pageToken, cancellationToken);

        return _apiApplicationModelConfigurationMapper
            .ToConfigurationKeyValueCollectionWIthPageToken(response);
    }
}