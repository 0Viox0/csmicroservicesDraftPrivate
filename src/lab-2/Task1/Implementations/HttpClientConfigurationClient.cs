using System.Net.Http.Json;
using Task1.Interfaces;
using Task1.Mappers;
using Task1.Models.ApplicationConfigurationModels;
using Task1.Models.ClientResponseModels;

namespace Task1.Implementations;

public class HttpClientConfigurationClient : IConfigurationClient
{
    private readonly HttpClient _httpClient;
    private readonly ApiApplicationModelConfigurationMapper _apiApplicationModelConfigurationMapper;

    public HttpClientConfigurationClient(
        IHttpClientFactory httpClientFactory,
        ApiApplicationModelConfigurationMapper apiApplicationModelConfigurationMapper)
    {
        _apiApplicationModelConfigurationMapper = apiApplicationModelConfigurationMapper;
        _httpClient = httpClientFactory.CreateClient("ConfigurationServiceClient");
    }

    public async Task<ConfigurationKeyValueCollectionWIthPageToken> GetConfigurationsAsync(
        int pageSize, string? pageToken, CancellationToken cancellationToken)
    {
        string query = $"configurations?pageSize={pageSize}&pageToken={pageToken}";

        HttpResponseMessage response = await _httpClient.GetAsync(query, cancellationToken).ConfigureAwait(false);

        response.EnsureSuccessStatusCode();

        ExternalQueryConfigurationsResponse? configurationsResponse =
            await response.Content
                .ReadFromJsonAsync<ExternalQueryConfigurationsResponse>(cancellationToken)
                .ConfigureAwait(false);

        if (configurationsResponse is null)
            throw new Exception("No data received");

        return _apiApplicationModelConfigurationMapper
            .ToConfigurationKeyValueCollectionWIthPageToken(configurationsResponse);
    }
}