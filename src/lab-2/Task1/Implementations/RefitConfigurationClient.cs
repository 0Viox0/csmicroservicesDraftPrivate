using Task1.Interfaces;
using Task1.Models;

namespace Task1.Implementations;

public class RefitConfigurationClient : IConfigurationClient
{
    private readonly IRefitConfigurationClient _refitConfigurationClient;

    public RefitConfigurationClient(IRefitConfigurationClient refitConfigurationClient)
    {
        _refitConfigurationClient = refitConfigurationClient;
    }

    public async Task<QueryConfigurationsResponse> GetConfigurationsAsync(
        int pageSize, string? pageToken, CancellationToken cancellationToken)
    {
        return await _refitConfigurationClient
            .GetConfigurationsAsync(pageSize, pageToken, cancellationToken)
            .ConfigureAwait(false);
    }
}