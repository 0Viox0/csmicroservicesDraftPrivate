using Task1.Models.ApplicationConfigurationModels;
using Task1.Models.ClientResponseModels;

namespace Task1.Mappers;

public class ApiApplicationModelConfigurationMapper
{
    public ConfigurationKeyValueItem ToConfigurationKeyValueItem(ExternalConfigurationItem externalConfiguration)
    {
        return new ConfigurationKeyValueItem(externalConfiguration.Key, externalConfiguration.Value);
    }

    public ConfigurationKeyValueCollectionWIthPageToken ToConfigurationKeyValueCollectionWIthPageToken(
        ExternalQueryConfigurationsResponse externalQueryConfigurations)
    {
        return new ConfigurationKeyValueCollectionWIthPageToken(
            externalQueryConfigurations.Items.Select(ToConfigurationKeyValueItem),
            externalQueryConfigurations.PageToken);
    }
}