using Task1.Models;

namespace Task1.Interfaces;

public interface IConfigurationClient
{
    Task<QueryConfigurationsResponse> GetConfigurationsAsync(
        int pageSize, string? pageToken, CancellationToken cancellationToken);
}