using System.Net.Http.Json;
using Task1.Interfaces;
using Task1.Models;

namespace Task1.Implementations;

public class HttpClientConfigurationClient : IConfigurationClient
{
    private readonly HttpClient _httpClient;

    public HttpClientConfigurationClient(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient("ConfigurationServiceClient");
    }

    public async Task<QueryConfigurationsResponse> GetConfigurationsAsync(
        int pageSize, string? pageToken, CancellationToken cancellationToken)
    {
        string query = $"configurations?pageSize={pageSize}&pageToken={pageToken}";

        HttpResponseMessage response = await _httpClient.GetAsync(query, cancellationToken).ConfigureAwait(false);

        response.EnsureSuccessStatusCode();

        QueryConfigurationsResponse? configuration =
            await response.Content
                .ReadFromJsonAsync<QueryConfigurationsResponse>(cancellationToken)
                .ConfigureAwait(false);

        return configuration ?? throw new Exception("No data received");
    }
}