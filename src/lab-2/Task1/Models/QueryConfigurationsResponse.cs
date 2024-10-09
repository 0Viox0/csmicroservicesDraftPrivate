namespace Task1.Models;

public record QueryConfigurationsResponse(IEnumerable<ConfigurationItemDto> Items, string? PageToken);
