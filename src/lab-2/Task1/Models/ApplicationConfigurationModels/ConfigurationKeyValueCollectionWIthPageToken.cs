namespace Task1.Models.ApplicationConfigurationModels;

public record ConfigurationKeyValueCollectionWIthPageToken(
    IEnumerable<ConfigurationKeyValueItem> ConfigurationKeyValueItems,
    string? PageToken);