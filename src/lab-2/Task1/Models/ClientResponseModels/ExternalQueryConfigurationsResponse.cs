namespace Task1.Models.ClientResponseModels;

public record ExternalQueryConfigurationsResponse(IEnumerable<ExternalConfigurationItem> Items, string? PageToken);
