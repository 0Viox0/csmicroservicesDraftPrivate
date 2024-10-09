using Refit;
using Task1.Models;

namespace Task1.Interfaces;

public interface IRefitConfigurationClient
{
    [Get("/configurations")]
    Task<QueryConfigurationsResponse> GetConfigurationsAsync(
        [Query] int pageSize,
        [Query] string? pageToken,
        CancellationToken cancellationToken);
}