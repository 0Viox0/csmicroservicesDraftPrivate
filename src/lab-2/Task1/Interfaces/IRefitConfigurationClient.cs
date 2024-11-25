using Refit;
using Task1.Models.ClientResponseModels;

namespace Task1.Interfaces;

public interface IRefitConfigurationClient
{
    [Get("/configurations")]
    Task<ExternalQueryConfigurationsResponse> GetConfigurationsAsync(
        [Query] int pageSize,
        [Query] string? pageToken,
        CancellationToken cancellationToken);
}